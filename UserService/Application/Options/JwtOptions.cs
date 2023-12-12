namespace UserService.Application.Options
{
    public class JwtOptions
    {
        public const string Jwt = "Jwt";
        public required string RsaPrivateKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}
