using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using UserService.Presentation.Handlers;

namespace UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddSwaggerGen();
            }

            string connectionString = builder.Configuration["ConnectionStrings:UserServiceConnection"] ??
                throw new Exception("No connection string in configuration.");

            builder.Services.AddDbContext<UserServiceDbContext>(options =>
                options.UseSqlServer(connectionString));

            var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var dbContext = new UserServiceDbContext(optionsBuilder.Options))
            {
                string initialRoleName = builder.Configuration["Users:InitialUserRoleName"] ??
                    throw new Exception("No initial role name in configuration.");

                DatabaseHelper.SetupDatabaseAndSeedData(dbContext, initialRoleName);
                var options = new UserCreationOptions(DatabaseHelper.GetRole(dbContext, initialRoleName));

                builder.Services.AddSingleton(Options.Create(options));
            }

            builder.Services.AddScoped<IUserRepository, EFUserRepository>();
            
            builder.Services.AddSingleton<IPasswordHelper, PasswordHelper>();

            builder.Services.AddScoped<IUserService, Application.Services.UserService>();

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

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
    }
}
