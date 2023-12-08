using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class UpdateUserReq
    {
        [StringLength(maximumLength: 30, MinimumLength = 2)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(maximumLength: 30, MinimumLength = 8)]
        public string? Email { get; set; }
    }
}
