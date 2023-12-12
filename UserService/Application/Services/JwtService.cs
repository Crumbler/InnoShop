using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services
{
    public class JwtService(RsaSecurityKey key, JwtSecurityTokenHandler handler) : IJwtService
    {
        public string GetJwtToken(User user)
        {
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow.AddMinutes(10),
                Audience = "User",
                Issuer = "UserService",
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new("sub_id", user.UserId.ToString(CultureInfo.InvariantCulture)),
                    new("admin", user.Role.HasAdminPrivileges.ToString(CultureInfo.InvariantCulture))
                }),
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }
    }
}
