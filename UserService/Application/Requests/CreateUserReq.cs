using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class CreateUserReq
    {
        [StringLength(maximumLength: 30, MinimumLength = 2)]
        public required string Name { get; set; }

        [EmailAddress]
        [StringLength(maximumLength: 30, MinimumLength = 8)]
        public required string Email { get; set; }

        [StringLength(maximumLength: 30, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)$",
            ErrorMessage ="The password must contain only letters of the English alphabet and digits")]
        public required string Password { get; set; }
    }
}
