namespace SchoolErp.Domain.Entities;

public class Student
{
    public string STUDENT_ID { get; set; } = Guid.NewGuid().ToString();
    public string NAME { get; set; } = default!;
    public string MOBILE_NO { get; set; } = default!;
    public string FATHER_NAME { get; set; } = default!;
    public string MOTHER_NAME { get; set; } = default!;
    public string ADDRESS { get; set; } = default!;
    public string ADMISSION_NO { get; set; } = default!;
    public string EMAIL_ID { get; set; } = default!;
    public string GENDER_ID { get; set; } = default!;
    public string DATE_OF_BIRTH { get; set; } = default!;
    public string? STATE_ID { get; set; }
    public string? USER_ID { get; set; }
    public string? NATIONALITY_ID { get; set; }
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string? UPDATED_AT { get; set; }
}
