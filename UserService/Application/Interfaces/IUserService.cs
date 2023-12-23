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
        Task<LoginDTO> LoginAsync(LoginReq req);
        Task ConfirmUserAsync(string tokenString);
        Task ForgotPasswordAsync(ForgotPasswordReq req);
        Task ResetPasswordAsync(string tokenString, ResetPasswordReq req);
    }
}
