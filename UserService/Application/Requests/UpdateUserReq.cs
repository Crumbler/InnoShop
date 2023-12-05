using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Requests
{
    public class UpdateUserReq
    {
        [MaxLength(30)]
        public string? Name { get; set; }

        [EmailAddress]
        [MaxLength(30)]
        public string? Email { get; set; }
    }
}
