using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Interfaces;
using UserService.Application.Models;
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
                    new(jwtOptions.UserIdClaimType, user.UserId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer32),
                    new(jwtOptions.IsAdminClaimType, user.Role.HasAdminPrivileges.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Boolean)
                }),
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }

        public string GetToken(User user, JwtTokenType type) => type switch
        {
            JwtTokenType.Authentication => GetToken(user, jwtOptions.LoginDuration),
            JwtTokenType.EmailConfirmation => GetToken(user, jwtOptions.EmailConfirmationDuration),
            JwtTokenType.PasswordReset => GetToken(user, jwtOptions.ResetPasswordDuration),
            _ => throw new ArgumentException("Unknown Jwt token type", nameof(type)),
        };

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
