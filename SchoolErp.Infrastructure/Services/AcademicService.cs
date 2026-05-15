using Dapper;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Queries;
using SchoolErp.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace SchoolErp.Infrastructure.Services;

public class AcademicService : IAcademicService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMemoryCache _cache;

    public AcademicService(IDbConnectionFactory connectionFactory, IMemoryCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<bool> CreateClassAsync(string className, string section, string? teacherId, string isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.InsertClass, new
        {
            CLASS_ID = Guid.NewGuid().ToString(),
            CLASS_NAME = className,
            SECTION = section,
            TEACHER_ID = teacherId,
            IS_ACTIVE = isActive
        });
        if (result > 0) _cache.Remove("DynamicList_Classes");
        return result > 0;
    }

    public async Task<bool> UpdateClassAsync(string classId, string className, string section, string? teacherId, string isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.UpdateClass, new
        {
            CLASS_ID = classId,
            CLASS_NAME = className,
            SECTION = section,
            TEACHER_ID = teacherId,
            IS_ACTIVE = isActive
        });
        if (result > 0) _cache.Remove("DynamicList_Classes");
        return result > 0;
    }

    public async Task<bool> DeleteClassAsync(string classId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteClass, new { CLASS_ID = classId });
        if (result > 0) _cache.Remove("DynamicList_Classes");
        return result > 0;
    }

    public async Task<bool> SaveSupportEntryAsync(string tableName, string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql = tableName switch
        {
            "Gender" => AcademicQueries.InsertGender,
            "StaffCategory" => AcademicQueries.InsertStaffCategory,
            "Nationality" => AcademicQueries.InsertNationality,
            _ => throw new ArgumentException("Invalid table name")
        };

        var result = await connection.ExecuteAsync(sql, new
        {
            GENDER_ID = Guid.NewGuid().ToString(),
            CATEGORY_ID = Guid.NewGuid().ToString(),
            NATIONALITY_ID = Guid.NewGuid().ToString(),
            GENDER_NAME = name,
            CATEGORY_NAME = name,
            NATIONALITY_NAME = name
        });
        return result > 0;
    }

    public async Task<bool> DeleteSupportEntryAsync(string tableName, string id)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql = tableName switch
        {
            "Gender" => AcademicQueries.DeleteGender,
            "StaffCategory" => AcademicQueries.DeleteStaffCategory,
            "Nationality" => AcademicQueries.DeleteNationality,
            _ => throw new ArgumentException("Invalid table name")
        };

        var result = await connection.ExecuteAsync(sql, new
        {
            GENDER_ID = id,
            CATEGORY_ID = id,
            NATIONALITY_ID = id
        });
        return result > 0;
    }

    public async Task<List<SchoolErp.Application.Common.Models.DropdownItem>> GetDynamicListAsync(string tableName, string idColumn, string nameColumn)
    {
        string cacheKey = $"DynamicList_{tableName}";
        if (_cache.TryGetValue(cacheKey, out List<DropdownItem>? cachedList))
        {
            return cachedList!;
        }

        ValidateTableName(tableName);
        ValidateTableName(idColumn);
        ValidateTableName(nameColumn);

        string sql = AcademicQueries.DynamicFetch
            .Replace("{TableName}", tableName)
            .Replace("{IdColumn}", idColumn)
            .Replace("{NameColumn}", nameColumn);

        using var connection = _connectionFactory.CreateConnection();
        var result = (await connection.QueryAsync<SchoolErp.Application.Common.Models.DropdownItem>(sql)).ToList();
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
        return result;
    }

    public async Task<bool> SaveDynamicEntryAsync(string tableName, string idColumn, string nameColumn, string name)
    {
        ValidateTableName(tableName);
        ValidateTableName(idColumn);
        ValidateTableName(nameColumn);

        string sql = AcademicQueries.DynamicInsert
            .Replace("{TableName}", tableName)
            .Replace("{IdColumn}", idColumn)
            .Replace("{NameColumn}", nameColumn);

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), Name = name });
        if (result > 0) _cache.Remove($"DynamicList_{tableName}");
        return result > 0;
    }

    public async Task<bool> UpdateDynamicEntryAsync(string tableName, string idColumn, string nameColumn, string id, string name)
    {
        ValidateTableName(tableName);
        ValidateTableName(idColumn);
        ValidateTableName(nameColumn);

        string sql = AcademicQueries.DynamicUpdate
            .Replace("{TableName}", tableName)
            .Replace("{IdColumn}", idColumn)
            .Replace("{NameColumn}", nameColumn);

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(sql, new { Id = id, Name = name });
        if (result > 0) _cache.Remove($"DynamicList_{tableName}");
        return result > 0;
    }

    public async Task<bool> DeleteDynamicEntryAsync(string tableName, string idColumn, string id)
    {
        ValidateTableName(tableName);
        ValidateTableName(idColumn);

        string sql = AcademicQueries.DynamicDelete
            .Replace("{TableName}", tableName)
            .Replace("{IdColumn}", idColumn);

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        if (result > 0) _cache.Remove($"DynamicList_{tableName}");
        return result > 0;
    }

    public async Task<bool> SaveMenuAsync(string menuName, string url, string icon, int order, string isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.InsertMenu, new
        {
            MENU_ID = Guid.NewGuid().ToString(),
            MENU_NAME = menuName,
            MENU_URL = url,
            ICON = icon,
            PARENT_MENU_ID = (string?)null,
            DISPLAY_ORDER = order,
            IS_ACTIVE = isActive == "1" ? 1 : 0
        });
        return result > 0;
    }

    public async Task<bool> DeleteMenuAsync(string menuId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteMenu, new { MENU_ID = menuId });
        return result > 0;
    }

    public async Task<bool> CreateAcademicYearAsync(string yearName, string fromDate, string toDate, string isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.InsertAcademicYear, new
        {
            ACADEMIC_YEAR_ID = Guid.NewGuid().ToString(),
            ACADEMIC_YEAR = yearName,
            FROM_DATE = fromDate,
            TO_DATE = toDate,
            IS_ACTIVE = isActive == "1" ? 1 : 0
        });
        return result > 0;
    }

    public async Task<bool> UpdateAcademicYearAsync(string yearId, string yearName, string fromDate, string toDate, string isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.UpdateAcademicYear, new
        {
            ACADEMIC_YEAR_ID = yearId,
            ACADEMIC_YEAR = yearName,
            FROM_DATE = fromDate,
            TO_DATE = toDate,
            IS_ACTIVE = isActive == "1" ? 1 : 0
        });
        return result > 0;
    }

    public async Task<bool> SetActiveAcademicYearAsync(string yearId)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(AcademicQueries.SetAllYearsInactive, null, transaction);
            var result = await connection.ExecuteAsync(AcademicQueries.SetYearActive, new { ACADEMIC_YEAR_ID = yearId }, transaction);
            transaction.Commit();
            return result > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAcademicYearAsync(string yearId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteAcademicYear, new { ACADEMIC_YEAR_ID = yearId });
        return result > 0;
    }

    // Subjects
    public async Task<List<SubjectModel>> GetSubjectsAsync()
    {
        const string cacheKey = "Academic_Subjects";
        if (_cache.TryGetValue(cacheKey, out List<SubjectModel>? subjects))
        {
            return subjects!;
        }

        using var connection = _connectionFactory.CreateConnection();
        var result = (await connection.QueryAsync<SubjectModel>(AcademicQueries.GetAllSubjects)).ToList();
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    public async Task<bool> SaveSubjectAsync(SubjectModel subject)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql = string.IsNullOrEmpty(subject.SUBJECT_ID) ? AcademicQueries.InsertSubject : AcademicQueries.UpdateSubject;
        if (string.IsNullOrEmpty(subject.SUBJECT_ID)) subject.SUBJECT_ID = Guid.NewGuid().ToString();
        var result = await connection.ExecuteAsync(sql, subject);
        if (result > 0) _cache.Remove("Academic_Subjects");
        return result > 0;
    }

    public async Task<bool> DeleteSubjectAsync(string subjectId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteSubject, new { SUBJECT_ID = subjectId });
        if (result > 0) _cache.Remove("Academic_Subjects");
        return result > 0;
    }

    // Mappings
    public async Task<List<StudentClassMappingModel>> GetStudentMappingsAsync(string classId, string academicYearId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<StudentClassMappingModel>(AcademicQueries.GetStudentMappings, new { CLASS_ID = classId, ACADEMIC_YEAR_ID = academicYearId });
        return result.ToList();
    }

    public async Task<bool> SaveStudentMappingAsync(string studentId, string classId, string academicYearId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.InsertStudentMapping, new
        {
            STU_CLASS_ID = Guid.NewGuid().ToString(),
            STUDENT_ID = studentId,
            CLASS_ID = classId,
            ACADEMIC_YEAR_ID = academicYearId
        });
        return result > 0;
    }

    public async Task<bool> BulkStudentMappingAsync(List<string> studentIds, string classId, string academicYearId)
    {
        if (studentIds == null || !studentIds.Any()) return true;

        const int batchSize = 200;
        int totalAffected = 0;

        for (int i = 0; i < studentIds.Count; i += batchSize)
        {
            var batch = studentIds.Skip(i).Take(batchSize).ToList();
            using var connection = _connectionFactory.CreateConnection();
            var sqlBuilder = new System.Text.StringBuilder();
            sqlBuilder.Append("INSERT INTO STU_CLASS (STU_CLASS_ID, STUDENT_ID, CLASS_ID, ACADEMIC_YEAR_ID, REGISTER_NO, ROLL_NO, IS_LEFT, IS_ACTIVE, IS_DELETED, CREATED_AT) VALUES ");
            
            var parameters = new DynamicParameters();
            for (int j = 0; j < batch.Count; j++)
            {
                var studentId = batch[j];
                sqlBuilder.Append($"(@Id{j}, @Stu{j}, @Class{j}, @Year{j}, '', '', 0, 1, 0, CURRENT_TIMESTAMP)");
                if (j < batch.Count - 1) sqlBuilder.Append(", ");
                
                parameters.Add($"Id{j}", Guid.NewGuid().ToString());
                parameters.Add($"Stu{j}", studentId);
                parameters.Add($"Class{j}", classId);
                parameters.Add($"Year{j}", academicYearId);
            }
            
            sqlBuilder.Append(@" ON DUPLICATE KEY UPDATE 
                CLASS_ID = VALUES(CLASS_ID), 
                ACADEMIC_YEAR_ID = VALUES(ACADEMIC_YEAR_ID), 
                IS_DELETED = 0,
                UPDATED_AT = CURRENT_TIMESTAMP;");

            totalAffected += await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
        }

        return totalAffected > 0;
    }

    public async Task<bool> DeleteStudentMappingAsync(string mappingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteStudentMapping, new { STU_CLASS_ID = mappingId });
        return result > 0;
    }

    public async Task<List<ClassSubjectStaffModel>> GetSubjectStaffMappingsAsync(string classId, string academicYearId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ClassSubjectStaffModel>(AcademicQueries.GetSubjectStaffMappings, new { CLASS_ID = classId, ACADEMIC_YEAR_ID = academicYearId });
        return result.ToList();
    }

    public async Task<bool> SaveSubjectStaffMappingAsync(ClassSubjectStaffModel mapping)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (string.IsNullOrEmpty(mapping.CLASS_SUBJECT_STAFF_ID)) mapping.CLASS_SUBJECT_STAFF_ID = Guid.NewGuid().ToString();
        var result = await connection.ExecuteAsync(AcademicQueries.InsertSubjectStaffMapping, mapping);
        return result > 0;
    }

    public async Task<bool> DeleteSubjectStaffMappingAsync(string mappingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteSubjectStaffMapping, new { CLASS_SUBJECT_STAFF_ID = mappingId });
        return result > 0;
    }

    public async Task<List<StaffDirectoryModel>> GetStaffDirectoryAsync(string? categoryId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        string filter = string.IsNullOrEmpty(categoryId) ? "" : " AND s.CATEGORY_ID = @CategoryId";
        string sql = AcademicQueries.GetStaffDirectory.Replace("{CategoryFilter}", filter);
        var result = await connection.QueryAsync<StaffDirectoryModel>(sql, new { CategoryId = categoryId });
        return result.ToList();
    }

    public async Task<bool> SaveStaffAsync(StaffDirectoryModel staff)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql = string.IsNullOrEmpty(staff.STAFF_ID) ? AcademicQueries.InsertStaff : AcademicQueries.UpdateStaff;
        if (string.IsNullOrEmpty(staff.STAFF_ID)) staff.STAFF_ID = Guid.NewGuid().ToString();
        var result = await connection.ExecuteAsync(sql, staff);
        return result > 0;
    }

    public async Task<bool> DeleteStaffAsync(string staffId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(AcademicQueries.DeleteStaff, new { STAFF_ID = staffId });
        return result > 0;
    }

    public async Task<bool> BulkUpdateStaffAsync(List<StaffDirectoryModel> staffList)
    {
        if (staffList == null || !staffList.Any()) return true;

        const int batchSize = 200;
        int totalAffected = 0;

        for (int i = 0; i < staffList.Count; i += batchSize)
        {
            var batch = staffList.Skip(i).Take(batchSize).ToList();
            using var connection = _connectionFactory.CreateConnection();
            var sqlBuilder = new System.Text.StringBuilder();
            sqlBuilder.Append("INSERT INTO STAFF_INFO (STAFF_ID, NAME, STAFF_CODE, DEPARTMENT, IS_ACTIVE, IS_DELETED, CREATED_AT) VALUES ");
            
            var parameters = new DynamicParameters();
            for (int j = 0; j < batch.Count; j++)
            {
                var staff = batch[j];
                sqlBuilder.Append($"(@Id{j}, @Name{j}, @Code{j}, @Dept{j}, @Active{j}, 0, CURRENT_TIMESTAMP)");
                if (j < batch.Count - 1) sqlBuilder.Append(", ");
                
                parameters.Add($"Id{j}", staff.STAFF_ID);
                parameters.Add($"Name{j}", staff.NAME);
                parameters.Add($"Code{j}", staff.STAFF_CODE);
                parameters.Add($"Dept{j}", staff.DEPARTMENT);
                parameters.Add($"Active{j}", staff.IS_ACTIVE);
            }
            
            sqlBuilder.Append(@" ON DUPLICATE KEY UPDATE 
                NAME = VALUES(NAME), 
                STAFF_CODE = VALUES(STAFF_CODE), 
                DEPARTMENT = VALUES(DEPARTMENT), 
                IS_ACTIVE = VALUES(IS_ACTIVE), 
                UPDATED_AT = CURRENT_TIMESTAMP;");

            totalAffected += await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
        }

        return totalAffected > 0;
    }

    public async Task<List<StudentDirectoryModel>> GetStudentDirectoryAsync(string? classId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        string filter = string.IsNullOrEmpty(classId) ? "" : " AND m.CLASS_ID = @ClassId";
        string sql = AcademicQueries.GetStudentDirectory.Replace("{ClassFilter}", filter);
        var result = await connection.QueryAsync<StudentDirectoryModel>(sql, new { ClassId = classId });
        return result.ToList();
    }

    public async Task<bool> BulkUpdateStudentsAsync(List<StudentDirectoryModel> students)
    {
        if (students == null || !students.Any()) return true;

        const int batchSize = 200;
        int totalAffected = 0;

        for (int i = 0; i < students.Count; i += batchSize)
        {
            var batch = students.Skip(i).Take(batchSize).ToList();
            using var connection = _connectionFactory.CreateConnection();
            var sqlBuilder = new System.Text.StringBuilder();
            sqlBuilder.Append("INSERT INTO STUDENTS_INFO (STUDENT_ID, NAME, ADMISSION_NO, FATHER_NAME, MOTHER_NAME, IS_ACTIVE, IS_DELETED, CREATED_AT, MOBILE_NO, ADDRESS, EMAIL_ID, GENDER_ID, DATE_OF_BIRTH) VALUES ");
            
            var parameters = new DynamicParameters();
            for (int j = 0; j < batch.Count; j++)
            {
                var student = batch[j];
                sqlBuilder.Append($"(@Id{j}, @Name{j}, @Adm{j}, @Father{j}, @Mother{j}, @Active{j}, 0, CURRENT_TIMESTAMP, '', '', '', '', '1900-01-01')");
                if (j < batch.Count - 1) sqlBuilder.Append(", ");
                
                parameters.Add($"Id{j}", student.STUDENT_ID);
                parameters.Add($"Name{j}", student.NAME);
                parameters.Add($"Adm{j}", student.ADMISSION_NO);
                parameters.Add($"Father{j}", student.FATHER_NAME);
                parameters.Add($"Mother{j}", student.MOTHER_NAME);
                parameters.Add($"Active{j}", student.IS_ACTIVE);
            }
            
            sqlBuilder.Append(@" ON DUPLICATE KEY UPDATE 
                NAME = VALUES(NAME), 
                ADMISSION_NO = VALUES(ADMISSION_NO), 
                FATHER_NAME = VALUES(FATHER_NAME), 
                MOTHER_NAME = VALUES(MOTHER_NAME), 
                IS_ACTIVE = VALUES(IS_ACTIVE), 
                UPDATED_AT = CURRENT_TIMESTAMP;");

            totalAffected += await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
        }

        return totalAffected > 0;
    }

    private void ValidateTableName(string name)
    {
        // Simple regex to allow only alphanumeric and underscores to prevent SQL injection
        if (string.IsNullOrEmpty(name) || !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$"))
        {
            throw new ArgumentException("Invalid database object name.");
        }
    }
}
