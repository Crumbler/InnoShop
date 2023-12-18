using Microsoft.AspNetCore.Routing;
using Moq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Requests;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Tests.UnitTests
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public static class UserServiceTests
    {
        private static Application.Services.UserService CreateUserService(
            IUserRepository? userRepository = null,
            IPasswordHelper? passwordHelper = null, IJwtService? jwtService = null,
            UserCreationOptions? options = null, IEmailService? emailService = null,
            LinkGenerator? linkGenerator = null)
        {
            return new Application.Services.UserService(
                userRepository ?? Mock.Of<IUserRepository>(),
                passwordHelper ?? Mock.Of<IPasswordHelper>(),
                jwtService ?? Mock.Of<IJwtService>(),
                options!,
                emailService ?? Mock.Of<IEmailService>(),
                linkGenerator!
                );
        }

        private static User CreateUser(string email = "email", string name = "John Doe",
            string? passwordHash = null, bool emailConfirmed = false,
            int id = 1) => new()
            {
                Email = email,
                Name = name,
                PasswordHash = passwordHash!,
                Role = null!,
                IsEmailConfirmed = emailConfirmed,
                UserId = id
            };

        [Test]
        public static void CreateUser_UserWithConfirmedEmailExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = CreateUser(email: "email", emailConfirmed: true);

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(user.Email).Result)
                .Returns(user);

            var userService = CreateUserService(mockUserRepo.Object);

            var req = new CreateUserReq()
            {
                Email = user.Email,
                Name = "John Doe",
                Password = "12345"
            };

            // Act
            Assert.ThrowsAsync<EmailInUseException>(() => userService.CreateUserAsync(req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(user.Email), Times.Once());
        }

        [Test]
        public static void CreateUser_UserWithUnconfirmedEmailExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<LinkGenerator> mockLinkGen = new();

            var user = CreateUser(email: "email");

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(user.Email).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            mockLinkGen.SetReturnsDefault(string.Empty);

            var userService = CreateUserService(mockUserRepo.Object,
                linkGenerator: mockLinkGen.Object);

            var req = new CreateUserReq()
            {
                Email = user.Email,
                Name = "John Doe",
                Password = "12345"
            };

            // Act
            var dto = userService.CreateUserAsync(req).Result;

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(user.Email), Times.Once());
            mockUserRepo.Verify(m => m.UpdateUserAsync(user), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo(req.Name));
                Assert.That(dto.Email, Is.EqualTo(req.Email));
            });
        }

        [Test]
        public static void CreateUser_NoUserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<LinkGenerator> mockLinkGen = new();

            var user = CreateUser(email: "email", name: "Bob");

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(user.Email).Result)
                .Returns((User?)null);
            mockUserRepo.Setup(m => m.CreateUserAsync(It.IsAny<User>()).Result)
                .Returns(user);

            mockLinkGen.SetReturnsDefault(string.Empty);

            var userService = CreateUserService(mockUserRepo.Object,
                options: new UserCreationOptions(null!),
                linkGenerator: mockLinkGen.Object);

            var req = new CreateUserReq()
            {
                Email = user.Email,
                Name = user.Name,
                Password = "12345"
            };

            // Act
            var dto = userService.CreateUserAsync(req).Result;

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(req.Email), Times.Once());
            mockUserRepo.Verify(m => m.CreateUserAsync(It.IsAny<User>()), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo(user.Name));
                Assert.That(dto.Email, Is.EqualTo(user.Email));
            });
        }

        [Test]
        public static void DeleteUser_NoUserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            const int userId = 1;

            mockUserRepo.Setup(m => m.GetUserAsync(userId).Result)
                .Returns((User?)null);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            Assert.ThrowsAsync(Is.TypeOf<UserNotFoundException>()
                               .And.Message.Contains(userId.ToString(CultureInfo.InvariantCulture)),
                () => userService.DeleteUserAsync(userId));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(userId), Times.Once());
        }

        [Test]
        public static void DeleteUser_UserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = CreateUser();

            mockUserRepo.Setup(m => m.GetUserAsync(user.UserId).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.DeleteUserAsync(user.UserId))
                .Returns(Task.CompletedTask);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            userService.DeleteUserAsync(user.UserId).Wait();

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(user.UserId), Times.Once());
            mockUserRepo.Verify(m => m.DeleteUserAsync(user.UserId), Times.Once());
        }

        [Test]
        public static void GetUser_NoUserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            const int userId = 1;

            mockUserRepo.Setup(m => m.GetUserAsync(userId).Result)
                .Returns((User?)null);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            Assert.ThrowsAsync(Is.TypeOf<UserNotFoundException>()
                               .And.Message.Contains(userId.ToString(CultureInfo.InvariantCulture)),
                () => userService.GetUserAsync(userId));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(userId), Times.Once());
        }

        [Test]
        public static void GetUser_UserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = CreateUser();

            mockUserRepo.Setup(m => m.GetUserAsync(user.UserId).Result)
                .Returns(user);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            UserDTO dto = userService.GetUserAsync(user.UserId).Result;

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(user.UserId), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(dto.UserId, Is.EqualTo(user.UserId));
                Assert.That(dto.Name, Is.EqualTo(user.Name));
                Assert.That(dto.Email, Is.EqualTo(user.Email));
                Assert.That(dto.CreatedOn, Is.EqualTo(user.CreatedOn));
                Assert.That(dto.Role, Is.EqualTo(user.Role));
            });
        }

        [Test]
        public static void UpdateUser_NoUserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            const int id = 1;

            mockUserRepo.Setup(m => m.GetUserAsync(id).Result)
                .Returns((User?)null);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            Assert.ThrowsAsync(Is.TypeOf<UserNotFoundException>()
                               .And.Message.Contains(id.ToString(CultureInfo.InvariantCulture)),
                () => userService.UpdateUserAsync(id, null!));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(id), Times.Once());
        }

        [Test]
        public static void UpdateUser_EmailTaken()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = CreateUser(email: "oldEmail");
            string newEmail = "email";

            mockUserRepo.Setup(m => m.GetUserAsync(user.UserId).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.CheckEmailAvailableAsync(newEmail).Result)
                .Returns(false);

            var userService = CreateUserService(mockUserRepo.Object);

            var req = new UpdateUserReq()
            {
                Email = newEmail
            };

            // Act
            Assert.ThrowsAsync(Is.TypeOf<EmailInUseException>()
                               .And.Message.Contains(req.Email),
                () => userService.UpdateUserAsync(user.UserId, req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(user.UserId), Times.Once());
            mockUserRepo.Verify(m => m.CheckEmailAvailableAsync(req.Email), Times.Once());
        }

        [Test]
        public static void UpdateUser_EmailAvailable()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = CreateUser(email: "oldEmail");
            string newEmail = "email";

            mockUserRepo.Setup(m => m.GetUserAsync(user.UserId).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.CheckEmailAvailableAsync(newEmail).Result)
                .Returns(true);
            mockUserRepo.Setup(m => m.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            var userService = CreateUserService(mockUserRepo.Object);

            var req = new UpdateUserReq()
            {
                Email = newEmail
            };

            // Act
            userService.UpdateUserAsync(user.UserId, req).Wait();

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(user.UserId), Times.Once());
            mockUserRepo.Verify(m => m.CheckEmailAvailableAsync(req.Email), Times.Once());
            mockUserRepo.Verify(m => m.UpdateUserAsync(user), Times.Once());
        }

        [Test]
        public static void UpdateUser_UpdateName()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new();

            var user = CreateUser();

            mockUserRepo.Setup(m => m.GetUserAsync(user.UserId).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            var userService = CreateUserService(mockUserRepo.Object);

            var req = new UpdateUserReq()
            {
                Name = "Christopher"
            };

            // Act
            userService.UpdateUserAsync(user.UserId, req).Wait();

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(user.UserId), Times.Once());
            mockUserRepo.Verify(m => m.CheckEmailAvailableAsync(It.IsAny<string>()), Times.Never());
            mockUserRepo.Verify(m => m.UpdateUserAsync(user), Times.Once());
        }

        [Test]
        public static void Login_NoUserExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            LoginReq req = new()
            {
                Email = "email",
                Password = "12345"
            };

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(req.Email).Result)
                .Returns((User?)null);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            Assert.ThrowsAsync<InvalidCredentialsException>(() => userService.LoginAsync(req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(req.Email), Times.Once());
        }

        [Test]
        public static void Login_EmailNotConfirmed()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            LoginReq req = new()
            {
                Email = "email",
                Password = "12345"
            };

            var user = CreateUser(email: req.Email);

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(req.Email).Result)
                .Returns(user);

            var userService = CreateUserService(mockUserRepo.Object);

            // Act
            Assert.ThrowsAsync<InvalidCredentialsException>(() => userService.LoginAsync(req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(req.Email), Times.Once());
        }

        [Test]
        public static void Login_WrongCredentials()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<IPasswordHelper> mockPswHelper = new(MockBehavior.Strict);

            LoginReq req = new()
            {
                Email = "email",
                Password = "12345"
            };

            string actualPassword = "123";
            var user = CreateUser(email: req.Email, passwordHash: actualPassword,
                emailConfirmed: true);

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(req.Email).Result)
                .Returns(user);

            mockPswHelper.Setup(m => m.IsValid(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string a, string b) => a.Equals(b, StringComparison.Ordinal));

            var userService = CreateUserService(mockUserRepo.Object, mockPswHelper.Object);

            // Act
            Assert.ThrowsAsync<InvalidCredentialsException>(() => userService.LoginAsync(req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(req.Email), Times.Once());
            mockPswHelper.Verify(m => m.IsValid(req.Password, actualPassword), Times.Once());
        }

        [Test]
        public static void Login_Normal()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<IPasswordHelper> mockPswHelper = new(MockBehavior.Strict);

            LoginReq req = new()
            {
                Email = "email",
                Password = "12345"
            };

            var user = CreateUser(email: req.Email, passwordHash: req.Password,
                emailConfirmed: true);

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(req.Email).Result)
                .Returns(user);

            mockPswHelper.Setup(m => m.IsValid(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string a, string b) => a.Equals(b, StringComparison.Ordinal));

            var userService = CreateUserService(mockUserRepo.Object, mockPswHelper.Object);

            // Act
            LoginDTO dto = userService.LoginAsync(req).Result;

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync(req.Email), Times.Once());
            mockPswHelper.Verify(m => m.IsValid(req.Password, req.Password), Times.Once());
            Assert.That(dto.UserId, Is.EqualTo(user.UserId));
        }

        [Test]
        public static void ConfirmUser_InvalidToken()
        {
            // Arrange
            Mock<IJwtService> mockJwtService = new(MockBehavior.Strict);

            JwtSecurityToken? token = default;
            mockJwtService.Setup(m => m.ValidateToken(It.IsAny<string>(), out token))
                .Returns((string t, out JwtSecurityToken? outT) =>
                {
                    outT = null;
                    return false;
                });

            var userService = CreateUserService(jwtService: mockJwtService.Object);

            // Act
            Assert.ThrowsAsync<InvalidTokenException>(() =>
                userService.ConfirmUserAsync(string.Empty));

            // Assert
            mockJwtService.Verify(m => m.ValidateToken(string.Empty, out token), Times.Once());

            Assert.That(token, Is.Null);
        }

        [Test]
        public static void ConfirmUser_UserNotFound()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<IJwtService> mockJwtService = new(MockBehavior.Strict);

            mockUserRepo.Setup(m => m.GetUserAsync(It.IsAny<int>()).Result)
                .Returns((User?)null);

            const int userId = 1;

            JwtSecurityToken? token = new(claims:
                        [new("sub_id", userId.ToString(CultureInfo.InvariantCulture))]);
            mockJwtService.Setup(m => m.ValidateToken(It.IsAny<string>(), out token))
                .Returns(true);

            var userService = CreateUserService(mockUserRepo.Object,
                jwtService: mockJwtService.Object);

            // Act
            Assert.ThrowsAsync(Is.TypeOf<UserNotFoundException>()
                               .And.Message.Contains(userId.ToString(CultureInfo.InvariantCulture)),
                () => userService.ConfirmUserAsync(null!));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(userId), Times.Once());
            mockJwtService.Verify(m => m.ValidateToken(null!, out token), Times.Once());
        }

        [Test]
        public static void ConfirmUser_AlreadyConfirmed()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<IJwtService> mockJwtService = new(MockBehavior.Strict);

            const int userId = 1;
            var user = CreateUser(id: userId, emailConfirmed: true);

            mockUserRepo.Setup(m => m.GetUserAsync(It.IsAny<int>()).Result)
                .Returns(user);

            JwtSecurityToken? token = new(claims:
                        [new("sub_id", userId.ToString(CultureInfo.InvariantCulture))]);
            mockJwtService.Setup(m => m.ValidateToken(It.IsAny<string>(), out token))
                .Returns(true);

            var userService = CreateUserService(mockUserRepo.Object,
                jwtService: mockJwtService.Object);

            // Act
            Assert.ThrowsAsync<UserAlreadyConfirmedException>(() => 
                userService.ConfirmUserAsync(null!));

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(userId), Times.Once());
            mockJwtService.Verify(m => m.ValidateToken(null!, out token), Times.Once());
        }

        [Test]
        public static void ConfirmUser_Normal()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<IJwtService> mockJwtService = new(MockBehavior.Strict);

            const int userId = 1;
            var user = CreateUser(id: userId);

            mockUserRepo.Setup(m => m.GetUserAsync(It.IsAny<int>()).Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            JwtSecurityToken? token = new(claims:
                        [new("sub_id", userId.ToString(CultureInfo.InvariantCulture))]);
            mockJwtService.Setup(m => m.ValidateToken(It.IsAny<string>(), out token))
                .Returns(true);

            var userService = CreateUserService(mockUserRepo.Object,
                jwtService: mockJwtService.Object);

            // Act
            userService.ConfirmUserAsync(null!).Wait();

            // Assert
            mockUserRepo.Verify(m => m.GetUserAsync(userId), Times.Once());
            mockUserRepo.Verify(m => m.UpdateUserAsync(user), Times.Once());
            mockJwtService.Verify(m => m.ValidateToken(null!, out token), Times.Once());
        }
    }
}
