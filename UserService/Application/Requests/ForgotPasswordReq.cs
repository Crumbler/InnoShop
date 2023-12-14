using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class ForgotPasswordReq
    {
        [EmailAddress]
        [StringLength(maximumLength: 30, MinimumLength = 8)]
        public required string Email { get; set; }
    }
}
