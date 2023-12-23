using UserService.Domain.Entities;

namespace UserService.Application.DTOs
{
    public class UserDTO
    {
        public required int UserId { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required Role Role { get; init; }
        public required DateTime CreatedOn { get; init; }

        public static UserDTO FromUser(User user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedOn = user.CreatedOn
        };
    }
}
