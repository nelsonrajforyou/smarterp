namespace SchoolErp.Application.Common.Queries;

public static class CommonQueries
{
    public const string FetchStudents = @"
        SELECT STUDENT_ID as Value, NAME as Text 
        FROM STUDENTS_INFO 
        WHERE IS_DELETED = 0 AND IS_ACTIVE = 1 
        ORDER BY NAME;";

    public const string FetchStaff = @"
        SELECT STAFF_ID as Value, NAME as Text 
        FROM STAFF_INFO 
        WHERE IS_DELETED = 0 AND IS_ACTIVE = 1 
        ORDER BY NAME;";

    public const string FetchClasses = @"
        SELECT CLASS_ID as Value, CONCAT(CLASS_NAME, ' - ', SECTION) as Text 
        FROM CLASSES 
        WHERE IS_DELETED = 0 AND IS_ACTIVE = 1 
        ORDER BY CLASS_NAME, SECTION;";

    public const string FetchAcademicYears = @"
        SELECT ACADEMIC_YEAR_ID as Value, ACADEMIC_YEAR as Text 
        FROM ACADEMIC_YEAR 
        WHERE IS_ACTIVE = 1 
        ORDER BY FROM_DATE DESC;";

    public const string FetchGenders = @"
        SELECT GENDER_ID as Value, GENDER_NAME as Text 
        FROM SUP_GENDER 
        ORDER BY GENDER_NAME;";

    public const string FetchStaffCategories = @"
        SELECT CATEGORY_ID as Value, CATEGORY_NAME as Text 
        FROM STAFF_CATEGORY 
        WHERE IS_DELETED = 0 AND IS_ACTIVE = 1 
        ORDER BY CATEGORY_NAME;";
}
