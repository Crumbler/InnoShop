using Microsoft.EntityFrameworkCore;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data.Entities;

namespace UserService.Infrastructure.Data
{
    public static class DatabaseHelper
    {
        public static void SetupDatabaseAndSeedData(UserServiceDbContext context,
            string initialUserRoleName)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (context.Roles.Any() || context.Users.Any())
            {
                return;
            }

            EFRole roleRegular = new()
            {
                Name = initialUserRoleName
            },
            roleAdmin = new()
            {
                Name = "Admin",
                HasAdminPrivileges = true
            };

            var pHelper = new PasswordHelper();
            string hash = pHelper.HashPassword("abc12345");

            var users = new EFUser[]
            {
                new()
                {
                    Name = "John Doe",
                    Email = "johndoe@mail.com",
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = hash,
                    Role = roleAdmin
                },
                new()
                {
                    Name = "Christopher Davis",
                    Email = "ChristopherBDavis@rhyta.com",
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = hash,
                    Role = roleRegular
                },
                new()
                {
                    Name = "Gilbert Gustafson",
                    Email = "GilbertDGustafson@dayrep.com",
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = hash,
                    Role = roleRegular
                }
            };

            context.Users.AddRange(users);

            context.SaveChanges();
        }

        public static Role GetRole(UserServiceDbContext context, string name)
        {
            return context.Roles.Single(r => r.Name == name).ToRole();
        }
    }
}
