namespace SchoolErp.Application.Features.Attendance;

public static class AttendanceQueries
{
    public const string GetAttendanceTypes = @"
        SELECT ATTENDANCE_TYPE_ID, ATTENDANCE_TYPE_NAME, ATTENDANCE_TYPE_CODE,
               CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE
        FROM sup_attendance_type
        WHERE IS_DELETED = 0
        ORDER BY ATTENDANCE_TYPE_NAME;";

    public const string GetStudentsForAttendance = @"
        SELECT s.STUDENT_ID, s.NAME, s.ADMISSION_NO,
               COALESCE(a.ATTENDANCE_TYPE_ID, '') as ATTENDANCE_TYPE_ID,
               COALESCE(a.STU_ATTENDANCE_ID, '') as STU_ATTENDANCE_ID
        FROM STUDENTS_INFO s
        JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_DELETED = 0
        LEFT JOIN stu_attendance_entries a ON s.STUDENT_ID = a.STUDENT_ID 
             AND a.ATTENDANCE_DATE = @AttendanceDate AND a.IS_DELETED = 0
        WHERE sc.CLASS_ID = @ClassId AND s.IS_DELETED = 0
        ORDER BY s.NAME;";

    public const string UpsertAttendance = @"
        INSERT INTO stu_attendance_entries 
        (STU_ATTENDANCE_ID, STUDENT_ID, ATTENDANCE_DATE, ATTENDANCE_TYPE_ID, ENTRY_ID, IS_ACTIVE, IS_DELETED, CREATED_AT)
        VALUES (@STU_ATTENDANCE_ID, @STUDENT_ID, @ATTENDANCE_DATE, @ATTENDANCE_TYPE_ID, @ENTRY_ID, '1', '0', CURRENT_TIMESTAMP)
        ON DUPLICATE KEY UPDATE 
        ATTENDANCE_TYPE_ID = @ATTENDANCE_TYPE_ID, 
        ENTRY_ID = @ENTRY_ID, 
        UPDATED_AT = CURRENT_TIMESTAMP,
        IS_DELETED = 0;";

    public const string MarkDeleted = @"
        UPDATE stu_attendance_entries 
        SET IS_DELETED = 1, UPDATED_AT = CURRENT_TIMESTAMP 
        WHERE STU_ATTENDANCE_ID = @STU_ATTENDANCE_ID;";
}
