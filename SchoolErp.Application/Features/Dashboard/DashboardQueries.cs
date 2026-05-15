namespace SchoolErp.Application.Features.Dashboard;

public static class DashboardQueries
{
    public const string GetDashboardStats = @"
        SELECT 
            CAST((SELECT COUNT(*) FROM STUDENTS_INFO WHERE IS_DELETED = 0) AS CHAR) AS TOTAL_STUDENTS,
            CAST((SELECT COUNT(*) FROM STAFF_INFO WHERE IS_DELETED = 0) AS CHAR) AS TOTAL_TEACHERS,
            CAST((SELECT COUNT(*) FROM CLASSES WHERE IS_DELETED = 0) AS CHAR) AS TOTAL_CLASSES;";
}
