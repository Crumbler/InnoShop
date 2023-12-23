using Microsoft.IdentityModel.Tokens;
using UserService.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.Tests.IntegrationTests
{
    public static class TestHelper
    {
        public static string GenerateExpiredJwtToken(IServiceProvider services)
        {
            var key = services.GetRequiredService<RsaSecurityKey>();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var options = services.GetRequiredService<JwtOptions>();

            var handler = services.GetRequiredService<JwtSecurityTokenHandler>();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                NotBefore = DateTime.UtcNow - TimeSpan.FromDays(3),
                Expires = DateTime.UtcNow - TimeSpan.FromDays(1),
                Audience = options.Audience,
                Issuer = options.Issuer,
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            var tokenString = handler.WriteToken(token);

            return tokenString;
        }
    }
}
