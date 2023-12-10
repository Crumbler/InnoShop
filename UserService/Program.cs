using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using UserService.Presentation.Handlers;

namespace UserService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
            
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            { 
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, 
            IConfiguration config, IWebHostEnvironment environment)
        {
            services.AddControllers();

            if (environment.IsDevelopment())
            {
                services.AddSwaggerGen();
            }

            string connectionString = config["ConnectionStrings:UserServiceConnection"] ??
                throw new Exception("No connection string in configuration.");

            services.AddDbContext<UserServiceDbContext>(options => {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging(environment.IsDevelopment());
            });

            var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var dbContext = new UserServiceDbContext(optionsBuilder.Options))
            {
                string initialRoleName =
                    config.GetRequiredSection(UserCreationOptions.Users)[UserCreationOptions.InitialUserRoleName] ??
                    throw new Exception("No initial role name in configuration.");

                DatabaseHelper.SetupDatabaseAndSeedData(dbContext, initialRoleName);
                var options = new UserCreationOptions(DatabaseHelper.GetRole(dbContext, initialRoleName));

                services.AddSingleton(Options.Create(options));
            }

            services.AddScoped<IUserRepository, EFUserRepository>();

            services.AddSingleton<JwtSecurityTokenHandler>();

            string privateKey = config["Authentication:RsaPrivateKey"] ??
                throw new Exception("Authentication options not specified");

            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            services.AddSingleton(new RsaSecurityKey(rsa));

            services.AddSingleton<IPasswordHelper, PasswordHelper>();
            
            services.AddScoped<IUserService, Application.Services.UserService>();
            
            services.AddProblemDetails();
            services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();
        }
    }
}
