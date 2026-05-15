using Dapper;
using FluentValidation;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Attendance;

public class SaveAttendanceCommand : IRequest<bool>
{
    public string ClassId { get; set; } = default!;
    public string AttendanceDate { get; set; } = default!;
    public string EntryId { get; set; } = default!; // User ID of the person entering data
    public List<StudentAttendanceUpdateDto> Students { get; set; } = new();
}

public class SaveAttendanceCommandValidator : AbstractValidator<SaveAttendanceCommand>
{
    public SaveAttendanceCommandValidator()
    {
        RuleFor(v => v.ClassId)
            .NotEmpty().WithMessage("Class ID is required.");
            
        RuleFor(v => v.AttendanceDate)
            .NotEmpty().WithMessage("Attendance Date is required.");

        RuleFor(v => v.EntryId)
            .NotEmpty().WithMessage("Entry ID is required.");
    }
}

public class SaveAttendanceCommandHandler : IRequestHandler<SaveAttendanceCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SaveAttendanceCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(SaveAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (request.Students == null || !request.Students.Any()) return true;

        using var connection = _connectionFactory.CreateConnection();
        var sqlBuilder = new System.Text.StringBuilder();
        sqlBuilder.Append("INSERT INTO stu_attendance_entries (STU_ATTENDANCE_ID, STUDENT_ID, ATTENDANCE_DATE, ATTENDANCE_TYPE_ID, ENTRY_ID, IS_ACTIVE, IS_DELETED, CREATED_AT) VALUES ");
        
        var parameters = new DynamicParameters();
        for (int i = 0; i < request.Students.Count; i++)
        {
            var student = request.Students[i];
            sqlBuilder.Append($"(@Id{i}, @Stu{i}, @Date{i}, @Type{i}, @Entry{i}, '1', '0', CURRENT_TIMESTAMP)");
            if (i < request.Students.Count - 1) sqlBuilder.Append(", ");
            
            parameters.Add($"Id{i}", Guid.NewGuid().ToString());
            parameters.Add($"Stu{i}", student.STUDENT_ID);
            parameters.Add($"Date{i}", request.AttendanceDate);
            parameters.Add($"Type{i}", student.ATTENDANCE_TYPE_ID);
            parameters.Add($"Entry{i}", request.EntryId);
        }
        
        sqlBuilder.Append(@" ON DUPLICATE KEY UPDATE 
            ATTENDANCE_TYPE_ID = VALUES(ATTENDANCE_TYPE_ID), 
            ENTRY_ID = VALUES(ENTRY_ID), 
            UPDATED_AT = CURRENT_TIMESTAMP,
            IS_DELETED = 0;");

        var result = await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
        return result > 0;
    }
}
