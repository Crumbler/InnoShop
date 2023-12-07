using UserService.Domain.Entities;

namespace UserService.Application.Options
{
    public class UserCreationOptions(Role initialRole)
    {
        public Role InitialRole { get; } = initialRole;
    }
}
