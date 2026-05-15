using Dapper;
using FluentValidation;
using SchoolErp.Application.Common.Mediator;
using Microsoft.AspNetCore.Identity;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Features.Users;

// CREATE COMMAND
public class CreateUserCommand : IRequest<string>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string UserId { get; set; } = default!; // Staff/Student ID
    public string Status { get; set; } = "Active";
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IErrorLogger _logger;

    public CreateUserCommandHandler(IDbConnectionFactory connectionFactory, IPasswordHasher<User> passwordHasher, IErrorLogger logger)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            USER_INFO_ID = Guid.NewGuid().ToString(),
            EMAIL = request.Email,
            FIRST_NAME = request.FirstName,
            LAST_NAME = request.LastName,
            ROLE_ID = request.RoleId,
            USER_ID = request.UserId,
            STATUS = request.Status,
            IS_ACTIVE = "1",
            IS_DELETED = "0",
            CREATED_AT = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        user.PASSWORD_HASH = _passwordHasher.HashPassword(user, request.Password);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(UserQueries.InsertUser, user);

        await _logger.LogInfoAsync($"User created: {user.EMAIL}", source: "CreateUserCommandHandler");

        return user.USER_INFO_ID;
    }
}

// UPDATE COMMAND
public class UpdateUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!; // USER_INFO_ID
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string UserId { get; set; } = default!; // Staff/Student ID
    public string Status { get; set; } = default!;
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateUserCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.UpdateUser, new 
        { 
            USER_INFO_ID = request.Id, 
            FIRST_NAME = request.FirstName, 
            LAST_NAME = request.LastName, 
            ROLE_ID = request.RoleId,
            USER_ID = request.UserId,
            STATUS = request.Status, 
            UPDATED_AT = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") 
        });

        return affected > 0;
    }
}

// DELETE COMMAND
public class DeleteUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!; // USER_INFO_ID
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeleteUserCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.DeleteUser, new 
        { 
            USER_INFO_ID = request.Id, 
            UPDATED_AT = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") 
        });

        return affected > 0;
    }
}

// ROLE COMMANDS
public class CreateRoleCommand : IRequest<string>
{
    public string Name { get; set; } = default!;
    public int IsActive { get; set; } = 1;
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, string>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CreateRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(UserQueries.InsertRole, new
        {
            ROLE_ID = id,
            NAME = request.Name,
            IS_ACTIVE = request.IsActive,
            CREATED_AT = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        });
        return id;
    }
}

public class UpdateRoleCommand : IRequest<bool>
{
    public string RoleId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int IsActive { get; set; }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.UpdateRole, new
        {
            ROLE_ID = request.RoleId,
            NAME = request.Name,
            IS_ACTIVE = request.IsActive
        });
        return affected > 0;
    }
}

public class DeleteRoleCommand : IRequest<bool>
{
    public string RoleId { get; set; } = default!;
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeleteRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.DeleteRole, new { ROLE_ID = request.RoleId });
        return affected > 0;
    }
}

// USER-ROLE ASSIGNMENT COMMANDS
public class AssignUserRoleCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string AcademicYearId { get; set; } = default!;
    public int IsActive { get; set; } = 1;
}

public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AssignUserRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.AssignUserRole, request);
        return affected > 0;
    }
}

public class UpdateUserRoleCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string AcademicYearId { get; set; } = default!;
    public int IsActive { get; set; }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateUserRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.UpdateUserRole, request);
        return affected > 0;
    }
}


public class UnassignUserRoleCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string RoleId { get; set; } = default!;
}

public class UnassignUserRoleCommandHandler : IRequestHandler<UnassignUserRoleCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnassignUserRoleCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UnassignUserRoleCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(UserQueries.UnassignUserRole, request);
        return affected > 0;
    }
}


