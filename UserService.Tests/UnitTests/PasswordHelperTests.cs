using UserService.Application.Services;

namespace UserService.Tests.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class PasswordHelperTests
    {
        private static readonly PasswordHelper helper = new();

        [Test]
        public static void ValidHash()
        {
            // Arrange
            const string password = "SomePassword";

            // Act
            string hash = helper.HashPassword(password);

            // Assert
            bool res = helper.IsValid(password, hash);

            Assert.That(res);
        }

        [Test]
        public static void InvalidHash()
        {
            bool res = helper.IsValid("SomePassword", helper.HashPassword("OtherPassword"));

            Assert.That(!res);
        }
    }
}
