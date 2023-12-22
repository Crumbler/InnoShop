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
        }
    }
}
