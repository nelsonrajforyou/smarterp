namespace SchoolErp.Application.Common.Models;

public class SubjectModel
{
    public string SUBJECT_ID { get; set; } = "";
    public string SUBJECT_CODE { get; set; } = "";
    public string SUBJECT_NAME { get; set; } = "";
    public string SUBJECT_TYPE_ID { get; set; } = "";
    public string IS_ACTIVE { get; set; } = "1";
}

public class ClassSubjectStaffModel
{
    public string CLASS_SUBJECT_STAFF_ID { get; set; } = "";
    public string STAFF_ID { get; set; } = "";
    public string STAFF_NAME { get; set; } = "";
    public string CLASS_ID { get; set; } = "";
    public string CLASS_NAME { get; set; } = "";
    public string SUBJECT_ID { get; set; } = "";
    public string SUBJECT_NAME { get; set; } = "";
    public string ACADEMIC_YEAR_ID { get; set; } = "";
    public int IS_CLASS_INCHARGE { get; set; } = 0;
    public string IS_ACTIVE { get; set; } = "1";
}

public class StudentClassMappingModel
{
    public string STU_CLASS_ID { get; set; } = "";
    public string STUDENT_ID { get; set; } = "";
    public string STUDENT_NAME { get; set; } = "";
    public string CLASS_ID { get; set; } = "";
    public string ACADEMIC_YEAR_ID { get; set; } = "";
}

public class StaffDirectoryModel
{
    public string STAFF_ID { get; set; } = "";
    public string NAME { get; set; } = "";
    public string STAFF_CODE { get; set; } = "";
    public DateTime? DATE_OF_BIRTH { get; set; }
    public string DEPARTMENT { get; set; } = "";
    public string DEPARTMEMT_ID { get; set; } = ""; // Note: matching typo in SQL 'DEPARTMEMT_ID'
    public string BLOOD_ID { get; set; } = "";
    public string MOBILE { get; set; } = "";
    public string PHONE { get; set; } = "";
    public string ADDRESS { get; set; } = "";
    public string CATEGORY_ID { get; set; } = "";
    public string CATEGORY_NAME { get; set; } = "";
    public string QUALIFICATION_ID { get; set; } = "";
    public string DESIGNATION_ID { get; set; } = "";
    public string DESIGNATION_NAME { get; set; } = "";
    public string IS_ACTIVE { get; set; } = "1";
}

public class StudentDirectoryModel
{
    public string STUDENT_ID { get; set; } = "";
    public string NAME { get; set; } = "";
    public string ADMISSION_NO { get; set; } = "";
    public string CLASS_NAME { get; set; } = "";
    public string FATHER_NAME { get; set; } = "";
    public string MOTHER_NAME { get; set; } = "";
    public string IS_ACTIVE { get; set; } = "1";
}
