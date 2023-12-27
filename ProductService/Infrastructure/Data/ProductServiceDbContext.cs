using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure.Data.Entities;

namespace ProductService.Infrastructure.Data
{
    public class ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) :
        DbContext(options)
    {
        public DbSet<EFProduct> Products { get; set; }
        public DbSet<EFCategory> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_100_CS_AS_KS_WS_SC");

            modelBuilder.Entity<EFProduct>().Property(p => p.Name)
                .UseCollation("Latin1_General_100_CI_AS_KS_WS_SC");

            modelBuilder.Entity<EFProduct>().Property(p => p.Description)
                .UseCollation("Latin1_General_100_CI_AS_KS_WS_SC");
        }
    }
}
