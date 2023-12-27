namespace ProductService.Presentation.Options
{
    public class JwtOptions
    {
        public const string Jwt = "Jwt";
        public required string RsaPublicKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string UserIdClaimType { get; set; }
        public required string IsAdminClaimType { get; set; }
    }
}
