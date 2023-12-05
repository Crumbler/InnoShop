using UserService.Domain.Entities;

namespace UserService.Application.DTOs
{
    public class UserDTO(User user)
    {
        public int UserId { get; } = user.UserId;
        public string Name { get; } = user.Name;
        public string Email { get; } = user.Email;
        public Role Role { get; } = user.Role;
        public DateTime CreatedOn { get; } = user.CreatedOn;
    }
}
