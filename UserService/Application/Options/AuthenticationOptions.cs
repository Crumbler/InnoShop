namespace UserService.Application.Options
{
    public class AuthenticationOptions
    {
        public const string Authentication = "Authentication";
        public required string RsaPrivateKey { get; set; }
        public required string RsaPublicKey { get; set; }
    }
}
