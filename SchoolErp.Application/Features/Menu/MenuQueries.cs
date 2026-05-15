namespace SchoolErp.Application.Features.Menu;

public static class MenuQueries
{
    public const string GetMenusByUserId = @"
        SELECT DISTINCT m.MENU_ID, m.MENU_NAME, m.MENU_URL, m.ICON, m.PARENT_MENU_ID, m.DISPLAY_ORDER
        FROM MENUS m
        INNER JOIN ROLE_MENUS rm ON m.MENU_ID = rm.MENU_ID
        INNER JOIN USER_ROLES ur ON rm.ROLE_ID = ur.ROLE_ID
        WHERE ur.USER_ID IN @UserIds AND m.IS_DELETED = 0 AND m.IS_ACTIVE = 1
        ORDER BY m.DISPLAY_ORDER;";

    public const string GetModulesWithMenusByUserId = @"
        SELECT DISTINCT 
            md.PARENT_ID, md.MODULE_NAME, md.ICON, md.DISPLAY_ORDER,
            m.MENU_ID, m.MENU_NAME, m.MENU_URL, m.ICON, m.PARENT_MENU_ID, m.DISPLAY_ORDER
        FROM modules md
        LEFT JOIN MENUS m ON md.PARENT_ID = m.PARENT_MENU_ID
        INNER JOIN ROLE_MENUS rm ON m.MENU_ID = rm.MENU_ID
        INNER JOIN USER_ROLES ur ON rm.ROLE_ID = ur.ROLE_ID
        WHERE ur.USER_ID IN @UserIds 
          AND md.IS_DELETED = 0 AND md.IS_ACTIVE = 1
          AND m.IS_DELETED = 0 AND m.IS_ACTIVE = 1
        ORDER BY md.DISPLAY_ORDER, m.DISPLAY_ORDER;";

    public const string GetAllMenus = "SELECT MENU_ID, MENU_NAME, MENU_URL, ICON, PARENT_MENU_ID, DISPLAY_ORDER, IS_ACTIVE FROM MENUS WHERE IS_DELETED = 0 ORDER BY DISPLAY_ORDER;";
    
    public const string InsertMenu = "INSERT INTO MENUS (MENU_ID, MENU_NAME, MENU_URL, ICON, PARENT_MENU_ID, DISPLAY_ORDER, IS_ACTIVE) VALUES (@MENU_ID, @MENU_NAME, @MENU_URL, @ICON, @PARENT_MENU_ID, @DISPLAY_ORDER, @IS_ACTIVE);";
    
    public const string UpdateMenu = "UPDATE MENUS SET MENU_NAME = @MENU_NAME, MENU_URL = @MENU_URL, ICON = @ICON, PARENT_MENU_ID = @PARENT_MENU_ID, DISPLAY_ORDER = @DISPLAY_ORDER, IS_ACTIVE = @IS_ACTIVE WHERE MENU_ID = @MENU_ID;";
    
    public const string DeleteMenu = "UPDATE MENUS SET IS_DELETED = 1 WHERE MENU_ID = @MENU_ID;";

    public const string GetRoleMenuAssignments = @"
        SELECT rm.ROLE_ID, r.NAME as ROLE_NAME, rm.MENU_ID, m.MENU_NAME, rm.IS_ACTIVE
        FROM ROLE_MENUS rm
        JOIN ROLES r ON rm.ROLE_ID = r.ROLE_ID
        JOIN MENUS m ON rm.MENU_ID = m.MENU_ID
        WHERE rm.IS_DELETED = 0;";

    public const string AssignRoleMenu = "INSERT INTO ROLE_MENUS (ROLE_ID, MENU_ID, IS_ACTIVE, IS_DELETED) VALUES (@ROLE_ID, @MENU_ID, @IS_ACTIVE, 0);";
    
    public const string UpdateRoleMenu = "UPDATE ROLE_MENUS SET IS_ACTIVE = @IS_ACTIVE WHERE ROLE_ID = @ROLE_ID AND MENU_ID = @MENU_ID;";

    public const string UnassignRoleMenu = "UPDATE ROLE_MENUS SET IS_DELETED = 1 WHERE ROLE_ID = @ROLE_ID AND MENU_ID = @MENU_ID;";
}
