namespace SchoolErp.Domain.Entities;

public class Role
{
    public string ROLE_ID { get; set; } = Guid.NewGuid().ToString();
    public string NAME { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
