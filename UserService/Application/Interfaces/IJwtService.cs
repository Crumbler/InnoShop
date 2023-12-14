using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.Models;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IJwtService
    {
        public string GetToken(User user, JwtTokenType type);
        public bool ValidateToken(string tokenString, [NotNullWhen(true)] out JwtSecurityToken? token);
    }
}
