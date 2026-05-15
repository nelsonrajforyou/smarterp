using Dapper;
using FluentValidation;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Menu;

// MENU COMMANDS
public class CreateMenuCommand : IRequest<string>
{
    public string Name { get; set; } = default!;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string? ParentId { get; set; }
    public int Order { get; set; }
}

public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Menu Name is required.")
            .MaximumLength(100).WithMessage("Menu Name must not exceed 100 characters.");
    }
}

public class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Menu Name is required.")
            .MaximumLength(100).WithMessage("Menu Name must not exceed 100 characters.");
    }
}

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, string>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CreateMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(MenuQueries.InsertMenu, new
        {
            MENU_ID = id,
            MENU_NAME = request.Name,
            MENU_URL = request.Url,
            ICON = request.Icon,
            PARENT_MENU_ID = request.ParentId,
            DISPLAY_ORDER = request.Order,
            IS_ACTIVE = 1
        });
        return id;
    }
}

public class UpdateMenuCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string? ParentId { get; set; }
    public int Order { get; set; }
    public int IsActive { get; set; }
}

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(MenuQueries.UpdateMenu, new
        {
            MENU_ID = request.Id,
            MENU_NAME = request.Name,
            MENU_URL = request.Url,
            ICON = request.Icon,
            PARENT_MENU_ID = request.ParentId,
            DISPLAY_ORDER = request.Order,
            IS_ACTIVE = request.IsActive
        });
        return affected > 0;
    }
}

public class DeleteMenuCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
}

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeleteMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(MenuQueries.DeleteMenu, new { MENU_ID = request.Id });
        return affected > 0;
    }
}

// ROLE-MENU PERMISSION COMMANDS
public class AssignRoleMenuCommand : IRequest<bool>
{
    public string RoleId { get; set; } = default!;
    public string MenuId { get; set; } = default!;
    public int IsActive { get; set; } = 1;
}

public class AssignRoleMenuCommandHandler : IRequestHandler<AssignRoleMenuCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AssignRoleMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(AssignRoleMenuCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(MenuQueries.AssignRoleMenu, request);
        return affected > 0;
    }
}

public class UpdateRoleMenuCommand : IRequest<bool>
{
    public string RoleId { get; set; } = default!;
    public string MenuId { get; set; } = default!;
    public int IsActive { get; set; }
}

public class UpdateRoleMenuCommandHandler : IRequestHandler<UpdateRoleMenuCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UpdateRoleMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UpdateRoleMenuCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(MenuQueries.UpdateRoleMenu, request);
        return affected > 0;
    }
}


public class UnassignRoleMenuCommand : IRequest<bool>
{
    public string RoleId { get; set; } = default!;
    public string MenuId { get; set; } = default!;
}

public class UnassignRoleMenuCommandHandler : IRequestHandler<UnassignRoleMenuCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnassignRoleMenuCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(UnassignRoleMenuCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(MenuQueries.UnassignRoleMenu, request);
        return affected > 0;
    }
}
