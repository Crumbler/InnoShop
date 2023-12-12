using Microsoft.Extensions.Options;
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
        IPasswordHelper passwordHelper, IJwtService jwtService,
        UserCreationOptions userCreationOptions) : IUserService
    {
        public async Task<UserDTO> CreateUserAsync(CreateUserReq req)
        {
            bool isEmailAvailable = await userRepository.CheckEmailAvailableAsync(req.Email);
            if (!isEmailAvailable)
            {
                throw new EmailInUseException(req.Email);
            }

            string hash = passwordHelper.HashPassword(req.Password);

            var user = new User()
            {
                Name = req.Name,
                Email = req.Email,
                Role = userCreationOptions.InitialRole,
                PasswordHash = hash
            };

            User createdUser = await userRepository.CreateUserAsync(user);

            return new UserDTO(createdUser);
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
                bool isEmailAvailable = await userRepository.CheckEmailAvailableAsync(req.Email);
                if (!isEmailAvailable)
                {
                    throw new EmailInUseException(req.Email);
                }

                user.Email = req.Email;
            }

            await userRepository.UpdateUserAsync(user);
        }

        public async Task<LoginDTO> Login(LoginReq req)
        {
            User user = await userRepository.GetUserByEmailAsync(req.Email) ??
                throw new InvalidCredentialsException();

            if (!passwordHelper.IsValid(req.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            return new LoginDTO()
            {
                UserId = user.UserId,
                Token = jwtService.GetJwtToken(user)
            };
        }
    }
}
