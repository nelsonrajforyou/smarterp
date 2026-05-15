namespace SchoolErp.Domain.Entities;

public class Module
{
    public string PARENT_ID { get; set; } = Guid.NewGuid().ToString();
    public string MODULE_NAME { get; set; } = default!;
    public string? ICON { get; set; }
    public int DISPLAY_ORDER { get; set; } = 0;
    public int IS_ACTIVE { get; set; } = 1;
    public int IS_DELETED { get; set; } = 0;
    public DateTime CREATED_AT { get; set; } = DateTime.UtcNow;
}
