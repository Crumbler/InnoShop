using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services,
            ConfigurationManager config, IWebHostEnvironment environment)
        {
            services.AddControllers();

            if (environment.IsDevelopment())
            {
                services.AddSwaggerGen();
            }

            ConfigureDatabase(services, config, environment);

            ConfigureAuthentication(services, config);

            ConfigureEmail(services, config);

            services.AddSingleton<IPasswordHelper, PasswordHelper>();

            services.AddScoped<IUserService, Application.Services.UserService>();

            services.AddProblemDetails();
            services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();
        }

        private static void ConfigureDatabase(IServiceCollection services,
            ConfigurationManager config, IWebHostEnvironment environment)
        {
            string connectionString = config.GetConnectionString("UserServiceConnection") ??
                throw new Exception("No UserServiceConnection string in configuration.ConnectionStrings.");

            services.AddDbContext<UserServiceDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging(environment.IsDevelopment());
            });

            var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var dbContext = new UserServiceDbContext(optionsBuilder.Options))
            {
                string initialRoleName =
                    config.GetRequiredSection(UserCreationOptions.Users)[UserCreationOptions.InitialUserRoleName] ??
                    throw new Exception("No initial user role name in configuration.");

                DatabaseHelper.SetupDatabaseAndSeedData(dbContext, initialRoleName);
                var options = new UserCreationOptions(DatabaseHelper.GetRole(dbContext, initialRoleName));

                services.AddSingleton(options);
            }
        }

        private static void ConfigureAuthentication(IServiceCollection services,
            ConfigurationManager config)
        {
            services.AddSingleton<IJwtService, JwtService>();

            services.AddScoped<IUserRepository, EFUserRepository>();

            services.AddSingleton<JwtSecurityTokenHandler>();

            JwtOptions jwtOptions = config.GetRequiredSection(JwtOptions.Jwt).Get<JwtOptions>() ??
                throw new Exception($"{nameof(JwtOptions)} not specified");

            services.AddSingleton(jwtOptions);

            var rsa = RSA.Create();
            rsa.ImportFromPem(jwtOptions.RsaPrivateKey);
            var securityKey = new RsaSecurityKey(rsa);
            services.AddSingleton(securityKey);

            services.AddAuthentication(A =>
            {
                A.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                A.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(O =>
            {
                O.RequireHttpsMetadata = false;
                O.SaveToken = false;
                O.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = securityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience
                };
            });
        }

        private static void ConfigureEmail(IServiceCollection services,
            ConfigurationManager config)
        {
            EmailOptions emailOptions = config.GetRequiredSection(EmailOptions.Email).Get<EmailOptions>() ??
                throw new Exception($"{nameof(EmailOptions)} not specified");

            services.AddSingleton(emailOptions);

            services.AddSingleton<IEmailService, EmailService>();
        }
    }
}
