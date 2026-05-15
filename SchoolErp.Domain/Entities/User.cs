namespace SchoolErp.Domain.Entities;

public class User
{
    public string USER_INFO_ID { get; set; } = Guid.NewGuid().ToString();
    public string EMAIL { get; set; } = default!;
    public string PASSWORD_HASH { get; set; } = default!;
    public string ROLE_ID { get; set; } = default!;
    public string USER_ID { get; set; } = default!; // Staff/Student ID
    public string FIRST_NAME { get; set; } = default!;
    public string LAST_NAME { get; set; } = default!;
    public string STATUS { get; set; } = "Active";
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string? UPDATED_AT { get; set; }
}
