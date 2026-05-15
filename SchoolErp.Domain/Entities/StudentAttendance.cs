using System;

namespace SchoolErp.Domain.Entities;

public class StudentAttendance
{
    public string STU_ATTENDANCE_ID { get; set; } = Guid.NewGuid().ToString();
    public string STUDENT_ID { get; set; } = default!;
    public string ATTENDANCE_DATE { get; set; } = default!;
    public string ATTENDANCE_TYPE_ID { get; set; } = default!;
    public string ENTRY_ID { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string? UPDATED_AT { get; set; }
}
