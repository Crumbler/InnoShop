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

            await dbContext.Users.AddAsync(efUser);

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

        public async Task UpdateUserAsync(User user)
        {
            var efUser = EFUser.FromUser(user);
            dbContext.Users.Update(efUser);

            await dbContext.SaveChangesAsync();
        }
    }
}
