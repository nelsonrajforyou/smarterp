using Dapper;
using FluentValidation;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Features.Students;

// CREATE COMMAND
public class CreateStudentCommand : IRequest<string>
{
    public string Name { get; set; } = default!;
    public string MobileNo { get; set; } = default!;
    public string FatherName { get; set; } = default!;
    public string MotherName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string AdmissionNo { get; set; } = default!;
    public string EmailId { get; set; } = default!;
    public string GenderId { get; set; } = default!;
    public string DateOfBirth { get; set; } = default!;
    public string? StateId { get; set; }
    public string? NationalityId { get; set; }
    public string? UserId { get; set; }
    
    // Class related
    public string ClassId { get; set; } = default!;
    public string AcademicYearId { get; set; } = default!;
    public string RegisterNo { get; set; } = default!;
    public string RollNo { get; set; } = default!;
}

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MobileNo).NotEmpty();
        RuleFor(x => x.AdmissionNo).NotEmpty();
        RuleFor(x => x.GenderId).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.AcademicYearId).NotEmpty();
    }
}

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, string>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CreateStudentCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var studentId = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Insert into STUDENTS_INFO
            await connection.ExecuteAsync(StudentQueries.InsertStudent, new 
            {
                STUDENT_ID = studentId,
                NAME = request.Name,
                MOBILE_NO = request.MobileNo,
                FATHER_NAME = request.FatherName,
                MOTHER_NAME = request.MotherName,
                ADDRESS = request.Address,
                ADMISSION_NO = request.AdmissionNo,
                EMAIL_ID = request.EmailId,
                GENDER_ID = request.GenderId,
                DATE_OF_BIRTH = request.DateOfBirth,
                STATE_ID = request.StateId,
                USER_ID = request.UserId,
                NATIONALITY_ID = request.NationalityId,
                IS_ACTIVE = "1",
                IS_DELETED = "0",
                CREATED_AT = createdAt
            }, transaction);

            // 2. Insert into STU_CLASS
            await connection.ExecuteAsync(StudentQueries.InsertStudentClass, new 
            {
                STU_CLASS_ID = Guid.NewGuid().ToString(),
                STUDENT_ID = studentId,
                CLASS_ID = request.ClassId,
                ACADEMIC_YEAR_ID = request.AcademicYearId,
                REGISTER_NO = request.RegisterNo,
                ROLL_NO = request.RollNo,
                IS_ACTIVE = "1",
                IS_DELETED = "0",
                CREATED_AT = createdAt
            }, transaction);

            transaction.Commit();
            return studentId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

// UPDATE COMMAND
public class UpdateStudentCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string MobileNo { get; set; } = default!;
    public string FatherName { get; set; } = default!;
    public string MotherName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string AdmissionNo { get; set; } = default!;
    public string EmailId { get; set; } = default!;
    public string GenderId { get; set; } = default!;
    public string DateOfBirth { get; set; } = default!;
    public string? StateId { get; set; }
    public string? NationalityId { get; set; }
    
    // Class related (updating current active class)
    public string ClassId { get; set; } = default!;
    public string RegisterNo { get; set; } = default!;
    public string RollNo { get; set; } = default!;
}

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateStudentCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Update STUDENTS_INFO
            var studentAffected = await connection.ExecuteAsync(StudentQueries.UpdateStudent, new 
            { 
                STUDENT_ID = request.Id, 
                NAME = request.Name, 
                MOBILE_NO = request.MobileNo,
                FATHER_NAME = request.FatherName,
                MOTHER_NAME = request.MotherName,
                ADDRESS = request.Address,
                ADMISSION_NO = request.AdmissionNo,
                EMAIL_ID = request.EmailId,
                GENDER_ID = request.GenderId,
                DATE_OF_BIRTH = request.DateOfBirth,
                STATE_ID = request.StateId,
                NATIONALITY_ID = request.NationalityId,
                UPDATED_AT = updatedAt 
            }, transaction);

            // 2. Update STU_CLASS
            await connection.ExecuteAsync(StudentQueries.UpdateStudentClass, new 
            { 
                STUDENT_ID = request.Id, 
                CLASS_ID = request.ClassId, 
                REGISTER_NO = request.RegisterNo,
                ROLL_NO = request.RollNo,
                UPDATED_AT = updatedAt 
            }, transaction);

            transaction.Commit();
            return studentAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

// DELETE COMMAND
public class DeleteStudentCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
}

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeleteStudentCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var studentAffected = await connection.ExecuteAsync(StudentQueries.DeleteStudent, new 
            { 
                STUDENT_ID = request.Id, 
                UPDATED_AT = updatedAt 
            }, transaction);

            await connection.ExecuteAsync(StudentQueries.DeleteStudentClass, new 
            { 
                STUDENT_ID = request.Id, 
                UPDATED_AT = updatedAt 
            }, transaction);

            transaction.Commit();
            return studentAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
