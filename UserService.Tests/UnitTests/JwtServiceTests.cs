using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
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
        private static readonly SigningCredentials credentials = 
            new(key, SecurityAlgorithms.RsaSha256);
        private static readonly JwtOptions options = new()
        {
            Audience = "Audience",
            Issuer = "Issuer",
            RsaPrivateKey = null!,
            LoginDuration = TimeSpan.FromMinutes(10),
            ResetPasswordDuration = TimeSpan.FromHours(1),
            EmailConfirmationDuration = TimeSpan.FromDays(3)
        };
        private static readonly TokenValidationParameters validationParameters = new()
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience
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

        [Test]
        public static void ValidateToken()
        {
            // Arrange
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow + TimeSpan.FromHours(3),
                Audience = options.Audience,
                Issuer = options.Issuer,
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            var tokenString =  handler.WriteToken(token);

            var jwtService = new JwtService(null!, handler, null!, 
                validationParameters, Mock.Of<ILogger<JwtService>>());

            // Act
            bool isValid = jwtService.ValidateToken(tokenString, 
                out JwtSecurityToken? resToken);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(isValid);
                Assert.That(resToken is not null);
            });
        }

        [Test]
        public static void ValidateToken_Expired()
        {
            // Arrange
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

            var jwtService = new JwtService(null!, handler, null!, 
                validationParameters, Mock.Of<ILogger<JwtService>>());

            // Act
            bool isValid = jwtService.ValidateToken(tokenString, 
                out JwtSecurityToken? resToken);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(!isValid);
                Assert.That(resToken is null);
            });
        }

        [Test]
        public static void ValidateToken_InvalidAudience()
        {
            // Arrange
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow + TimeSpan.FromDays(1),
                Audience = "DefinetelyNotValidAudience",
                Issuer = options.Issuer,
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            var tokenString = handler.WriteToken(token);

            var jwtService = new JwtService(null!, handler, null!,
                validationParameters, Mock.Of<ILogger<JwtService>>());

            // Act
            bool isValid = jwtService.ValidateToken(tokenString,
                out JwtSecurityToken? resToken);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(!isValid);
                Assert.That(resToken is null);
            });
        }

        [Test]
        public static void ValidateToken_InvalidIssuer()
        {
            // Arrange
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow + TimeSpan.FromDays(1),
                Audience = options.Audience,
                Issuer = "InvalidIssuer",
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            var tokenString = handler.WriteToken(token);

            var jwtService = new JwtService(null!, handler, null!,
                validationParameters, Mock.Of<ILogger<JwtService>>());

            // Act
            bool isValid = jwtService.ValidateToken(tokenString,
                out JwtSecurityToken? resToken);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(!isValid);
                Assert.That(resToken is null);
            });
        }
    }
}
