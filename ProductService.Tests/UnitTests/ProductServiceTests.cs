using LinqKit;
using Moq;
using ProductService.Application.Options;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;
using System.Globalization;
using System.Linq.Expressions;

namespace ProductService.Tests.UnitTests
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public static class ProductServiceTests
    {
        private static readonly BrowsingOptions browsingOptions = new()
        {
            PageSize = 5
        };

        private static Application.Services.ProductService CreateProductService(
            IProductRepository? productRepository = null,
            ICategoryRepository? categoryRepository = null,
            BrowsingOptions? browsingOptions = null)
        {
            return new(productRepository ?? Mock.Of<IProductRepository>(),
                categoryRepository ?? Mock.Of<ICategoryRepository>(),
                browsingOptions ?? ProductServiceTests.browsingOptions);
        }

        private static Product Product(int productId = 1, string name = "Name",
            string description = "Description", decimal price = 10,
            int userId = 1, bool isAvailable = true, DateTime? createdOn = null,
            int categoryId = 1) => new()
            {
                ProductId = productId,
                Name = name,
                Description = description,
                Price = price,
                UserId = userId,
                IsAvailable = isAvailable,
                CreatedOn = createdOn ?? DateTime.UtcNow,
                Category = new Category()
                {
                    CategoryId = categoryId,
                    Name = null!
                },
            };

        [Test]
        public static void CreateProduct_CategoryNotFound()
        {
            // Arrange
            Mock<ICategoryRepository> mockCategoryRepo = new(MockBehavior.Strict);

            mockCategoryRepo.Setup(m => m.GetCategoryAsync(It.IsAny<int>()).Result)
                .Returns((Category?)null);

            var productService = CreateProductService(categoryRepository: mockCategoryRepo.Object);

            var req = new CreateProductReq()
            { 
                CategoryId = 1, 
                Description = null!,
                Name = null!
            };

            // Act
            Assert.ThrowsAsync(Is.TypeOf<CategoryNotFoundException>()
                               .And.Message.Contains(req.CategoryId.ToString(CultureInfo.InvariantCulture)),
                () => productService.CreateProductAsync(1, req));

            // Assert
            mockCategoryRepo.Verify(m => m.GetCategoryAsync(req.CategoryId), Times.Once());
        }

        [Test]
        public static void CreateProduct()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);
            Mock<ICategoryRepository> mockCategoryRepo = new(MockBehavior.Strict);

            mockProductRepo.Setup(m => m.CreateProductAsync(It.IsAny<Product>()).Result)
                .Returns((Product p) => p);

            mockCategoryRepo.Setup(m => m.GetCategoryAsync(It.IsAny<int>()).Result)
                .Returns((int id) => new Category() 
                { 
                    CategoryId = id, 
                    Name = null! 
                });

            var productService = CreateProductService(mockProductRepo.Object,
                mockCategoryRepo.Object);

            var req = new CreateProductReq()
            {
                CategoryId = 1,
                Description = null!,
                Name = null!
            };

            const int userId = 1;

            // Act
            var product = productService.CreateProductAsync(userId, req).Result;

            // Assert
            mockCategoryRepo.Verify(m => m.GetCategoryAsync(req.CategoryId), Times.Once());
            mockProductRepo.Verify(m => m.CreateProductAsync(It.IsAny<Product>()), 
                Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(product.IsAvailable);
                Assert.That(product.Name, Is.EqualTo(req.Name));
                Assert.That(product.Price, Is.EqualTo(req.Price));
                Assert.That(product.Description, Is.EqualTo(req.Description));
                Assert.That(product.Category.CategoryId, Is.EqualTo(req.CategoryId));
                Assert.That(product.UserId, Is.EqualTo(userId));
                Assert.That(product.CreatedOn,
                    Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
            });

        }

        [Test]
        public static void DeleteProduct_NotFound()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            mockProductRepo.Setup(p => p.GetProductAsync(It.IsAny<int>()).Result)
                .Returns((Product?)null);

            var productService = CreateProductService(mockProductRepo.Object);

            const int productId = 1;

            // Act
            Assert.ThrowsAsync(Is.TypeOf<ProductNotFoundException>()
                               .And.Message.Contains(productId.ToString(CultureInfo.InvariantCulture)),
                () => productService.DeleteProductAsync(default, default, productId));

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(productId), Times.Once());
        }

        [Test]
        public static void DeleteProduct_OtherUser_Unauthorized()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            const int userId = 2;
            var product = Product(userId: 1);

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);

            var productService = CreateProductService(mockProductRepo.Object);

            // Act
            Assert.ThrowsAsync<OtherUserAdminOnlyException>(() => 
                productService.DeleteProductAsync(userId, false, product.ProductId));

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
        }

        [Test]
        public static void DeleteProduct_OtherUser()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            const int userId = 2;
            var product = Product(userId: 1);

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);
            mockProductRepo.Setup(m => m.DeleteProductAsync(product.ProductId))
                .Returns(Task.CompletedTask);

            var productService = CreateProductService(mockProductRepo.Object);

            // Act
            productService.DeleteProductAsync(userId, true, product.ProductId).Wait();

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
            mockProductRepo.Verify(m => m.DeleteProductAsync(product.ProductId), Times.Once());
        }

        [Test]
        public static void DeleteProduct()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            var product = Product();

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);
            mockProductRepo.Setup(m => m.DeleteProductAsync(product.ProductId))
                .Returns(Task.CompletedTask);

            var productService = CreateProductService(mockProductRepo.Object);

            // Act
            productService.DeleteProductAsync(product.UserId, false, product.ProductId).Wait();

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
            mockProductRepo.Verify(m => m.DeleteProductAsync(product.ProductId), Times.Once());
        }

        [Test]
        public static void GetCategories()
        {
            // Arrange
            Mock<ICategoryRepository> mockCategoryRepo = new(MockBehavior.Strict);

            Category[] categories = [ 
                new()
                { 
                    CategoryId = 1, 
                    Name = "First category"
                },
                new()
                {
                    CategoryId = 2,
                    Name = "Second category"
                }
            ];


            mockCategoryRepo.Setup(m => m.GetCategoriesAsync().Result)
                .Returns(categories);

            var productService = CreateProductService(categoryRepository: mockCategoryRepo.Object);

            // Act
            var resCategories = productService.GetCategoriesAsync().Result;

            // Assert
            mockCategoryRepo.Verify(m => m.GetCategoriesAsync(), Times.Once());

            Assert.That(categories, Is.EquivalentTo(resCategories));
        }

        [Test]
        public static void GetProduct_NotFound()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            mockProductRepo.Setup(m => m.GetProductAsync(It.IsAny<int>()).Result)
                .Returns((Product?)null);

            var productService = CreateProductService(mockProductRepo.Object);

            const int productId = 1;

            // Act
            Assert.ThrowsAsync(Is.TypeOf<ProductNotFoundException>()
                               .And.Message.Contains(productId.ToString(CultureInfo.InvariantCulture)),
                () => productService.GetProductAsync(productId));

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(productId), Times.Once());
        }

        [Test]
        public static void GetProduct()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            var product = Product();

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);

            var productService = CreateProductService(mockProductRepo.Object);

            // Act
            var resProduct = productService.GetProductAsync(product.ProductId).Result;

            // Assert
            Assert.That(resProduct, Is.EqualTo(product));

            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
        }

        [Test]
        public static void UpdateProduct_NotFound()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            mockProductRepo.Setup(m => m.GetProductAsync(It.IsAny<int>()).Result)
                .Returns((Product?)null);

            var productService = CreateProductService(mockProductRepo.Object);

            const int productId = 1;

            // Act
            Assert.ThrowsAsync(Is.TypeOf<ProductNotFoundException>()
                               .And.Message.Contains(productId.ToString(CultureInfo.InvariantCulture)),
                () => productService.UpdateProductAsync(default, default, productId, null!));

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(productId), Times.Once());
        }

        [Test]
        public static void UpdateProduct_OtherUser_Unauthorized()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            const int userId = 2;
            var product = Product(userId: 1);

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);

            var productService = CreateProductService(mockProductRepo.Object);

            // Act
            Assert.ThrowsAsync<OtherUserAdminOnlyException>(() =>
                productService.UpdateProductAsync(userId, false, product.ProductId, null!));

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
        }

        [Test]
        public static void UpdateProduct_OtherUser()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            const int userId = 2;
            var product = Product(userId: 1);

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);
            mockProductRepo.Setup(m => m.UpdateProductAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var productService = CreateProductService(mockProductRepo.Object);

            var req = new UpdateProductReq()
            {
                IsAvailable = false,
                Description = "New description",
                Price = 1000M
            };

            // Act
            productService.UpdateProductAsync(userId, true, product.ProductId, req).Wait();

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
            mockProductRepo.Verify(m => m.UpdateProductAsync(product), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(product.IsAvailable, Is.EqualTo(req.IsAvailable));
                Assert.That(product.Description, Is.EqualTo(req.Description));
                Assert.That(product.Price, Is.EqualTo(req.Price));
            });
        }

        [Test]
        public static void UpdateProduct()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            var product = Product();

            mockProductRepo.Setup(m => m.GetProductAsync(product.ProductId).Result)
                .Returns(product);
            mockProductRepo.Setup(m => m.UpdateProductAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var productService = CreateProductService(mockProductRepo.Object);

            var req = new UpdateProductReq()
            {
                IsAvailable = false,
                Description = "New description",
                Price = 1000M
            };

            // Act
            productService.UpdateProductAsync(product.UserId, true, product.ProductId, req).Wait();

            // Assert
            mockProductRepo.Verify(m => m.GetProductAsync(product.ProductId), Times.Once());
            mockProductRepo.Verify(m => m.UpdateProductAsync(product), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(product.IsAvailable, Is.EqualTo(req.IsAvailable));
                Assert.That(product.Description, Is.EqualTo(req.Description));
                Assert.That(product.Price, Is.EqualTo(req.Price));
            });
        }

        [Test]
        public static void GetProducts()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            Expression<Func<Product, bool>> expr = null!;

            mockProductRepo.Setup(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<SortBy>(), It.IsAny<int>(), It.IsAny<int>()).Result)
                .Returns((Expression<Func<Product, bool>> predicate, SortBy sortType, int page, int pageSize) =>
                {
                    expr = predicate;
                    return [];
                });

            var productService = CreateProductService(mockProductRepo.Object,
                browsingOptions: new BrowsingOptions()
                { 
                    PageSize = 5
                });

            var req = new GetProductsReq();

            // Act
            productService.GetProductsAsync(req).Wait();

            // Assert
            mockProductRepo.Verify(m => 
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    SortBy.Name, 1, browsingOptions.PageSize), Times.Once());

            Assert.That(expr.Invoke(Product()));
        }

        [Test]
        public static void GetProducts_FilterByNameAndDescription()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            Expression<Func<Product, bool>> expr = null!;

            mockProductRepo.Setup(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<SortBy>(), It.IsAny<int>(), It.IsAny<int>()).Result)
                .Returns((Expression<Func<Product, bool>> predicate, SortBy sortType, int page, int pageSize) =>
                {
                    expr = predicate;
                    return [];
                });

            var productService = CreateProductService(mockProductRepo.Object,
                browsingOptions: new BrowsingOptions()
                {
                    PageSize = 5
                });

            var req = new GetProductsReq()
            {
                SearchName = "name",
                SearchDesc = "desc"
            };

            // Act
            productService.GetProductsAsync(req).Wait();

            // Assert
            mockProductRepo.Verify(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    SortBy.Name, 1, browsingOptions.PageSize), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(expr.Invoke(Product(name: "name", description: "desc")));
                Assert.That(expr.Invoke(Product(name: "name", description: "Other")), 
                    Is.Not.True);
                Assert.That(expr.Invoke(Product(name: "Other", description: "desc")),
                    Is.Not.True);
            });
        }

        [Test]
        public static void GetProducts_FilterByPrice()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            Expression<Func<Product, bool>> expr = null!;

            mockProductRepo.Setup(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<SortBy>(), It.IsAny<int>(), It.IsAny<int>()).Result)
                .Returns((Expression<Func<Product, bool>> predicate, SortBy sortType, int page, int pageSize) =>
                {
                    expr = predicate;
                    return [];
                });

            var productService = CreateProductService(mockProductRepo.Object,
                browsingOptions: new BrowsingOptions()
                {
                    PageSize = 5
                });

            var req = new GetProductsReq()
            {
                MinPrice = 50M,
                MaxPrice = 100M
            };

            // Act
            productService.GetProductsAsync(req).Wait();

            // Assert
            mockProductRepo.Verify(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    SortBy.Name, 1, browsingOptions.PageSize), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(expr.Invoke(Product(price: 10M)), Is.Not.True);
                Assert.That(expr.Invoke(Product(price: 70M)));
                Assert.That(expr.Invoke(Product(price: 150M)), Is.Not.True);
            });
        }

        [Test]
        public static void GetProducts_FilterByUserIdAndAvailability()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            Expression<Func<Product, bool>> expr = null!;

            mockProductRepo.Setup(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<SortBy>(), It.IsAny<int>(), It.IsAny<int>()).Result)
                .Returns((Expression<Func<Product, bool>> predicate, SortBy sortType, int page, int pageSize) =>
                {
                    expr = predicate;
                    return [];
                });

            var productService = CreateProductService(mockProductRepo.Object,
                browsingOptions: new BrowsingOptions()
                {
                    PageSize = 5
                });

            var req = new GetProductsReq()
            {
                UserId = 1,
                Availability = false
            };

            // Act
            productService.GetProductsAsync(req).Wait();

            // Assert
            mockProductRepo.Verify(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    SortBy.Name, 1, browsingOptions.PageSize), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(expr.Invoke(Product(userId: req.UserId.Value, 
                    isAvailable: !req.Availability.Value)), Is.Not.True);
                Assert.That(expr.Invoke(Product(userId: req.UserId.Value + 1, 
                    isAvailable: req.Availability.Value)), Is.Not.True);
                Assert.That(expr.Invoke(Product(userId: req.UserId.Value, 
                    isAvailable: req.Availability.Value)));
            });
        }

        [Test]
        public static void GetProducts_FilterByDateAndCategory()
        {
            // Arrange
            Mock<IProductRepository> mockProductRepo = new(MockBehavior.Strict);

            Expression<Func<Product, bool>> expr = null!;

            mockProductRepo.Setup(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<SortBy>(), It.IsAny<int>(), It.IsAny<int>()).Result)
                .Returns((Expression<Func<Product, bool>> predicate, SortBy sortType, int page, int pageSize) =>
                {
                    expr = predicate;
                    return [];
                });

            var productService = CreateProductService(mockProductRepo.Object,
                browsingOptions: new BrowsingOptions()
                {
                    PageSize = 5
                });

            var req = new GetProductsReq()
            {
                MinDate = new DateTime(2020, 1, 1),
                MaxDate = new DateTime(2021, 1, 1),
                CategoryId = 5,
                SortBy = SortBy.DateDesc
            };

            // Act
            productService.GetProductsAsync(req).Wait();

            // Assert
            mockProductRepo.Verify(m =>
                m.GetProductsAsync(It.IsAny<Expression<Func<Product, bool>>>(),
                    req.SortBy.Value, 1, browsingOptions.PageSize), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(expr.Invoke(Product(createdOn: new DateTime(2019, 12, 31), categoryId: req.CategoryId.Value)), 
                    Is.Not.True);
                Assert.That(expr.Invoke(Product(createdOn: new DateTime(2020, 12, 31), categoryId: req.CategoryId.Value)));
                Assert.That(expr.Invoke(Product(createdOn: new DateTime(2020, 12, 31), categoryId: req.CategoryId.Value + 1)),
                    Is.Not.True);
                Assert.That(expr.Invoke(Product(createdOn: new DateTime(2022, 1, 1), categoryId: req.CategoryId.Value)), 
                    Is.Not.True);
            });
        }
    }
}
