using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data.Entities
{
    [Table("Roles")]
    public class EFRole
    {
        [Key]
        public int RoleId { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }

        public bool HasAdminPrivileges { get; set; }

        public EFRole(Role role)
        {
            RoleId = role.RoleId;
            Name = role.Name;
            HasAdminPrivileges = role.HasAdminPrivileges;
        }

        public Role ToRole() => new()
        { 
            RoleId = RoleId,
            Name = Name,
            HasAdminPrivileges = HasAdminPrivileges
        };
    }
}
