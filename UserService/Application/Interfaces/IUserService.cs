using UserService.Application.DTOs;
using UserService.Application.Options;
using UserService.Application.Requests;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(CreateUserReq req, UserCreationOptions options);
        Task<UserDTO> GetUserAsync(int id);
        Task UpdateUserAsync(int id, UpdateUserReq req);
        Task DeleteUserAsync(int id);
    }
}
