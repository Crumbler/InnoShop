using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Data.Entities;

namespace UserService.Infrastructure.Repositories
{
    public class EFUserRepository(UserServiceDbContext dbContext) : IUserRepository
    {
        public async Task<bool> CheckEmailAvailableAsync(string email)
        {
            return !await dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var efUser = EFUser.FromUser(user);

            dbContext.Users.Entry(efUser).State = EntityState.Added;

            await dbContext.SaveChangesAsync();

            return efUser.ToUser();
        }

        public async Task DeleteUserAsync(int id)
        {
            await dbContext.Users.Where(u => u.UserId == id).ExecuteDeleteAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            EFUser? user = await dbContext.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.UserId == id);

            return user?.ToUser();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            EFUser? user = await dbContext.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Email == email);

            return user?.ToUser();
        }

        public async Task UpdateUserAsync(User user)
        {
            EFUser oldUser = await dbContext.Users.FindAsync(user.UserId) ??
                throw new Exception("User not found");

            var efUser = EFUser.FromUser(user);
            dbContext.Users.Entry(oldUser).CurrentValues.SetValues(efUser);

            await dbContext.SaveChangesAsync();
        }
    }
}
