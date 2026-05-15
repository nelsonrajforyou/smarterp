using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User user, IList<string> roles, string? academicYearId = null, string? academicYear = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.USER_INFO_ID), // Use the primary key of the user record
            new Claim(JwtRegisteredClaimNames.Email, user.EMAIL),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.USER_INFO_ID),
            new Claim(ClaimTypes.Name, $"{user.FIRST_NAME} {user.LAST_NAME}"),
            new Claim("StaffStudentId", user.USER_ID) // Add the linked ID as a custom claim
        };

        if (!string.IsNullOrEmpty(academicYearId)) claims.Add(new Claim("AcademicYearId", academicYearId));
        if (!string.IsNullOrEmpty(academicYear)) claims.Add(new Claim("AcademicYear", academicYear));

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
