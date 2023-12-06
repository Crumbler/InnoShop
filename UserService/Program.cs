using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;

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

            builder.Services.AddDbContext<UserServiceDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration["ConnectionStrings:UserServiceConnection"]));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            { 
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
