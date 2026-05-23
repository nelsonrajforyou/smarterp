using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Mediator;

namespace SchoolErp.Application.Features.Attendance.Reports;

public class GenerateAttendanceReportQueryHandler : IRequestHandler<GenerateAttendanceReportQuery, AttendanceReportResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GenerateAttendanceReportQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AttendanceReportResponse> Handle(GenerateAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var response = new AttendanceReportResponse
        {
            ReportTitle = GetReportTitle(request),
            ClassName = await GetClassNameAsync(connection, request.ClassId),
            GeneratedAt = DateTime.Now
        };

        switch (request.ReportType)
        {
            case "DailyAttendance":
                response.DailyReport = await GetDailyAttendanceAsync(connection, request);
                break;
            case "MonthlyRegister":
                response.MonthlyRegister = await GetMonthlyRegisterAsync(connection, request);
                break;
            case "Defaulters":
                response.DefaultersReport = await GetDefaultersAsync(connection, request, 75); // 75% threshold
                break;
            case "ContinuousAbsentees":
                response.DefaultersReport = await GetContinuousAbsenteesAsync(connection, request, 3); // 3+ days
                break;
        }

        return response;
    }

    private string GetReportTitle(GenerateAttendanceReportQuery request)
    {
        return request.ReportType switch
        {
            "DailyAttendance" => $"Daily Attendance Report - {request.TargetDate:dd MMM yyyy}",
            "MonthlyRegister" => $"Monthly Attendance Register - {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(request.Month)} {request.Year}",
            "Defaulters" => "Defaulters List (Below 75%)",
            "ContinuousAbsentees" => "Continuous Absentees List (3+ days)",
            _ => "Attendance Report"
        };
    }

    private async Task<string> GetClassNameAsync(System.Data.IDbConnection connection, string? classId)
    {
        if (string.IsNullOrEmpty(classId)) return "All Classes";
        
        var sql = "SELECT CONCAT(CLASS_NAME, ' - ', SECTION) FROM CLASSES WHERE CLASS_ID = @ClassId AND IS_DELETED = 0";
        return await connection.ExecuteScalarAsync<string>(sql, new { ClassId = classId }) ?? "Unknown Class";
    }

    private async Task<List<DailyAttendanceRow>> GetDailyAttendanceAsync(System.Data.IDbConnection connection, GenerateAttendanceReportQuery request)
    {
        var sql = @"
            SELECT 
                s.ADMISSION_NO as AdmissionNo,
                s.NAME as StudentName,
                COALESCE(t.ATTENDANCE_TYPE_NAME, 'Not Marked') as Status,
                '' as Remarks
            FROM STUDENTS_INFO s
            JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_DELETED = 0
            LEFT JOIN stu_attendance_entries a ON s.STUDENT_ID = a.STUDENT_ID AND a.ATTENDANCE_DATE = @Date AND a.IS_DELETED = 0
            LEFT JOIN sup_attendance_type t ON a.ATTENDANCE_TYPE_ID = t.ATTENDANCE_TYPE_ID
            WHERE (@ClassId IS NULL OR sc.CLASS_ID = @ClassId) AND s.IS_DELETED = 0
            ORDER BY s.NAME;";

        var result = await connection.QueryAsync<DailyAttendanceRow>(sql, new { Date = request.TargetDate.ToString("yyyy-MM-dd"), ClassId = string.IsNullOrEmpty(request.ClassId) ? null : request.ClassId });
        return result.ToList();
    }

    private async Task<MonthlyRegisterData> GetMonthlyRegisterAsync(System.Data.IDbConnection connection, GenerateAttendanceReportQuery request)
    {
        int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var startDate = new DateTime(request.Year, request.Month, 1).ToString("yyyy-MM-dd");
        var endDate = new DateTime(request.Year, request.Month, daysInMonth).ToString("yyyy-MM-dd");

        var sql = @"
            SELECT 
                s.ADMISSION_NO, s.NAME as StudentName,
                DAY(a.ATTENDANCE_DATE) as DayOfMonth,
                t.ATTENDANCE_TYPE_CODE as StatusCode
            FROM STUDENTS_INFO s
            JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_DELETED = 0
            LEFT JOIN stu_attendance_entries a ON s.STUDENT_ID = a.STUDENT_ID 
                 AND a.ATTENDANCE_DATE >= @StartDate AND a.ATTENDANCE_DATE <= @EndDate AND a.IS_DELETED = 0
            LEFT JOIN sup_attendance_type t ON a.ATTENDANCE_TYPE_ID = t.ATTENDANCE_TYPE_ID
            WHERE (@ClassId IS NULL OR sc.CLASS_ID = @ClassId) AND s.IS_DELETED = 0
            ORDER BY s.NAME, a.ATTENDANCE_DATE;";

        var entries = await connection.QueryAsync(sql, new { 
            StartDate = startDate, 
            EndDate = endDate, 
            ClassId = string.IsNullOrEmpty(request.ClassId) ? null : request.ClassId 
        });

        var data = new MonthlyRegisterData
        {
            Month = request.Month,
            Year = request.Year,
            DaysInMonth = daysInMonth
        };

        var grouped = entries.GroupBy(e => new { e.ADMISSION_NO, e.StudentName });
        foreach (var g in grouped)
        {
            var row = new MonthlyRegisterRow
            {
                AdmissionNo = g.Key.ADMISSION_NO,
                StudentName = g.Key.StudentName
            };

            foreach (var entry in g)
            {
                if (entry.DayOfMonth != null && entry.StatusCode != null)
                {
                    row.DailyStatus[(int)entry.DayOfMonth] = (string)entry.StatusCode;
                    if ((string)entry.StatusCode == "P") row.TotalPresent++;
                    if ((string)entry.StatusCode == "A") row.TotalAbsent++;
                }
            }
            data.Rows.Add(row);
        }

        return data;
    }

    private async Task<List<DefaulterRow>> GetDefaultersAsync(System.Data.IDbConnection connection, GenerateAttendanceReportQuery request, double threshold)
    {
        // Calculate based on the last 30 days up to TargetDate
        var endDate = request.TargetDate;
        var startDate = endDate.AddDays(-30);

        var sql = @"
            SELECT 
                s.ADMISSION_NO as AdmissionNo,
                s.NAME as StudentName,
                SUM(CASE WHEN t.ATTENDANCE_TYPE_CODE = 'P' THEN 1 ELSE 0 END) as DaysPresent,
                SUM(CASE WHEN t.ATTENDANCE_TYPE_CODE = 'A' THEN 1 ELSE 0 END) as DaysAbsent,
                COUNT(a.STU_ATTENDANCE_ID) as TotalWorkingDays
            FROM STUDENTS_INFO s
            JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_DELETED = 0
            LEFT JOIN stu_attendance_entries a ON s.STUDENT_ID = a.STUDENT_ID 
                 AND a.ATTENDANCE_DATE >= @StartDate AND a.ATTENDANCE_DATE <= @EndDate AND a.IS_DELETED = 0
            LEFT JOIN sup_attendance_type t ON a.ATTENDANCE_TYPE_ID = t.ATTENDANCE_TYPE_ID
            WHERE (@ClassId IS NULL OR sc.CLASS_ID = @ClassId) AND s.IS_DELETED = 0
            GROUP BY s.ADMISSION_NO, s.NAME
            HAVING TotalWorkingDays > 0;";

        var results = await connection.QueryAsync<DefaulterRow>(sql, new { 
            StartDate = startDate.ToString("yyyy-MM-dd"), 
            EndDate = endDate.ToString("yyyy-MM-dd"), 
            ClassId = string.IsNullOrEmpty(request.ClassId) ? null : request.ClassId 
        });

        var defaulters = new List<DefaulterRow>();
        foreach (var r in results)
        {
            if (r.TotalWorkingDays > 0)
            {
                r.Percentage = Math.Round((double)r.DaysPresent / r.TotalWorkingDays * 100, 1);
                if (r.Percentage < threshold)
                {
                    defaulters.Add(r);
                }
            }
        }

        return defaulters.OrderBy(d => d.Percentage).ToList();
    }

    private async Task<List<DefaulterRow>> GetContinuousAbsenteesAsync(System.Data.IDbConnection connection, GenerateAttendanceReportQuery request, int days)
    {
        // Simple heuristic: get students whose last N records are 'A'
        // For accurate tracking, this requires complex window functions, but for this implementation we can check the last N days.
        var endDate = request.TargetDate;
        var startDate = endDate.AddDays(-14);

        var sql = @"
            SELECT 
                s.ADMISSION_NO,
                s.NAME as StudentName,
                a.ATTENDANCE_DATE,
                t.ATTENDANCE_TYPE_CODE
            FROM STUDENTS_INFO s
            JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_DELETED = 0
            JOIN stu_attendance_entries a ON s.STUDENT_ID = a.STUDENT_ID 
                 AND a.ATTENDANCE_DATE >= @StartDate AND a.ATTENDANCE_DATE <= @EndDate AND a.IS_DELETED = 0
            JOIN sup_attendance_type t ON a.ATTENDANCE_TYPE_ID = t.ATTENDANCE_TYPE_ID
            WHERE (@ClassId IS NULL OR sc.CLASS_ID = @ClassId) AND s.IS_DELETED = 0
            ORDER BY s.STUDENT_ID, a.ATTENDANCE_DATE DESC;";

        var entries = await connection.QueryAsync(sql, new { 
            StartDate = startDate.ToString("yyyy-MM-dd"), 
            EndDate = endDate.ToString("yyyy-MM-dd"), 
            ClassId = string.IsNullOrEmpty(request.ClassId) ? null : request.ClassId 
        });

        var absentees = new List<DefaulterRow>();
        var grouped = entries.GroupBy(e => new { e.ADMISSION_NO, e.StudentName });

        foreach (var g in grouped)
        {
            int consecutiveAbsences = 0;
            foreach (var entry in g) // Already ordered by date desc
            {
                if ((string)entry.ATTENDANCE_TYPE_CODE == "A")
                {
                    consecutiveAbsences++;
                }
                else
                {
                    break; // Streak broken
                }
            }

            if (consecutiveAbsences >= days)
            {
                absentees.Add(new DefaulterRow
                {
                    AdmissionNo = g.Key.ADMISSION_NO,
                    StudentName = g.Key.StudentName,
                    DaysAbsent = consecutiveAbsences,
                    TotalWorkingDays = consecutiveAbsences, // Used here to display streak length
                    Percentage = 0
                });
            }
        }

        return absentees.OrderByDescending(d => d.DaysAbsent).ToList();
    }
}
