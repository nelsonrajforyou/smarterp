using Microsoft.AspNetCore.Identity;
using SchoolErp.Domain.Entities;
using System;

var hasher = new PasswordHasher<User>();
var user = new User { EMAIL = "admin@school.edu" };
var password = "Admin@123";
var hash = hasher.HashPassword(user, password);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"SQL Insert:");
Console.WriteLine($"INSERT INTO USER_INFO (USER_ID, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, STATUS) VALUES ");
Console.WriteLine($"('{Guid.NewGuid()}', '{user.EMAIL}', '{hash}', 'System', 'Admin', 'Active');");
