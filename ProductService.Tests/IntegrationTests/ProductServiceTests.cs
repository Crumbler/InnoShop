using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Presentation.Options;

namespace ProductService.Tests.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class ProductServiceTests
    {
        const int adminUserId = 1, normalUserId = 2;

        private static HttpClient client;
        private static WebApplicationFactory<ProductService.Program> factory;
        private static string normalToken, adminToken;
        private static AuthenticationHeaderValue normalAuthHeader, adminAuthHeader;

        [OneTimeSetUp]
        public static void Setup()
        {
            var rsa = RSA.Create();

            var jwtOptions = new JwtOptions()
            {
                Audience = "Audience",
                IsAdminClaimType = "admin",
                Issuer = "Issuer",
                RsaPublicKey = rsa.ExportRSAPublicKeyPem(),
                UserIdClaimType = "sub_id"
            };

            var key = new RsaSecurityKey(rsa);

            normalToken = TestHelper.GenerateJwtToken(key, jwtOptions, normalUserId, false);
            adminToken = TestHelper.GenerateJwtToken(key, jwtOptions, adminUserId, true);

            normalAuthHeader = new AuthenticationHeaderValue("Bearer", normalToken);
            adminAuthHeader = new AuthenticationHeaderValue("Bearer", adminToken);

            factory = new WebApplicationFactory<ProductService.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(jwtOptions);
                        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                            options =>
                        {
                            options.TokenValidationParameters.IssuerSigningKey = key;
                            options.TokenValidationParameters.ValidIssuer = jwtOptions.Issuer;
                            options.TokenValidationParameters.ValidAudience = jwtOptions.Audience;
                        });
                    });
                });

            client = factory.CreateClient();
        }

        [Test]
        public static async Task GetProduct_NotFound()
        {
            // Act
            var res = await client.GetAsync("products/-25");

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public static async Task GetProduct()
        {
            // Act
            var res = await client.GetAsync("products/1");
            var product = await res.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(product, Is.Not.Null);
                Assert.That(product?.UserId, Is.GreaterThan(0));
                Assert.That(product?.Name, Is.Not.Empty);
                Assert.That(product?.Description, Is.Not.Empty);
                Assert.That(product?.CreatedOn, Is.Not.Default);
                Assert.That(product?.ProductId, Is.GreaterThan(0));
                Assert.That(product?.Price, Is.Not.Default);
                Assert.That(product?.IsAvailable, Is.Not.Null);
                Assert.That(product?.Category, Is.Not.Null);
                Assert.That(product?.Category.Name, Is.Not.Empty);
                Assert.That(product?.Category.CategoryId, Is.GreaterThan(0));
            });
        }

        [Test]
        public static async Task GetProducts_BadRequest()
        {
            // Arrange
            string[] paths =
            [
                "page=-1",
                "sortby=none",
                "searchname=" + new string('a', 40),
                "searchdesc=" + new string('a', 60),
                "minprice=0",
                "maxprice=0",
                "minprice=50&maxprice=40",
                "mindate=2020-1-1&maxdate=2019-12-31"
            ];

            // Act
            var res = await Task.WhenAll(paths.Select(p => client.GetAsync("products?" + p)));

            // Assert
            Assert.That(res.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task GetProducts_Sorted([Values] SortBy sortType)
        {
            // Arrange
            string query = "sortBy=" + sortType.ToString();

            // Act
            var products = await client.GetFromJsonAsync<Product[]>("products?" + query);

            // Assert
            Assert.That(products, Has.Length.AtLeast(2));

            var constraint = sortType switch
            {
                SortBy.NameDesc => Is.Ordered.Descending.By(nameof(Product.Name)),
                SortBy.Date => Is.Ordered.By(nameof(Product.CreatedOn)),
                SortBy.DateDesc => Is.Ordered.Descending.By(nameof(Product.CreatedOn)),
                _ => Is.Ordered.By(nameof(Product.Name)),
            };

            Assert.That(products, constraint);
        }

        [Test]
        public static async Task GetProducts_Paging()
        {
            // Arrange
            var tasks = Enumerable.Range(1, 3)
                .Select(i => client.GetFromJsonAsync<IEnumerable<Product>>($"products?page={i}"));

            // Act
            var arrays = await Task.WhenAll(tasks);

            // Assert
            Assert.That(arrays, Is.Not.Null.And.All.Not.Null);
            Assert.That(arrays.Aggregate((a1, a2) =>
                a1?.Concat(a2 ?? Enumerable.Empty<Product>()) ?? a2),
                Is.Unique);
        }

        [Test]
        public static async Task GetProducts_Name()
        {
            // Arrange
            const string name = "BoArD";

            // Act
            var products = await client.GetFromJsonAsync<IEnumerable<Product>>
                ($"products?searchname={name}");

            // Assert
            Assert.That(products, Is.Not.Empty);
            Assert.That(products.Select(p => p.Name), Is.All.Contain(name).IgnoreCase);
        }

        [Test]
        public static async Task GetProducts_Description()
        {
            // Arrange
            const string description = "OUt";

            // Act
            var products = await client.GetFromJsonAsync<IEnumerable<Product>>
                ($"products?searchdesc={description}");

            // Assert
            Assert.That(products, Is.Not.Empty);
            Assert.That(products.Select(p => p.Description), Is.All.Contain(description).IgnoreCase);
        }

        [Test]
        public static async Task GetProducts_Price()
        {
            // Arrange
            const decimal minPrice = 20M,
                maxPrice = 200M;

            // Act
            var products = await client.GetFromJsonAsync<IEnumerable<Product>>
                ($"products?minprice={minPrice}&maxprice={maxPrice}");

            // Assert
            Assert.That(products, Is.Not.Empty);
            Assert.That(products.Select(p => p.Price), Is.All.InRange(minPrice, maxPrice));
        }

        [Test]
        public static async Task GetCategories()
        {
            // Act
            var res = await client.GetAsync("products/categories");
            var categories = await res.Content.ReadFromJsonAsync<Category[]>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(categories, Is.Not.Empty);
                Assert.That(categories, Is.All.Not.Null);
                Assert.That(categories?.Select(c => c.Name), Is.All.Not.Empty);
                Assert.That(categories?.Select(c => c.CategoryId), Is.All.GreaterThan(0));
            });
        }

        [Test]
        public static async Task DeleteUpdateProduct_NotFound()
        {
            // Arrange
            const string path = "products/-1";

            HttpRequestMessage[] messages =
            [
                Message(HttpMethod.Delete, path),
                Message(HttpMethod.Put, path, new UpdateProductReq())
            ];

            // Act
            var res = await Task.WhenAll(messages.Select(m => client.SendAsync(m)));

            // Assert
            Assert.That(res.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public static async Task DeleteUpdateProduct_Unauthorized()
        {
            // Arrange
            const string path = "products/1";

            HttpRequestMessage[] messages =
            [
                Message(HttpMethod.Delete, path),
                Message(HttpMethod.Put, path, new UpdateProductReq())
            ];

            // Act
            var res = await Task.WhenAll(messages.Select(m => client.SendAsync(m)));

            // Assert
            Assert.That(res.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public static async Task CreateUpdateDeleteProduct_Unauthenticated()
        {
            // Arrange
            Task<HttpResponseMessage>[] tasks =
            [
                client.PostAsJsonAsync<CreateProductReq?>("products", null),
                client.PutAsJsonAsync<UpdateProductReq?>("products/1", null),
                client.DeleteAsync("products/1")
            ];

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.That(results.Select(s => s.StatusCode),
                Is.All.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public static async Task CreateProduct_CategoryNotFound()
        {
            // Arrange
            var req = new CreateProductReq()
            {
                CategoryId = -1,
                Description = "Description",
                Name = "Name",
                Price = 1M
            };

            var message = Message(HttpMethod.Post, "products", req);

            // Act
            var res = await client.SendAsync(message);

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public static async Task CreateProduct_BadRequest()
        {
            // Arrange
            const string path = "products";
            var method = HttpMethod.Post;

            HttpRequestMessage[] requests =
            [
                Message(method, path, new CreateProductReq()
                {
                    Name = null!,
                    Description = "Description",
                    Price = 1M,
                    CategoryId = 1
                }),
                Message(method, path, new CreateProductReq()
                {
                    Name = "Name",
                    Description = null!,
                    Price = 1M,
                    CategoryId = 1
                }),
                Message(method, path, new CreateProductReq()
                {
                    Name = "Name",
                    Description = "Description",
                    Price = decimal.MaxValue,
                    CategoryId = 1
                })
            ];

            // Act
            var res = await Task.WhenAll(requests.Select(r => client.SendAsync(r)));

            // Assert
            Assert.That(res.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task CreateProduct()
        {
            // Arrange
            var req = new CreateProductReq()
            {
                Description = "Description",
                Name = "Name",
                CategoryId = 1,
                Price = 10M
            };

            var message = Message(HttpMethod.Post, "products", req);

            // Act
            var res = await client.SendAsync(message);
            Product? product;

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            product = await res.Content.ReadFromJsonAsync<Product>();

            Assert.That(product, Is.Not.Null);

            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(product!.Name, Is.EqualTo(req.Name));
                    Assert.That(product!.Description, Is.EqualTo(req.Description));
                    Assert.That(product!.Category.CategoryId, Is.EqualTo(req.CategoryId));
                    Assert.That(product!.Price, Is.EqualTo(req.Price));
                    Assert.That(product!.UserId, Is.EqualTo(normalUserId));
                    Assert.That(product!.IsAvailable);
                });
            }
            finally
            {
                await DeleteProduct(product!.ProductId);
            }
        }

        [Test]
        public static async Task UpdateProduct([Values] bool isAdmin)
        {
            // Arrange
            var product = await CreateDummyProduct();

            var req = new UpdateProductReq()
            {
                Price = 50M
            };

            var message = Message(HttpMethod.Put, $"products/{product.ProductId}",
                req, isAdmin);

            // Act
            var res = await client.SendAsync(message);

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            // Cleanup
            await DeleteProduct(product.ProductId);
        }

        [Test]
        public static async Task UpdateProduct_BadRequest()
        {
            // Arrange
            const string path = "products/1";
            var method = HttpMethod.Put;

            HttpRequestMessage[] requests =
            [
                Message(method, path, new UpdateProductReq()
                {
                    Description = new string('a', 400),
                }),
                Message(method, path, new UpdateProductReq()
                {
                    Name = new string('a', 40)
                }),
                Message(method, path, new UpdateProductReq()
                {
                    Price = decimal.MaxValue
                })
            ];

            // Act
            var res = await Task.WhenAll(requests.Select(r => client.SendAsync(r)));

            // Assert
            Assert.That(res.Select(r => r.StatusCode),
                Is.All.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public static async Task DeleteProduct([Values] bool isAdmin)
        {
            // Arrange
            var product = await CreateDummyProduct();

            var message = Message(HttpMethod.Delete, $"products/{product.ProductId}", isAdmin);

            // Act
            var res = await client.SendAsync(message);

            // Assert
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Return an HttpRequestMessage using a JWT token for authorization
        /// </summary>
        private static HttpRequestMessage Message<T>(HttpMethod method, string path,
            T? content = null, bool isAdmin = false)
            where T : class
        {
            var message = new HttpRequestMessage(method, path)
            {
                Content = JsonContent.Create(content)
            };

            message.Headers.Authorization = isAdmin ? adminAuthHeader : normalAuthHeader;

            return message;
        }

        /// <summary>
        /// Return an HttpRequestMessage using a JWT token for authorization
        /// </summary>
        private static HttpRequestMessage Message(HttpMethod method, string path,
            bool isAdmin = false)
        {
            var message = new HttpRequestMessage(method, path);

            message.Headers.Authorization = isAdmin ? adminAuthHeader : normalAuthHeader;

            return message;
        }

        /// <summary>
        /// Creates a dummy product as a normal user
        /// </summary>
        private static async Task<Product> CreateDummyProduct()
        {
            var message = Message(HttpMethod.Post, "products", new CreateProductReq()
            {
                Description = "Description",
                Name = "Name",
                CategoryId = 1,
                Price = 1M
            });

            var res = await client.SendAsync(message);

            if (res.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception("Failed to create dummy product");
            }

            Product product = await res.Content.ReadFromJsonAsync<Product>() ??
                throw new Exception("Failed to create dummy product");

            return product;
        }

        /// <summary>
        /// Used for deleting temporary products after tests
        /// </summary>
        private static async Task DeleteProduct(int id)
        {
            var message = Message(HttpMethod.Delete, "products/" + id, (object?)null, true);

            var res = await client.SendAsync(message);

            if (res.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception($"Failed to delete product {id}");
            }
        }

        [OneTimeTearDown]
        public static async Task TearDown()
        {
            client.Dispose();

            await factory.DisposeAsync();
        }
    }
}
