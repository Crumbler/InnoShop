
using Moq;
using ProductService.Application.Options;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using ProductService.Domain.Repositories;
using System.Globalization;

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

        private static Product CreateProduct(int productId = 1, string name = "Name",
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
    }
}
