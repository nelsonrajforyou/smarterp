namespace SchoolErp.Application.Common.Queries;

public static class AcademicQueries
{
    public const string GetAllClasses = @"
        SELECT CLASS_ID, CLASS_NAME, SECTION, TEACHER_ID, 
               CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE, CAST(IS_DELETED AS CHAR) as IS_DELETED, 
               CAST(CREATED_AT AS CHAR) as CREATED_AT 
        FROM CLASSES 
        WHERE IS_DELETED <> 1 
        ORDER BY CLASS_NAME, SECTION;";

    public const string GetAllClassesWithStaff = @"
        SELECT c.CLASS_ID, c.CLASS_NAME, c.SECTION, c.TEACHER_ID, s.NAME as TeacherName,
               CAST(c.IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM CLASSES c
        LEFT JOIN STAFF_INFO s ON c.TEACHER_ID = s.STAFF_ID
        WHERE c.IS_DELETED <> 1 
        ORDER BY c.CLASS_NAME, c.SECTION;";

    public const string GetAllAcademicYears = @"
        SELECT ACADEMIC_YEAR_ID, ACADEMIC_YEAR, 
               CAST(FROM_DATE AS CHAR) as FROM_DATE, CAST(TO_DATE AS CHAR) as TO_DATE, 
               CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM ACADEMIC_YEAR 
        ORDER BY FROM_DATE DESC;";

    public const string GetGenders = "SELECT GENDER_ID, GENDER_NAME FROM SUP_GENDER ORDER BY GENDER_NAME;";
    
    public const string GetNationalities = "SELECT NATIONALITY_ID, NATIONALITY_NAME FROM SUP_NATIONALITY ORDER BY NATIONALITY_NAME;";
    
    public const string GetStaffCategories = "SELECT CATEGORY_ID, CATEGORY_NAME, CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE FROM STAFF_CATEGORY WHERE IS_DELETED <> 1 ORDER BY CATEGORY_NAME;";

    // Classes CRUD
    public const string InsertClass = @"
        INSERT INTO CLASSES (CLASS_ID, CLASS_NAME, SECTION, TEACHER_ID, IS_ACTIVE, IS_DELETED, CREATED_AT)
        VALUES (@CLASS_ID, @CLASS_NAME, @SECTION, @TEACHER_ID, @IS_ACTIVE, 0, CURRENT_TIMESTAMP);";

    public const string UpdateClass = @"
        UPDATE CLASSES SET CLASS_NAME = @CLASS_NAME, SECTION = @SECTION, TEACHER_ID = @TEACHER_ID, IS_ACTIVE = @IS_ACTIVE, UPDATED_AT = CURRENT_TIMESTAMP
        WHERE CLASS_ID = @CLASS_ID;";

    public const string DeleteClass = "UPDATE CLASSES SET IS_DELETED = 1, UPDATED_AT = CURRENT_TIMESTAMP WHERE CLASS_ID = @CLASS_ID;";

    // Support Tables CRUD
    public const string InsertGender = "INSERT INTO SUP_GENDER (GENDER_ID, GENDER_NAME) VALUES (@GENDER_ID, @GENDER_NAME);";
    public const string DeleteGender = "DELETE FROM SUP_GENDER WHERE GENDER_ID = @GENDER_ID;";

    public const string InsertStaffCategory = "INSERT INTO STAFF_CATEGORY (CATEGORY_ID, CATEGORY_NAME) VALUES (@CATEGORY_ID, @CATEGORY_NAME);";
    public const string DeleteStaffCategory = "UPDATE STAFF_CATEGORY SET IS_DELETED = 1 WHERE CATEGORY_ID = @CATEGORY_ID;";

    public const string InsertNationality = "INSERT INTO SUP_NATIONALITY (NATIONALITY_ID, NATIONALITY_NAME) VALUES (@NATIONALITY_ID, @NATIONALITY_NAME);";
    public const string DeleteNationality = "DELETE FROM SUP_NATIONALITY WHERE NATIONALITY_ID = @NATIONALITY_ID;";

    // Dynamic Support Table Queries (Placeholders for {TableName} and {ColumnName})
    public const string DynamicFetch = "SELECT {IdColumn} as Value, {NameColumn} as Text FROM {TableName} ORDER BY {NameColumn};";
    public const string DynamicInsert = "INSERT INTO {TableName} ({IdColumn}, {NameColumn}) VALUES (@Id, @Name);";
    public const string DynamicUpdate = "UPDATE {TableName} SET {NameColumn} = @Name WHERE {IdColumn} = @Id;";
    public const string DynamicDelete = "DELETE FROM {TableName} WHERE {IdColumn} = @Id;";

    // Menu Management Queries
    public const string GetAllMenus = @"
        SELECT MENU_ID, MENU_NAME, MENU_URL, ICON, PARENT_MENU_ID, DISPLAY_ORDER, 
               CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM MENUS 
        WHERE IS_DELETED = 0 
        ORDER BY DISPLAY_ORDER;";

    public const string InsertMenu = @"
        INSERT INTO MENUS (MENU_ID, MENU_NAME, MENU_URL, ICON, PARENT_MENU_ID, DISPLAY_ORDER, IS_ACTIVE, IS_DELETED)
        VALUES (@MENU_ID, @MENU_NAME, @MENU_URL, @ICON, @PARENT_MENU_ID, @DISPLAY_ORDER, @IS_ACTIVE, 0);";

    public const string DeleteMenu = "UPDATE MENUS SET IS_DELETED = 1 WHERE MENU_ID = @MENU_ID;";

    // Academic Year CRUD
    public const string InsertAcademicYear = @"
        INSERT INTO ACADEMIC_YEAR (ACADEMIC_YEAR_ID, ACADEMIC_YEAR, FROM_DATE, TO_DATE, IS_ACTIVE)
        VALUES (@ACADEMIC_YEAR_ID, @ACADEMIC_YEAR, @FROM_DATE, @TO_DATE, @IS_ACTIVE);";

    public const string UpdateAcademicYear = @"
        UPDATE ACADEMIC_YEAR 
        SET ACADEMIC_YEAR = @ACADEMIC_YEAR, FROM_DATE = @FROM_DATE, TO_DATE = @TO_DATE, IS_ACTIVE = @IS_ACTIVE
        WHERE ACADEMIC_YEAR_ID = @ACADEMIC_YEAR_ID;";

    public const string SetAllYearsInactive = "UPDATE ACADEMIC_YEAR SET IS_ACTIVE = 0;";
    
    public const string SetYearActive = "UPDATE ACADEMIC_YEAR SET IS_ACTIVE = 1 WHERE ACADEMIC_YEAR_ID = @ACADEMIC_YEAR_ID;";

    public const string DeleteAcademicYear = "DELETE FROM ACADEMIC_YEAR WHERE ACADEMIC_YEAR_ID = @ACADEMIC_YEAR_ID;";

    // Subject List
    public const string GetAllSubjects = "SELECT SUBJECT_ID, SUBJECT_CODE, SUBJECT_NAME, SUBJECT_TYPE_ID, CAST(IS_ACTIVE AS CHAR) as IS_ACTIVE FROM SUBJECT_LIST WHERE IS_DELETED = 0;";
    public const string InsertSubject = "INSERT INTO SUBJECT_LIST (SUBJECT_ID, SUBJECT_CODE, SUBJECT_NAME, SUBJECT_TYPE_ID, IS_ACTIVE) VALUES (@SUBJECT_ID, @SUBJECT_CODE, @SUBJECT_NAME, @SUBJECT_TYPE_ID, @IS_ACTIVE);";
    public const string UpdateSubject = "UPDATE SUBJECT_LIST SET SUBJECT_CODE = @SUBJECT_CODE, SUBJECT_NAME = @SUBJECT_NAME, SUBJECT_TYPE_ID = @SUBJECT_TYPE_ID, IS_ACTIVE = @IS_ACTIVE, UPDATED_AT = CURRENT_TIMESTAMP WHERE SUBJECT_ID = @SUBJECT_ID;";
    public const string DeleteSubject = "UPDATE SUBJECT_LIST SET IS_DELETED = 1 WHERE SUBJECT_ID = @SUBJECT_ID;";

    // Student Class Mapping (stu_class)
    public const string GetStudentMappings = @"
        SELECT m.STU_CLASS_ID, m.STUDENT_ID, u.USER_NAME as STUDENT_NAME, m.CLASS_ID, m.ACADEMIC_YEAR_ID 
        FROM STU_CLASS m 
        JOIN USER_INFO u ON m.STUDENT_ID = u.USER_ID 
        WHERE m.CLASS_ID = @CLASS_ID AND m.ACADEMIC_YEAR_ID = @ACADEMIC_YEAR_ID AND m.IS_DELETED = 0;";
    public const string InsertStudentMapping = "INSERT INTO STU_CLASS (STU_CLASS_ID, STUDENT_ID, CLASS_ID, ACADEMIC_YEAR_ID) VALUES (@STU_CLASS_ID, @STUDENT_ID, @CLASS_ID, @ACADEMIC_YEAR_ID);";
    public const string DeleteStudentMapping = "UPDATE STU_CLASS SET IS_DELETED = 1 WHERE STU_CLASS_ID = @STU_CLASS_ID;";

    // Class Subject Staff Mapping
    public const string GetSubjectStaffMappings = @"
        SELECT m.CLASS_SUBJECT_STAFF_ID, m.STAFF_ID, u.NAME as STAFF_NAME, m.CLASS_ID, c.CLASS_NAME, 
               m.SUBJECT_ID, s.SUBJECT_NAME, m.ACADEMIC_YEAR_ID, m.IS_CLASS_INCHARGE, 
               CAST(m.IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM CLASS_SUBJECT_STAFF m 
        JOIN STAFF_INFO u ON m.STAFF_ID = u.STAFF_ID 
        JOIN CLASSES c ON m.CLASS_ID = c.CLASS_ID 
        JOIN SUBJECT_LIST s ON m.SUBJECT_ID = s.SUBJECT_ID 
        WHERE m.CLASS_ID = @CLASS_ID AND m.ACADEMIC_YEAR_ID = @ACADEMIC_YEAR_ID AND m.IS_DELETED <> 1;";
    public const string InsertSubjectStaffMapping = @"
        INSERT INTO CLASS_SUBJECT_STAFF (CLASS_SUBJECT_STAFF_ID, STAFF_ID, CLASS_ID, SUBJECT_ID, ACADEMIC_YEAR_ID, IS_CLASS_INCHARGE, IS_ACTIVE) 
        VALUES (@CLASS_SUBJECT_STAFF_ID, @STAFF_ID, @CLASS_ID, @SUBJECT_ID, @ACADEMIC_YEAR_ID, @IS_CLASS_INCHARGE, @IS_ACTIVE);";
    public const string DeleteSubjectStaffMapping = "UPDATE CLASS_SUBJECT_STAFF SET IS_DELETED = 1 WHERE CLASS_SUBJECT_STAFF_ID = @CLASS_SUBJECT_STAFF_ID;";

    // Directories
    public const string GetStaffDirectory = @"
        SELECT s.STAFF_ID, s.NAME, s.STAFF_CODE, s.DATE_OF_BIRTH, s.DEPARTMENT, s.DEPARTMEMT_ID,
               s.BLOOD_ID, s.MOBILE, s.PHONE, s.ADDRESS, s.CATEGORY_ID, sc.CATEGORY_NAME,
               s.QUALIFICATION_ID, s.DESIGNATION_ID, sd.DESIGANTION_NAME as DESIGNATION_NAME,
               CAST(s.IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM STAFF_INFO s 
        LEFT JOIN STAFF_CATEGORY sc ON s.CATEGORY_ID = sc.CATEGORY_ID
        LEFT JOIN sup_designation sd ON s.DESIGNATION_ID = sd.DESIGANTION_ID
        WHERE s.IS_DELETED = 0 
        {CategoryFilter}
        ORDER BY s.NAME;";

    public const string BulkUpdateStaffDirectory = @"
        UPDATE STAFF_INFO 
        SET NAME = @NAME, STAFF_CODE = @STAFF_CODE, DEPARTMENT = @DEPARTMENT, 
            IS_ACTIVE = @IS_ACTIVE, UPDATED_AT = CURRENT_TIMESTAMP
        WHERE STAFF_ID = @STAFF_ID;";

    public const string InsertStaff = @"
        INSERT INTO STAFF_INFO (STAFF_ID, NAME, STAFF_CODE, DATE_OF_BIRTH, DEPARTMENT, DEPARTMEMT_ID, BLOOD_ID, MOBILE, PHONE, ADDRESS, CATEGORY_ID, QUALIFICATION_ID, DESIGNATION_ID, IS_ACTIVE) 
        VALUES (@STAFF_ID, @NAME, @STAFF_CODE, @DATE_OF_BIRTH, @DEPARTMENT, @DEPARTMEMT_ID, @BLOOD_ID, @MOBILE, @PHONE, @ADDRESS, @CATEGORY_ID, @QUALIFICATION_ID, @DESIGNATION_ID, @IS_ACTIVE);";

    public const string UpdateStaff = @"
        UPDATE STAFF_INFO 
        SET NAME = @NAME, STAFF_CODE = @STAFF_CODE, DATE_OF_BIRTH = @DATE_OF_BIRTH, DEPARTMENT = @DEPARTMENT, DEPARTMEMT_ID = @DEPARTMEMT_ID, 
            BLOOD_ID = @BLOOD_ID, MOBILE = @MOBILE, PHONE = @PHONE, ADDRESS = @ADDRESS, CATEGORY_ID = @CATEGORY_ID, 
            QUALIFICATION_ID = @QUALIFICATION_ID, DESIGNATION_ID = @DESIGNATION_ID, IS_ACTIVE = @IS_ACTIVE, UPDATED_AT = CURRENT_TIMESTAMP
        WHERE STAFF_ID = @STAFF_ID;";

    public const string DeleteStaff = "UPDATE STAFF_INFO SET IS_DELETED = 1 WHERE STAFF_ID = @STAFF_ID;";

    public const string GetStudentDirectory = @"
        SELECT s.STUDENT_ID, s.NAME, s.ADMISSION_NO, c.CLASS_NAME, s.FATHER_NAME, s.MOTHER_NAME, 
               CAST(s.IS_ACTIVE AS CHAR) as IS_ACTIVE 
        FROM STUDENTS_INFO s 
        LEFT JOIN STU_CLASS m ON s.STUDENT_ID = m.STUDENT_ID AND m.IS_DELETED = 0
        LEFT JOIN CLASSES c ON m.CLASS_ID = c.CLASS_ID
        WHERE s.IS_DELETED = 0
        {ClassFilter}
        ORDER BY s.NAME;";

    public const string BulkUpdateStudentDirectory = @"
        UPDATE STUDENTS_INFO 
        SET NAME = @NAME, ADMISSION_NO = @ADMISSION_NO, FATHER_NAME = @FATHER_NAME, 
            MOTHER_NAME = @MOTHER_NAME, IS_ACTIVE = @IS_ACTIVE, UPDATED_AT = CURRENT_TIMESTAMP
        WHERE STUDENT_ID = @STUDENT_ID;";
}
