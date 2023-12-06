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
        public string Name { get; set; }

        [MaxLength(30)]
        public string Email { get; set; }
        public EFRole Role { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }

        public EFUser(User user)
        {
            UserId = user.UserId;
            Name = user.Name;
            Email = user.Email;
            Role = new EFRole(user.Role);
            CreatedOn = user.CreatedOn;
            PasswordHash = user.PasswordHash;
            PasswordSalt = user.PasswordSalt;
        }

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
