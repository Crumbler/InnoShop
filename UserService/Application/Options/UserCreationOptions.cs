using UserService.Domain.Entities;

namespace UserService.Application.Options
{
    public class UserCreationOptions
    {
        public const string Users = "Users";
        public required string InitialUserRoleName { get; set; }

        private Role? initialRole;

        // Use method to avoid IConfiguration's binding
        public Role GetInitialRole()
        {
            if (initialRole == null)
            {
                throw new Exception($"{nameof(initialRole)} is null");
            }

            return initialRole;
        }

        public void SetInitialRole(Role initialRole)
        {
            this.initialRole = initialRole;
        }
    }
}
