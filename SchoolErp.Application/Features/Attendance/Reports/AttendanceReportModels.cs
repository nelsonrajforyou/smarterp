using System;
using System.Collections.Generic;
using SchoolErp.Application.Common.Mediator;

namespace SchoolErp.Application.Features.Attendance.Reports;

public class GenerateAttendanceReportQuery : IRequest<AttendanceReportResponse>
{
    public string ReportType { get; set; } = default!;
    public string? ClassId { get; set; }
    public DateTime TargetDate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class AttendanceReportResponse
{
    public string ReportTitle { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    // Daily Attendance Report Data
    public List<DailyAttendanceRow>? DailyReport { get; set; }
    
    // Monthly Register Data
    public MonthlyRegisterData? MonthlyRegister { get; set; }
    
    // Defaulters / Continuous Absentees Data
    public List<DefaulterRow>? DefaultersReport { get; set; }
}

public class DailyAttendanceRow
{
    public string AdmissionNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

public class MonthlyRegisterData
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int DaysInMonth { get; set; }
    public List<string> Holidays { get; set; } = new();
    public List<MonthlyRegisterRow> Rows { get; set; } = new();
}

public class MonthlyRegisterRow
{
    public string AdmissionNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    // Dictionary mapping day of month (1-31) to status code (P, A, L, HD)
    public Dictionary<int, string> DailyStatus { get; set; } = new();
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
}

public class DefaulterRow
{
    public string AdmissionNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public double Percentage { get; set; }
}
