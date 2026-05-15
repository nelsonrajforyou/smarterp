using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Common.Interfaces;

public interface IAcademicService
{
    // Classes
    Task<bool> CreateClassAsync(string className, string section, string? teacherId, string isActive);
    Task<bool> UpdateClassAsync(string classId, string className, string section, string? teacherId, string isActive);
    Task<bool> DeleteClassAsync(string classId);

    // Support Tables (Specific)
    Task<bool> SaveSupportEntryAsync(string tableName, string name);
    Task<bool> DeleteSupportEntryAsync(string tableName, string id);

    // Dynamic Support Tables (Generic)
    Task<List<DropdownItem>> GetDynamicListAsync(string tableName, string idColumn, string nameColumn);
    Task<bool> SaveDynamicEntryAsync(string tableName, string idColumn, string nameColumn, string name);
    Task<bool> UpdateDynamicEntryAsync(string tableName, string idColumn, string nameColumn, string id, string name);
    Task<bool> DeleteDynamicEntryAsync(string tableName, string idColumn, string id);

    // Menu Management
    Task<bool> SaveMenuAsync(string menuName, string url, string icon, int order, string isActive);
    Task<bool> DeleteMenuAsync(string menuId);

    // Academic Year
    Task<bool> CreateAcademicYearAsync(string yearName, string fromDate, string toDate, string isActive);
    Task<bool> UpdateAcademicYearAsync(string yearId, string yearName, string fromDate, string toDate, string isActive);
    Task<bool> DeleteAcademicYearAsync(string yearId);
    Task<bool> SetActiveAcademicYearAsync(string yearId);
    // Subjects
    Task<List<SubjectModel>> GetSubjectsAsync();
    Task<bool> SaveSubjectAsync(SubjectModel subject);
    Task<bool> DeleteSubjectAsync(string subjectId);

    // Mappings
    Task<List<StudentClassMappingModel>> GetStudentMappingsAsync(string classId, string academicYearId);
    Task<bool> SaveStudentMappingAsync(string studentId, string classId, string academicYearId);
    Task<bool> BulkStudentMappingAsync(List<string> studentIds, string classId, string academicYearId);
    Task<bool> DeleteStudentMappingAsync(string mappingId);

    Task<List<ClassSubjectStaffModel>> GetSubjectStaffMappingsAsync(string classId, string academicYearId);
    Task<bool> SaveSubjectStaffMappingAsync(ClassSubjectStaffModel mapping);
    Task<bool> DeleteSubjectStaffMappingAsync(string mappingId);

    // Directories
    Task<List<StaffDirectoryModel>> GetStaffDirectoryAsync(string? categoryId = null);
    Task<bool> SaveStaffAsync(StaffDirectoryModel staff);
    Task<bool> DeleteStaffAsync(string staffId);
    Task<bool> BulkUpdateStaffAsync(List<StaffDirectoryModel> staffList);
    
    Task<List<StudentDirectoryModel>> GetStudentDirectoryAsync(string? classId = null);
    Task<bool> BulkUpdateStudentsAsync(List<StudentDirectoryModel> students);
}
