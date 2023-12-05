using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Requests;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services
{
    public class UserService(IUserRepository userRepository, 
        IPasswordHelper passwordHelper) : IUserService
    {
        public async Task CreateUserAsync(CreateUserReq req, UserCreationOptions options)
        {
            (string hash, string salt) = passwordHelper.HashPassword(req.Password);

            var user = new User()
            {
                Name = req.Name,
                Email = req.Email,
                Role = options.GetInitialRole(),
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await userRepository.CreateUserAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            _ = await userRepository.GetUserAsync(id) ?? 
                throw new UserNotFoundException(id);

            await userRepository.DeleteUserAsync(id);
        }

        public async Task<UserDTO> GetUserAsync(int id)
        {
            User user = await userRepository.GetUserAsync(id) ?? 
                throw new UserNotFoundException(id);

            return new UserDTO(user);
        }

        public async Task UpdateUserAsync(int id, UpdateUserReq req)
        {
            User user = await userRepository.GetUserAsync(id) ?? 
                throw new UserNotFoundException(id);

            if (req.Name is not null)
            {
                user.Name = req.Name;
            }

            if (req.Email is not null)
            {
                user.Email = req.Email;
            }

            await userRepository.UpdateUserAsync(user);
        }
    }
}
