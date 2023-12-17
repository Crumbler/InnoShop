using Microsoft.AspNetCore.Routing;
using Moq;
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
                               .And.Message.Contains(userId.ToString()), 
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
                               .And.Message.Contains(userId.ToString()),
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
    }
}
