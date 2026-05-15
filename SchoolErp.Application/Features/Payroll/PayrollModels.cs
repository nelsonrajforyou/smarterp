using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Payroll;

// ── Models ─────────────────────────────────────────────────────────────────

public class StaffSalaryDto
{
    public string SalaryId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string ClassHandling { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string StaffType { get; set; } = "Teaching";
    public int YearsExperience { get; set; }
    public decimal PrevGrossSalary { get; set; }
    public decimal IncrementAmount { get; set; }
    public decimal GrossSalary { get; set; }
    public string AcademicYearId { get; set; } = string.Empty;
}

public class PayrollDetailDto
{
    public string DetailId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string ClassHandling { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string StaffType { get; set; } = "Teaching";
    public int YearsExperience { get; set; }

    // Salary Structure
    public decimal GrossSalary { get; set; }
    public decimal PrevGrossSalary { get; set; }
    public decimal IncrementAmount { get; set; }
    public decimal BasicPay { get; set; }
    public decimal DA { get; set; }
    public decimal BasicDA { get; set; }
    public decimal HRA { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowance { get; set; }

    // Deductions
    public decimal EPF { get; set; }
    public decimal ESI { get; set; }

    // Loss of Pay
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LopDays { get; set; }
    public decimal LopAmount { get; set; }

    public decimal NetSalary { get; set; }
}

public class PayrollMonthDto
{
    public string PayrollMonthId { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string Status { get; set; } = "Draft";
    public string StaffType { get; set; } = "All";
    public string AcademicYearId { get; set; } = string.Empty;
    public List<PayrollDetailDto> Details { get; set; } = new();
}

public class AllowanceConfigDto
{
    public string ConfigId { get; set; } = string.Empty;
    public string AcademicYearId { get; set; } = string.Empty;
    public decimal BasicPayPct { get; set; } = 40;
    public decimal DAPct { get; set; } = 20;
    public decimal HRAPct { get; set; } = 10;
    public decimal MedicalPct { get; set; } = 10;
    public decimal OtherAllowancePct { get; set; } = 20;
    public decimal EPFPct { get; set; } = 12;
    public decimal ESIPct { get; set; } = 0.75m;
    public int DefaultWorkingDays { get; set; } = 26;
}

public class IncrementHistoryDto
{
    public string IncrementId { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public decimal OldGrossSalary { get; set; }
    public decimal IncrementAmount { get; set; }
    public decimal NewGrossSalary { get; set; }
    public string EffectiveDate { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

// ── Form Models ─────────────────────────────────────────────────────────────

public class ProcessPayrollForm
{
    public string AcademicYearId { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string StaffType { get; set; } = "All";
    public int WorkingDays { get; set; } = 26;
    public List<StaffLopEntry> LopEntries { get; set; } = new();
}

public class StaffLopEntry
{
    public string StaffId { get; set; } = string.Empty;
    public int PresentDays { get; set; }
}

public class SetSalaryForm
{
    public string StaffId { get; set; } = string.Empty;
    public string AcademicYearId { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal IncrementAmount { get; set; }
}

public class BulkIncrementForm
{
    public string AcademicYearId { get; set; } = string.Empty;
    public string StaffType { get; set; } = "All";
    public decimal IncrementAmount { get; set; }
    public string EffectiveDate { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public List<string> StaffIds { get; set; } = new(); // empty = all staff
}
