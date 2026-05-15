using Dapper;
using SchoolErp.Application.Common.Mediator;
using Microsoft.AspNetCore.Identity;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Application.Features.Users;

namespace SchoolErp.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IErrorLogger _logger;

    public LoginCommandHandler(
        IDbConnectionFactory connectionFactory, 
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher<User> passwordHasher,
        IErrorLogger logger)
    {
        _connectionFactory = connectionFactory;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        try
        {
            var user = await connection.QuerySingleOrDefaultAsync<User>(
                UserQueries.GetUserByEmail, 
                new { Email = request.Email });

            if (user == null)
            {
                return new LoginResponse { Success = false, ErrorMessage = "Account not found." };
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PASSWORD_HASH, request.Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                return new LoginResponse { Success = false, ErrorMessage = "Incorrect password." };
            }

            if (user.IS_ACTIVE != "1")
            {
                return new LoginResponse { Success = false, ErrorMessage = "Account is inactive." };
            }

            // Fetch roles using both identifiers to be safe in a single round-trip
            var userIdentifiers = new[] { user.USER_ID, user.USER_INFO_ID }.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToArray();
            var rolesEnumerable = await connection.QueryAsync<string>(
                UserQueries.GetUserRoles, 
                new { UserIds = userIdentifiers });

            var roles = rolesEnumerable.ToList();

            // Fetch active academic year
            var activeYear = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT ACADEMIC_YEAR_ID, ACADEMIC_YEAR FROM ACADEMIC_YEAR WHERE IS_ACTIVE = 1 LIMIT 1");

            var token = _jwtTokenGenerator.GenerateToken(user, roles, activeYear?.ACADEMIC_YEAR_ID?.ToString(), activeYear?.ACADEMIC_YEAR?.ToString());

            return new LoginResponse { Success = true, Token = token };
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, source: "LoginCommandHandler");
            return new LoginResponse { Success = false, ErrorMessage = $"Database error: {ex.Message}" };
        }
    }
}
