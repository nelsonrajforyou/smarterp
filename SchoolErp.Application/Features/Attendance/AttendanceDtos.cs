namespace SchoolErp.Application.Features.Attendance;

public class AttendanceTypeDto
{
    public string ATTENDANCE_TYPE_ID { get; set; } = default!;
    public string ATTENDANCE_TYPE_NAME { get; set; } = default!;
    public string ATTENDANCE_TYPE_CODE { get; set; } = default!;
}

public class StudentAttendanceDto
{
    public string STUDENT_ID { get; set; } = default!;
    public string NAME { get; set; } = default!;
    public string ADMISSION_NO { get; set; } = default!;
    public string ATTENDANCE_TYPE_ID { get; set; } = default!;
    public string STU_ATTENDANCE_ID { get; set; } = default!;
}

public class SaveAttendanceCommandDto
{
    public string ClassId { get; set; } = default!;
    public string AttendanceDate { get; set; } = default!;
    public List<StudentAttendanceUpdateDto> Students { get; set; } = new();
}

public class StudentAttendanceUpdateDto
{
    public string STUDENT_ID { get; set; } = default!;
    public string ATTENDANCE_TYPE_ID { get; set; } = default!;
}
