using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace SchoolErp.Application.Features.Students;

public class ReportResponse
{
    public byte[] Content { get; set; } = System.Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class GenerateStudentReportQuery : IRequest<ReportResponse>
{
    public string? SearchTerm { get; set; }
    public List<string> SelectedColumns { get; set; } = new();
    public string ExportType { get; set; } = "Excel";
}

public class GenerateStudentReportQueryHandler : IRequestHandler<GenerateStudentReportQuery, ReportResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public GenerateStudentReportQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<ReportResponse> Handle(GenerateStudentReportQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch data using the same dynamic logic
        var result = await _mediator.Send(new GetDynamicStudentsQuery 
        { 
            SearchTerm = request.SearchTerm,
            PageNumber = 1,
            PageSize = 100000, // Fetch all for report
            SelectedColumns = request.SelectedColumns
        });

        var columns = request.SelectedColumns.Where(c => c != "STUDENT_ID").ToList();
        var data = result.Items;

        if (request.ExportType == "Excel")
        {
            return GenerateExcel(columns, data);
        }
        else
        {
            return GeneratePdf(columns, data);
        }
    }

    private ReportResponse GenerateExcel(List<string> columns, List<IDictionary<string, object>> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        // Headers
        for (int i = 0; i < columns.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = columns[i].Replace("_", " ");
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4f46e5");
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data
        for (int r = 0; r < data.Count; r++)
        {
            var row = data[r];
            for (int c = 0; c < columns.Count; c++)
            {
                var colName = columns[c];
                // Check if display field exists
                var val = row.ContainsKey($"{colName}_DISPLAY") ? row[$"{colName}_DISPLAY"] : (row.ContainsKey(colName) ? row[colName] : "");
                worksheet.Cell(r + 2, c + 1).Value = val?.ToString() ?? "";
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ReportResponse
        {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"StudentReport_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx"
        };
    }

    private ReportResponse GeneratePdf(List<string> columns, List<IDictionary<string, object>> data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Inter"));

                page.Header().Text("Student Report").FontSize(20).SemiBold().FontColor(Colors.Indigo.Medium);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(tableColumns =>
                    {
                        for (int i = 0; i < columns.Count; i++) tableColumns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        foreach (var col in columns)
                        {
                            header.Cell().Background(Colors.Indigo.Medium).Padding(5).Text(col.Replace("_", " ")).FontColor(Colors.White).SemiBold();
                        }
                    });

                    // Body
                    foreach (var row in data)
                    {
                        foreach (var col in columns)
                        {
                            var val = row.ContainsKey($"{col}_DISPLAY") ? row[$"{col}_DISPLAY"] : (row.ContainsKey(col) ? row[col] : "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(val?.ToString() ?? "");
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);

        return new ReportResponse
        {
            Content = stream.ToArray(),
            ContentType = "application/pdf",
            FileName = $"StudentReport_{System.DateTime.Now:yyyyMMddHHmmss}.pdf"
        };
    }
}
