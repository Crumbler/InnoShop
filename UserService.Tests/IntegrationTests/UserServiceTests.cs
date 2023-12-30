using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Options;
using UserService.Application.Requests;

namespace UserService.Tests.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class UserServiceTests
    {
        private static HttpClient client;
        private static WebApplicationFactory<UserService.Program> factory;
        private static JwtOptions jwtOptions;
        private static FakeEmailService emailService;

        [OneTimeSetUp]
        public static void Setup()
        {
            emailService = new FakeEmailService();

            factory = new WebApplicationFactory<UserService.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton<IEmailService>(emailService);
                    });
                });

            jwtOptions = factory.Services.GetRequiredService<JwtOptions>();

            client = factory.CreateClient();
        }

        [Test]
        public static async Task UpdateUser_DeleteUser_Unauthenticated()
        {
            // Arrange
            const string path = "users/1";
            Task<HttpResponseMessage>[] tasks = [client.PutAsync(path, null),
                client.DeleteAsync(path)];

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(res => res.StatusCode),
                Is.All.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public static async Task UpdateUser_DeleteUser_NotFound()
        {
            // Arrange
            var loginDto = await LoginAsAdmin();

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", loginDto.Token);

            const string path = "users/-50";

            Task<HttpResponseMessage>[] tasks =
                [
                    client.PutAsJsonAsync(path, new UpdateUserReq()),
                    client.DeleteAsync(path)
                ];

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(s => s.StatusCode), 
                Is.All.EqualTo(HttpStatusCode.NotFound));

            client.DefaultRequestHeaders.Authorization = null;
        }

        [Test]
        public static async Task UpdateUser_DeleteUser_OtherUser_EmailTaken()
        {
            (UserDTO user, string password) = await CreateUser();

            var loginDto = await LoginAsUser(user.Email, password);

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", loginDto.Token);

            var req = new UpdateUserReq()
            {
                Email = "johndoe@mail.com"
            };

            // Email taken
            var res = await client.PutAsJsonAsync($"users/{loginDto.UserId}", req);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            // Can't update other users if not admin
            res = await client.PutAsJsonAsync($"users/1", req);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

            req.Email = "newemail@mail.com";
            req.Name = "New Name";

            res = await client.PutAsJsonAsync($"users/{loginDto.UserId}", req);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            // Can't delete other users if not admin
            res = await client.DeleteAsync($"users/-5");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

            res = await client.DeleteAsync($"users/{loginDto.UserId}");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            client.DefaultRequestHeaders.Authorization = null;
        }

        [Test]
        public static async Task UpdateDeleteOtherUser_AsAdmin()
        {
            var user = (await CreateUser()).dto;

            var loginDto = await LoginAsAdmin();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginDto.Token);

            string path = $"users/{user.UserId}";

            var res = await client.PutAsJsonAsync(path, new UpdateUserReq()
            {
                Name = "New Name"
            });
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            res = await client.DeleteAsync(path);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            client.DefaultRequestHeaders.Authorization = null;
        }

        [Test]
        public static async Task GetUser()
        {
            // Act
            var res = await client.GetAsync("users/1");
            UserDTO? dto = await res.Content.ReadFromJsonAsync<UserDTO>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(dto, Is.Not.Null);
                Assert.That(dto?.UserId, Is.GreaterThan(0));
                Assert.That(dto?.Name, Is.Not.Empty);
                Assert.That(dto?.Email, Is.Not.Empty);
                Assert.That(dto?.CreatedOn, Is.Not.Default);
                Assert.That(dto?.Role, Is.Not.Null);
                Assert.That(dto?.Role.RoleId, Is.GreaterThan(0));
                Assert.That(dto?.Role.Name, Is.Not.Empty);
            });
        }

        [Test]
        public static async Task GetUser_NotFound()
        {
            // Act
            var res = await client.GetAsync("users/-25");

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public static async Task Login_BadRequest()
        {
            // Arrange
            LoginReq?[] requests = [null,
                new()
                {
                    Email = "notEmail",
                    Password = "invalidPassword"
                },
                new()
                {
                    Email = "mail@email.com",
                    Password = "invalidPassword"
                },
                new()
                {
                    Email = "notEmail",
                    Password = "Pass12345"
                }];

            // Act
            var tasks = requests.Select(req => client.PostAsJsonAsync("login", req));
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(res => res.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task Login_InvalidCredentials()
        {
            // Act
            var res = await client.PostAsJsonAsync("login", new LoginReq()
            {
                Email = "mail@mail.com",
                Password = "Pass12345"
            });

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public static async Task Login()
        {
            // Act
            var res = await client.PostAsJsonAsync("login", new LoginReq()
            {
                Email = "johndoe@mail.com",
                Password = "abc12345"
            });

            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            LoginDTO? dto = await res.Content.ReadFromJsonAsync<LoginDTO>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto, Is.Not.Null);
                Assert.That(dto?.UserId, Is.GreaterThan(0));
                Assert.That(dto?.Token, Is.Not.Empty);
            });
        }

        [Test]
        public static async Task CreateUser_BadRequest()
        {
            // Arrange
            CreateUserReq?[] requests = [null,
                new()
                {
                    Name = "a",
                    Email = "notEmail",
                    Password = "invalidPassword"
                },
                new()
                {
                    Name = "a",
                    Email = "mail@email.com",
                    Password = "abc12345"
                },
                new()
                {
                    Name = "SomeName",
                    Email = "mail@email.com",
                    Password = "invalidPassword"
                },
                new()
                {
                    Name = "SomeName",
                    Email = "notEmail",
                    Password = "Pass12345"
                }];

            // Act
            var tasks = requests.Select(req => client.PostAsJsonAsync("users", req));
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(res => res.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task CreateUser_EmailTaken()
        {
            // Arrange
            CreateUserReq req = new()
            {
                Name = "SomeName",
                Email = "johndoe@mail.com",
                Password = "abc12345"
            };

            // Act
            var res = await client.PostAsJsonAsync("users", req);

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public static async Task CreateUser_ConfirmUser_AlreadyConfirmed()
        {
            var req = new CreateUserReq()
            {
                Email = "bob@mail.com",
                Name = "Bob",
                Password = "abc12345"
            };

            var res = await client.PostAsJsonAsync("users", req);
            UserDTO? dto = await res.Content.ReadFromJsonAsync<UserDTO>();

            Assert.Multiple(() =>
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
                Assert.That(dto, Is.Not.Null);
                Assert.That(dto?.Name, Is.EqualTo(req.Name));
                Assert.That(dto?.Email, Is.EqualTo(req.Email));
                Assert.That(res.Headers.Location, Is.Not.Null);
                Assert.That(res.Headers.Location?.Segments[^1], 
                    Is.EqualTo(dto?.UserId.ToString()));
            });

            string token = await emailService.GetToken();

            res = await client.PostAsync($"confirm/{token}", null);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            res = await client.PostAsync($"confirm/{token}", null);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            // Cleanup
            await DeleteUser(dto!.UserId);
        }

        [Test]
        public static async Task ForgotPassword_BadRequest()
        {
            // Arrange
            ForgotPasswordReq?[] requests = [null,
                new()
                {
                    Email = "notEmail"
                },
                new()
                {
                    Email = "a"
                }];

            // Act
            var tasks = requests.Select(req => client.PostAsJsonAsync("forgotpassword", req));
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(res => res.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task ForgotPassword_ResetPassword_SamePassword()
        {
            UserDTO user = (await CreateUser()).dto;

            var forgotPasswordReq = new ForgotPasswordReq()
            { 
                Email = user.Email 
            };

            var res = await client.PostAsJsonAsync("forgotpassword", forgotPasswordReq);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            string token = await emailService.GetToken();

            var resetPasswordReq = new ResetPasswordReq()
            {
                Password = "cdf12345"
            };

            res = await client.PostAsJsonAsync($"resetpassword/{token}", resetPasswordReq);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            res = await client.PostAsJsonAsync($"resetpassword/{token}", resetPasswordReq);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            // Cleanup
            await DeleteUser(user.UserId);
        }

        [Test]
        public static async Task ResetPassword_BadRequest()
        {
            // Arrange
            ResetPasswordReq?[] requests = [null,
                new()
                {
                    Password = "123456789"
                },
                new()
                {
                    Password = "aaaabbbbcccc"
                },
                new()
                {
                    Password = "a1"
                }];

            // Act
            var tasks = requests.Select(req => client.PostAsJsonAsync("resetpassword/token", req));
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(res => res.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task ResetPassword_ConfirmEmail_InvalidToken()
        {
            // Arrange
            Task<HttpResponseMessage>[] tasks =
                [client.PostAsJsonAsync<object?>("resetpassword/token", null),
                    client.PostAsJsonAsync<object?>("confirm/token", null)];

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task ResetPassword_ConfirmEmail_ExpiredToken()
        {
            // Arrange
            string expiredToken = TestHelper.GenerateExpiredJwtToken(factory.Services);
            Task<HttpResponseMessage>[] tasks =
                [client.PostAsJsonAsync<object?>($"resetpassword/{expiredToken}", null),
                    client.PostAsJsonAsync($"confirm/{expiredToken}", new ResetPasswordReq()
                    {
                        Password = "abc12345"
                    })];

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        /// <summary>
        /// Used for deleting temporary users after tests
        /// </summary>
        private static async Task DeleteUser(int id)
        {
            var dto = await LoginAsAdmin();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"users/{id}");
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", dto.Token);

            await client.SendAsync(request);
        }

        private static Task<LoginDTO> LoginAsAdmin()
        {
            return LoginAsUser("johndoe@mail.com", "abc12345");
        }

        private static async Task<LoginDTO> LoginAsUser(string email, string password)
        {
            var res = await client.PostAsJsonAsync("login", new LoginReq()
            {
                Email = email,
                Password = password
            });

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception("Failed to successfully login with credentials: " +
                    $"${email} {password}");
            }

            LoginDTO dto = await res.Content.ReadFromJsonAsync<LoginDTO>() ??
                throw new Exception("Failed to successfully login with credentials: " +
                    $"${email} {password}");

            return dto;
        }

        /// <summary>
        /// Creates a dummy user
        /// </summary>
        private static async Task<(UserDTO dto, string password)> CreateUser()
        {
            var req = new CreateUserReq()
            {
                Email = "bob@mail.com",
                Name = "Bob",
                Password = "abc12345"
            };

            var res = await client.PostAsJsonAsync("users", req);
            UserDTO dto = await res.Content.ReadFromJsonAsync<UserDTO>()
                ?? throw new Exception("Failed to create user");

            string token = await emailService.GetToken();

            res = await client.PostAsync($"confirm/{token}", null);

            if (res.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception("Failed to create user");
            }

            return (dto, req.Password);
        }

        [OneTimeTearDown]
        public static async Task TearDown()
        {
            client.Dispose();

            await factory.DisposeAsync();
        }
    }
}
