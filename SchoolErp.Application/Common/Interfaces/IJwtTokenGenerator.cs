using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles, string? academicYearId = null, string? academicYear = null);
}
