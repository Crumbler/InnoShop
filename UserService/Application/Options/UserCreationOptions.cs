using UserService.Domain.Entities;

namespace UserService.Application.Options
{
    public class UserCreationOptions(Role initialRole)
    {
        public const string Users = "Users",
            InitialUserRoleName = "InitialUserRoleName";
        public Role InitialRole { get; } = initialRole;
    }
}
