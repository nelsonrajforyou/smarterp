namespace SchoolErp.Application.Features.Timetable;

public static class TimetableQueries
{
    // Subjects
    public const string GetSubjects = "SELECT * FROM subject_list WHERE IS_DELETED = 0;";
    public const string InsertSubject = "INSERT INTO subject_list (SUBJECT_ID, SUBJECT_NAME, SUBJECT_CODE, SUBJECT_TYPE_ID, CREATED_AT) VALUES (@SUBJECT_ID, @SUBJECT_NAME, @SUBJECT_CODE, @SUBJECT_TYPE_ID, @CREATED_AT);";

    // Rooms
    public const string GetRooms = "SELECT * FROM ROOMS WHERE IS_DELETED = 0;";
    public const string InsertRoom = "INSERT INTO ROOMS (ROOM_ID, ROOM_NAME, ROOM_TYPE, CAPACITY) VALUES (@ROOM_ID, @ROOM_NAME, @ROOM_TYPE, @CAPACITY);";

    // Config
    public const string GetConfig = "SELECT * FROM TIMETABLE_CONFIG WHERE IS_DELETED = 0 LIMIT 1;";
    public const string InsertConfig = "INSERT INTO TIMETABLE_CONFIG (CONFIG_ID, ACADEMIC_YEAR_ID, DAYS_PER_WEEK, PERIODS_PER_DAY, PERIOD_DURATION_MINS) VALUES (@CONFIG_ID, @ACADEMIC_YEAR_ID, @DAYS_PER_WEEK, @PERIODS_PER_DAY, @PERIOD_DURATION_MINS);";

    // Periods
    public const string GetPeriods = "SELECT * FROM PERIODS WHERE CONFIG_ID = @ConfigId AND IS_DELETED = 0 ORDER BY PERIOD_NUMBER;";
    public const string InsertPeriod = "INSERT INTO PERIODS (PERIOD_ID, CONFIG_ID, PERIOD_NUMBER, START_TIME, END_TIME, IS_BREAK, LABEL) VALUES (@PERIOD_ID, @CONFIG_ID, @PERIOD_NUMBER, @START_TIME, @END_TIME, @IS_BREAK, @LABEL);";

    // Entries
    public const string GetEntries = @"
        SELECT t.*, s.SUBJECT_NAME, st.NAME as TEACHER_NAME, c.CLASS_NAME, c.SECTION
        FROM TIMETABLE_ENTRIES t
        LEFT JOIN subject_list s ON t.SUBJECT_ID = s.SUBJECT_ID
        LEFT JOIN STAFF_INFO st ON t.TEACHER_ID = st.STAFF_ID
        LEFT JOIN CLASSES c ON t.CLASS_ID = c.CLASS_ID
        WHERE t.IS_DELETED = 0;";

    public const string InsertEntry = @"
        INSERT INTO TIMETABLE_ENTRIES (ENTRY_ID, CLASS_ID, PERIOD_ID, DAY_OF_WEEK, SUBJECT_ID, TEACHER_ID)
        VALUES (@ENTRY_ID, @CLASS_ID, @PERIOD_ID, @DAY_OF_WEEK, @SUBJECT_ID, @TEACHER_ID);";

    public const string ClearEntries = "DELETE FROM TIMETABLE_ENTRIES;";

    // Teacher subjects
    // Class Subject Staff
    public const string GetClassSubjectStaff = "SELECT * FROM class_subject_staff WHERE IS_DELETED = 0 AND IS_ACTIVE = 1;";
    public const string InsertClassSubjectStaff = "INSERT INTO class_subject_staff (CLASS_SUBJECT_STAFF_ID, STAFF_ID, CLASS_ID, SUBJECT_ID, ACADEMIC_YEAR_ID, MAX_HR_PER_WEEK, CREATED_AT) VALUES (@CLASS_SUBJECT_STAFF_ID, @STAFF_ID, @CLASS_ID, @SUBJECT_ID, @ACADEMIC_YEAR_ID, @MAX_HR_PER_WEEK, @CREATED_AT);";
}
