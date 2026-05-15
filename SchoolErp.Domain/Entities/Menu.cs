namespace SchoolErp.Domain.Entities;

public class Menu
{
    public string MENU_ID { get; set; } = Guid.NewGuid().ToString();
    public string MENU_NAME { get; set; } = default!;
    public string MENU_URL { get; set; } = default!;
    public string? ICON { get; set; }
    public string? PARENT_MENU_ID { get; set; }
    public string DISPLAY_ORDER { get; set; } = "0";
    public string IS_ACTIVE { get; set; } = "1";
    public string IS_DELETED { get; set; } = "0";
    public string CREATED_AT { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
