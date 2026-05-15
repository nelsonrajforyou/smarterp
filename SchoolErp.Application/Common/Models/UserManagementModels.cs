namespace SchoolErp.Application.Common.Models;

public class ModuleDto
{
    public string PARENT_ID { get; set; } = string.Empty;
    public string MODULE_NAME { get; set; } = string.Empty;
    public string? ICON { get; set; }
    public int DISPLAY_ORDER { get; set; }
    public int IS_ACTIVE { get; set; }
    public List<MenuDto> Menus { get; set; } = new();
}

public class RoleDto
{
    public string ROLE_ID { get; set; } = string.Empty;
    public string NAME { get; set; } = string.Empty;
    public int IS_ACTIVE { get; set; }
    public string CREATED_AT { get; set; } = string.Empty;
}

public class MenuDto
{
    public string MENU_ID { get; set; } = string.Empty;
    public string MENU_NAME { get; set; } = string.Empty;
    public string MENU_URL { get; set; } = string.Empty;
    public string? ICON { get; set; }
    public string? PARENT_MENU_ID { get; set; }
    public int DISPLAY_ORDER { get; set; }
    public int IS_ACTIVE { get; set; }
}

public class RoleMenuDto
{
    public string ROLE_ID { get; set; } = string.Empty;
    public string ROLE_NAME { get; set; } = string.Empty;
    public string MENU_ID { get; set; } = string.Empty;
    public string MENU_NAME { get; set; } = string.Empty;
    public int IS_ACTIVE { get; set; }
}

public class UserRoleDto
{
    public string USER_ID { get; set; } = string.Empty;
    public string USER_NAME { get; set; } = string.Empty;
    public string ROLE_ID { get; set; } = string.Empty;
    public string ROLE_NAME { get; set; } = string.Empty;
    public string ACADEMIC_YEAR_ID { get; set; } = string.Empty;
    public string ACADEMIC_YEAR { get; set; } = string.Empty;
    public int IS_ACTIVE { get; set; }
}
