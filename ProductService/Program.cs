using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.Application.Interfaces;
using ProductService.Application.Options;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Presentation.Handlers;
using ProductService.Presentation.Options;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace ProductService
{
    public class Program
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

            using (var dbContext = new ProductServiceDbContext(optionsBuilder.Options))
            {
                DatabaseHelper.SetupDatabaseAndSeedData(dbContext);
            }

            services.AddScoped<IProductRepository, EFProductRepository>();
            services.AddScoped<ICategoryRepository, EFCategoryRepository>();
        }

        private static void ConfigureAuthentication(IServiceCollection services,
            ConfigurationManager config)
        {
            JwtOptions jwtOptions = config.GetRequiredSection(JwtOptions.Jwt).Get<JwtOptions>() ??
                throw new Exception($"{nameof(JwtOptions)} not specified");

            services.AddSingleton(jwtOptions);

            var rsa = RSA.Create();
            rsa.ImportFromPem(jwtOptions.RsaPublicKey);

            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience
            };

            services.AddAuthentication(A =>
            {
                A.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                A.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(O =>
            {
                O.RequireHttpsMetadata = false;
                O.SaveToken = false;
                O.TokenValidationParameters = validationParameters;
            });
        }
    }
}
