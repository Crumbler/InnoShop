using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure.Data;

namespace ProductService
{
    public static class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

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
        }

        private static void ConfigureDatabase(IServiceCollection services,
            ConfigurationManager config, IWebHostEnvironment environment)
        {
            string connectionString = config.GetConnectionString("ProductServiceConnection") ??
                throw new Exception("No ProductService Connection string in configuration.ConnectionStrings.");

            services.AddDbContext<ProductServiceDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging(environment.IsDevelopment());
            });

            var optionsBuilder = new DbContextOptionsBuilder<ProductServiceDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
