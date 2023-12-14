using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Models;
using UserService.Application.Options;
using UserService.Application.Requests;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services
{
    public class UserService(IUserRepository userRepository,
        IPasswordHelper passwordHelper, IJwtService jwtService,
        UserCreationOptions userCreationOptions, IEmailService emailService,
        LinkGenerator linkGenerator) : IUserService
    {
        public async Task<UserDTO> CreateUserAsync(CreateUserReq req)
        {
            User? oldUser = await userRepository.GetUserByEmailAsync(req.Email);

            if (oldUser != null && oldUser.IsEmailConfirmed)
            {
                throw new EmailInUseException(req.Email);
            }

            string hash = passwordHelper.HashPassword(req.Password);

            User user;

            if (oldUser == null)
            {
                user = new User()
                {
                    Name = req.Name,
                    Email = req.Email,
                    Role = userCreationOptions.InitialRole,
                    PasswordHash = hash
                };

                user = await userRepository.CreateUserAsync(user);
            }
            else
            {
                user = oldUser;
                user.Email = req.Email;
                user.Name = req.Name;
                user.PasswordHash = hash;
                user.CreatedOn = DateTime.UtcNow;

                await userRepository.UpdateUserAsync(user);
            }

            _ = SendConfirmationEmail(user);

            return new UserDTO(user);
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

            if (!passwordHelper.IsValid(req.Password, user.PasswordHash) || !user.IsEmailConfirmed)
            {
                throw new InvalidCredentialsException();
            }

            return new LoginDTO()
            {
                UserId = user.UserId,
                Token = jwtService.GetAuthenticationToken(user)
            };
        }

        private Task SendConfirmationEmail(User user)
        {
            string token = jwtService.GetEmailConfirmationToken(user);
            string confirmationUrl = linkGenerator.GetPathByName("ConfirmEmail", new { token }) ??
                throw new Exception("Unable to generate url to ConfirmEmail route");

            var email = new Email()
            {
                Subject = "Account confirmation",
                RecipientName = user.Name,
                RecepientAddress = user.Email,
                Body = $"To confirm your account send a POST request to the following url: {confirmationUrl}"
            };

            return emailService.SendEmailAsync(email);
        }

        public async Task ConfirmUser(string tokenString)
        {
            if (!jwtService.ValidateToken(tokenString, out JwtSecurityToken? token))
            {
                throw new InvalidTokenException();
            }

            int userId = int.Parse(token.Claims.Single(c => c.Type == "sub_id").Value, 
                CultureInfo.InvariantCulture);

            User? user = await userRepository.GetUserAsync(userId) ??
                throw new UserNotFoundException(userId);

            if (user.IsEmailConfirmed)
            {
                throw new UserAlreadyConfirmedException();
            }

            user.IsEmailConfirmed = true;

            await userRepository.UpdateUserAsync(user);
        }
    }
}
