using Microsoft.AspNetCore.Routing;
using Moq;
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

        [Test]
        public static void CreateUser_UserWithConfirmedEmailExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);

            var user = new User()
            {
                Email = "email",
                Name = "name",
                PasswordHash = "passwordhash",
                Role = null!,
                IsEmailConfirmed = true
            };

            mockUserRepo.Setup(m => m.GetUserByEmailAsync("email").Result)
                .Returns(user);

            var userService = CreateUserService(mockUserRepo.Object);

            var req = new CreateUserReq()
            {
                Email = "email",
                Name = "John Doe",
                Password = "12345"
            };

            // Act
            Assert.ThrowsAsync<EmailInUseException>(() => userService.CreateUserAsync(req));

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync("email"), Times.Once());
        }

        [Test]
        public static void CreateUser_UserWithUnconfirmedEmailExists()
        {
            // Arrange
            Mock<IUserRepository> mockUserRepo = new(MockBehavior.Strict);
            Mock<LinkGenerator> mockLinkGen = new();

            var user = new User()
            {
                Email = "email",
                Name = "name",
                PasswordHash = "passwordhash",
                Role = null!
            };

            mockUserRepo.Setup(m => m.GetUserByEmailAsync("email").Result)
                .Returns(user);
            mockUserRepo.Setup(m => m.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            mockLinkGen.SetReturnsDefault(string.Empty);

            var userService = CreateUserService(mockUserRepo.Object,
                linkGenerator: mockLinkGen.Object);

            var req = new CreateUserReq()
            {
                Email = "email",
                Name = "John Doe",
                Password = "12345"
            };

            // Act
            var dto = userService.CreateUserAsync(req).Result;

            // Assert
            mockUserRepo.Verify(m => m.GetUserByEmailAsync("email"), Times.Once());
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

            var user = new User()
            {
                Email = "email",
                Name = "John Doe",
                PasswordHash = null!,
                Role = null!
            };

            mockUserRepo.Setup(m => m.GetUserByEmailAsync(user.Email).Result)
                .Returns<IUserRepository, User>(null);
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
            mockUserRepo.Verify(m => m.GetUserByEmailAsync("email"), Times.Once());
            mockUserRepo.Verify(m => m.CreateUserAsync(It.IsAny<User>()), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo(user.Name));
                Assert.That(dto.Email, Is.EqualTo(user.Email));
            });
        }
    }
}
