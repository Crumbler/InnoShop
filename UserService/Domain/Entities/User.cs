namespace UserService.Domain.Entities
{
    public class User
    {
        public int UserId { get; init; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required Role Role { get; set; }
        public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
        public required string PasswordHash { get; set; }
    }
}
