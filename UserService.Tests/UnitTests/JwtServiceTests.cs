using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using UserService.Application.Models;
using UserService.Application.Options;
using UserService.Application.Services;
using UserService.Domain.Entities;

namespace UserService.Tests.UnitTests
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public static class JwtServiceTests
    {
        private static readonly JwtSecurityTokenHandler handler = new();
        private static readonly RsaSecurityKey key = new(RSA.Create());
        private static readonly JwtOptions options = new()
        {
            Audience = "Audience",
            Issuer = "Issuer",
            RsaPrivateKey = null!,
            LoginDuration = TimeSpan.FromMinutes(10),
            ResetPasswordDuration = TimeSpan.FromHours(1),
            EmailConfirmationDuration = TimeSpan.FromDays(3)
        };

        private static string GetClaim(this JwtSecurityToken token, string claim)
        {
            return token.Claims.Single(c => 
                c.Type.Equals(claim, StringComparison.InvariantCulture)).Value;
        }

        [Test]
        public static void GetToken([Values] JwtTokenType tokenType, [Values] bool isAdmin)
        {
            // Arrange
            var jwtService = new JwtService(key, handler, options, null!, null!);

            TimeSpan duration = tokenType switch
            {
                JwtTokenType.Authentication => options.LoginDuration,
                JwtTokenType.PasswordReset => options.ResetPasswordDuration,
                JwtTokenType.EmailConfirmation => options.EmailConfirmationDuration,
                _ => throw new NotImplementedException()
            };

            var user = new User()
            {
                Email = null!,
                Name = null!,
                PasswordHash = null!,
                UserId = 1,
                Role = new Role()
                {
                    Name = null!,
                    HasAdminPrivileges = isAdmin
                }
            };

            DateTime expCalculated = user.CreatedOn + duration;

            // Act
            string tokenString = jwtService.GetToken(user, tokenType);

            // Assert
            var token = handler.ReadJwtToken(tokenString);

            int id = int.Parse(token.GetClaim("sub_id"), CultureInfo.InvariantCulture);
            bool isUserAdmin = bool.Parse(token.GetClaim("admin"));

            Assert.Multiple(() =>
            {
                Assert.That(id, Is.EqualTo(user.UserId));
                Assert.That(isUserAdmin, Is.EqualTo(isAdmin));
                Assert.That(token.Audiences, Has.Member(options.Audience));
                Assert.That(token.Issuer, Is.EqualTo(options.Issuer));
                Assert.That(token.ValidTo, Is.EqualTo(expCalculated)
                    .Within(TimeSpan.FromSeconds(1)));
            });
        }
    }
}
