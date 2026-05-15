using System;

namespace SchoolErp.Domain.Entities;

public class AttendanceType
{
    public string ATTENDANCE_TYPE_ID { get; set; } = Guid.NewGuid().ToString();
    public string ATTENDANCE_TYPE_NAME { get; set; } = default!;
    public string ATTENDANCE_TYPE_CODE { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
}
