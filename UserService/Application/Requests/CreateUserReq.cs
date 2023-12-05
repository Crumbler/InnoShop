using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class CreateUserReq
    {
        [MaxLength(30)]
        public required string Name { get; set; }

        [EmailAddress]
        [MaxLength(30)]
        public required string Email { get; set; }
        [MaxLength(30)]
        public required string Password { get; set; }
    }
}
