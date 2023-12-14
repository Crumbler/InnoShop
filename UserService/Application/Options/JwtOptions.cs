namespace UserService.Application.Options
{
    public class JwtOptions
    {
        public const string Jwt = "Jwt";
        public required string RsaPrivateKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required TimeSpan LoginDuration { get; set; }
        public required TimeSpan EmailConfirmationDuration { get; set; }
        public required TimeSpan ResetPasswordDuration { get; set; }
    }
}
