using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace UserService.Tests.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class UserServiceTests
    {
        private static IFutureDockerImage serviceImage;
        private static IContainer serviceContainer;

        [OneTimeSetUp]
        public static async Task Setup()
        {
            var factory = LoggerFactory.Create(o => o.AddDebug());
            TestcontainersSettings.Logger = factory.CreateLogger("logger");

            serviceImage = new ImageFromDockerfileBuilder()
                .WithName("userservicetest")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "UserService")
                .WithDockerfile("Dockerfile")
                .Build();

            await serviceImage.CreateAsync();

            serviceContainer = new ContainerBuilder()
                .Build();

            await serviceContainer.StartAsync();
        }

        [Test]
        public static void UnauthenticatedResponses()
        {
            
        }

        [OneTimeTearDown]
        public static async Task TearDown()
        {
            await serviceImage.DisposeAsync();

            if (serviceContainer != null)
            {
                await serviceContainer.DisposeAsync();
            }
        }
    }
}
