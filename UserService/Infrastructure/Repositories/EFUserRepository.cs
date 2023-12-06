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
            EFUser? user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
            
            return user != null;
        }

        public async Task CreateUserAsync(User user)
        {
            await dbContext.Users.AddAsync(EFUser.FromUser(user));

            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            await dbContext.Users.Where(u => u.UserId == id).ExecuteDeleteAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return (await dbContext.Users.SingleOrDefaultAsync(u => u.UserId == id))?.ToUser();
        }

        public async Task UpdateUserAsync(User user)
        {
            var efUser = EFUser.FromUser(user);
            dbContext.Users.Update(efUser);

            await dbContext.SaveChangesAsync();
        }
    }
}
