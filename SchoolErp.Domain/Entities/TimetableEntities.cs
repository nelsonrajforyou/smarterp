namespace SchoolErp.Domain.Entities;

public class Subject
{
    public string SUBJECT_ID { get; set; } = Guid.NewGuid().ToString();
    public string SUBJECT_CODE { get; set; } = default!;
    public string SUBJECT_NAME { get; set; } = default!;
    public string SUBJECT_TYPE_ID { get; set; } = default!;
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
    public DateTime CREATED_AT { get; set; } = DateTime.UtcNow;
    public DateTime? UPDATED_AT { get; set; }
}

public class Room
{
    public string ROOM_ID { get; set; } = Guid.NewGuid().ToString();
    public string ROOM_NAME { get; set; } = default!;
    public string ROOM_TYPE { get; set; } = "Classroom";
    public int CAPACITY { get; set; } = 40;
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
}

public class TimetableConfig
{
    public string CONFIG_ID { get; set; } = Guid.NewGuid().ToString();
    public string ACADEMIC_YEAR_ID { get; set; } = default!;
    public int DAYS_PER_WEEK { get; set; } = 5;
    public int PERIODS_PER_DAY { get; set; } = 8;
    public int PERIOD_DURATION_MINS { get; set; } = 45;
    public DateTime? DAY_ORDER_START_DATE { get; set; }
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
}

public class Period
{
    public string PERIOD_ID { get; set; } = Guid.NewGuid().ToString();
    public string CONFIG_ID { get; set; } = default!;
    public int PERIOD_NUMBER { get; set; } = 1;
    public TimeOnly START_TIME { get; set; }
    public TimeOnly END_TIME { get; set; }
    public int IS_BREAK { get; set; } = 0;
    public string? LABEL { get; set; }
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
}

public class ClassSubjectStaff
{
    public string CLASS_SUBJECT_STAFF_ID { get; set; } = Guid.NewGuid().ToString();
    public string STAFF_ID { get; set; } = default!;
    public string CLASS_ID { get; set; } = default!;
    public string SUBJECT_ID { get; set; } = default!;
    public string ACADEMIC_YEAR_ID { get; set; } = default!;
    public int MAX_HR_PER_WEEK { get; set; } = 5;
    public int IS_CLASS_INCHARGE { get; set; } = 1;
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
    public DateTime CREATED_AT { get; set; } = DateTime.UtcNow;
    public DateTime? UPDATED_AT { get; set; }
}

public class TimetableEntry
{
    public string ENTRY_ID { get; set; } = Guid.NewGuid().ToString();
    public string CLASS_ID { get; set; } = default!;
    public string PERIOD_ID { get; set; } = default!;
    public int DAY_OF_WEEK { get; set; } = 1;
    public string? SUBJECT_ID { get; set; }
    public string? TEACHER_ID { get; set; }
    public int IS_SUBSTITUTE { get; set; } = 0;
    public string? ORIGINAL_TEACHER_ID { get; set; }
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
}
public class DayOrder
{
    public string DAY_ID { get; set; } = Guid.NewGuid().ToString();
    public int DAY_ORDER { get; set; }
    public string DAY_ORDER_NAME { get; set; } = default!;
    public string? REMARK { get; set; }
}
