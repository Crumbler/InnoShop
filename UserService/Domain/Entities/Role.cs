namespace UserService.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; init; }
        public required string Name { get; set; }
        public bool HasAdminPrivileges { get; init; }
    }
}
