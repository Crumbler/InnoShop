namespace UserService.Application.DTOs
{
    public class LoginDTO
    {
        public required int UserId { get; init; }
        public required string Token { get; init; }
    }
}
