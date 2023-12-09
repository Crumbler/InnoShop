using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace UserService.Application.Options
{
    public class AuthenticationKeys
    {
        public RsaSecurityKey PrivateKey { get; }
        public RsaSecurityKey PublicKey { get; }

        public AuthenticationKeys(AuthenticationOptions options)
        {
            using var rsa = RSA.Create();

            rsa.ImportRSAPrivateKey(source: Convert.FromBase64String(options.RsaPrivateKey), out _);

            PrivateKey = new RsaSecurityKey(rsa);

            rsa.ImportRSAPublicKey(source: Convert.FromBase64String(options.RsaPublicKey), out _);

            PublicKey = new RsaSecurityKey(rsa);
        }
    }
}
