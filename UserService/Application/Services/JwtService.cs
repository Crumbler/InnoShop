using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Domain.Entities;

namespace UserService.Application.Services
{
    public class JwtService(RsaSecurityKey key, JwtSecurityTokenHandler handler,
        JwtOptions jwtOptions, TokenValidationParameters validationParameters,
        ILogger<JwtService> logger) : IJwtService
    {
        private string GetToken(User user, TimeSpan duration)
        {
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow + duration,
                Audience = jwtOptions.Audience,
                Issuer = jwtOptions.Issuer,
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

        public string GetAuthenticationToken(User user)
        {
            return GetToken(user, jwtOptions.LoginDuration);
        }

        public string GetEmailConfirmationToken(User user)
        {
            return GetToken(user, jwtOptions.EmailConfirmationDuration);
        }

        public bool ValidateToken(string tokenString, [NotNullWhen(true)] out JwtSecurityToken? token)
        {
            try
            {
                handler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);
                token = (JwtSecurityToken)validatedToken;
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to validate JWT token");

                token = null;
                return false;
            }
        }
    }
}
