using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data.Entities;

namespace UserService.Infrastructure.Data
{
    public class UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : 
        DbContext(options)
    {
        public DbSet<EFRole> Roles { get; set; }
        public DbSet<EFUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_100_CS_AS_KS_WS_SC");
        }
    }
}
