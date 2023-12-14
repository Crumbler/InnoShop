using UserService.Application.DTOs;
using UserService.Application.Requests;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> CreateUserAsync(CreateUserReq req);
        Task<UserDTO> GetUserAsync(int id);
        Task UpdateUserAsync(int id, UpdateUserReq req);
        Task DeleteUserAsync(int id);
        Task<LoginDTO> Login(LoginReq req);
        Task ConfirmUser(string tokenString);
        Task ForgotPassword(ForgotPasswordReq req);
        Task ResetPassword(string tokenString, ResetPasswordRequest req);
    }
}
