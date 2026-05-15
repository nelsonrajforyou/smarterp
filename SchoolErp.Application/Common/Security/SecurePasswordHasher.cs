using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using SchoolErp.Domain.Entities;

namespace SchoolErp.Application.Common.Security;

public class SecurePasswordHasher : IPasswordHasher<User>
{
    private readonly PasswordHasher<User> _internalHasher = new PasswordHasher<User>();

    public string HashPassword(User user, string password)
    {
        // Uses ASP.NET Core's default secure PBKDF2 implementation
        return _internalHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        // Check if it's the old legacy SHA256 format (contains ':')
        if (hashedPassword.Contains(':'))
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length == 2)
            {
                var salt = parts[0];
                var hash = parts[1];
                var providedHash = ComputeLegacySha256Hash(providedPassword, salt);
                
                if (hash.Equals(providedHash, StringComparison.OrdinalIgnoreCase))
                {
                    // Success, but needs to be rehashed to the new secure format
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
            }
            return PasswordVerificationResult.Failed;
        }
        
        // Also check if it's the very old unsalted legacy format
        if (hashedPassword.Length == 64 && !hashedPassword.Contains(':'))
        {
             var legacyHash = ComputeLegacySha256Hash(providedPassword, "");
             if (hashedPassword.Equals(legacyHash, StringComparison.OrdinalIgnoreCase))
             {
                 return PasswordVerificationResult.SuccessRehashNeeded;
             }
        }

        // Otherwise, verify using the secure ASP.NET Core hasher
        return _internalHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    private static string ComputeLegacySha256Hash(string rawData, string salt)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData + salt));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
