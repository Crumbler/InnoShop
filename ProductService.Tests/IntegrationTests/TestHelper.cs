
using Microsoft.IdentityModel.Tokens;
using ProductService.Presentation.Options;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProductService.Tests.IntegrationTests
{
    public static class TestHelper
    {
        private static readonly JwtSecurityTokenHandler handler = new();

        public static string GenerateJwtToken(RsaSecurityKey key, JwtOptions jwtOptions, 
            int userId, bool isAdmin)
        {
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow + TimeSpan.FromMinutes(1),
                Audience = jwtOptions.Audience,
                Issuer = jwtOptions.Issuer,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(jwtOptions.UserIdClaimType, userId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer32),
                    new(jwtOptions.IsAdminClaimType, isAdmin.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Boolean)
                }),
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }
    }
}
