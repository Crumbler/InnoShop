using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class ResetPasswordRequest
    {
        [StringLength(maximumLength: 30, MinimumLength = 8)]
        [RegularExpression(@"^(?=\d*[a-zA-Z]+)(?=[a-zA-Z]*\d+).*$",
            ErrorMessage = "The password must contain only letters of the English alphabet and digits")]
        public required string Password { get; set; }
    }
}
