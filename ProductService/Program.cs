using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Application.Options;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Presentation.Handlers;
using System.Text.Json.Serialization;

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

            services.AddScoped<IProductService, Application.Services.ProductService>();

            services.AddProblemDetails();
            services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

            var browsingOptions = config.GetRequiredSection(BrowsingOptions.Browsing).Get<BrowsingOptions>() ??
                throw new Exception($"{nameof(BrowsingOptions)} not specified");

            services.AddSingleton(browsingOptions);

            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SortBy>());
            });
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

            using var dbContext = new ProductServiceDbContext(optionsBuilder.Options);
            
            DatabaseHelper.SetupDatabaseAndSeedData(dbContext);

            services.AddScoped<IProductRepository, EFProductRepository>();
            services.AddScoped<ICategoryRepository, EFCategoryRepository>();
        }
    }
}
