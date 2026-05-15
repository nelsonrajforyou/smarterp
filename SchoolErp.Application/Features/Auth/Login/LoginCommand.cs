using System.ComponentModel.DataAnnotations;
using SchoolErp.Application.Common.Mediator;

namespace SchoolErp.Application.Features.Auth.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = default!;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
}
