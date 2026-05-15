using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Infrastructure.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
}
