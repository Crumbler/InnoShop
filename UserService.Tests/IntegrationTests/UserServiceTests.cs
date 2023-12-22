using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UserService.Tests.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public class UserServiceTests
    { 
        private static HttpClient client;
        private static WebApplicationFactory<UserService.Program>? factory;

        [OneTimeSetUp]
        public static void Setup()
        {
            factory = new WebApplicationFactory<UserService.Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
            client = factory.CreateClient();
        }

        [Test]
        public static async Task UnauthenticatedResponses()
        {
            // Arrange
            var request1 = new HttpRequestMessage(HttpMethod.Put, "users/1");
            var request2 = new HttpRequestMessage(HttpMethod.Delete, "users/25");

            // Act
            var response1 = await client.SendAsync(request1);
            var response2 = await client.SendAsync(request2);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            });
        }

        [OneTimeTearDown]
        public static async Task TearDown()
        {
            client.Dispose();

            if (factory != null)
            {
                await factory.DisposeAsync();
            }
        }
    }
}
