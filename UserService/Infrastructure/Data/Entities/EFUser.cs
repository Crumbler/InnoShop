using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Entities
{
    [Table("Users")]
    [Index(nameof(Email), IsUnique = true)]
    public class EFUser
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(30)]
        public required string Name { get; set; }

        [MaxLength(30)]
        public required string Email { get; set; }
        public required EFRole Role { get; set; }
        public DateTime CreatedOn { get; set; }
        public required string PasswordHash { get; set; }
        public required string PasswordSalt { get; set; }

        public static EFUser FromUser(User user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = EFRole.FromRole(user.Role),
            CreatedOn = user.CreatedOn,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt
        };

        public User ToUser() => new()
        {
            UserId = UserId,
            Name = Name,
            Email = Email,
            Role = Role.ToRole(),
            CreatedOn = CreatedOn,
            PasswordHash = PasswordHash,
            PasswordSalt = PasswordSalt
        };
    }
}
