﻿using UserService.Domain.Entities;

namespace UserService.Domain.Repositories
{
    public interface IUserRepository
    {
        public Task<User> CreateUserAsync(User user);
        public Task UpdateUserAsync(User user);
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task DeleteUserAsync(int id);
        public Task<bool> CheckEmailAvailableAsync(string email);
    }
}
