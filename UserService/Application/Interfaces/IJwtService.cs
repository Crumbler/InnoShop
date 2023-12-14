using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IJwtService
    {
        public string GetAuthenticationToken(User user);
        public string GetEmailConfirmationToken(User user);
        public bool ValidateToken(string tokenString, [NotNullWhen(true)] out JwtSecurityToken? token);
    }
}
