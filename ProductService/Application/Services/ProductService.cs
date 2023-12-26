using ProductService.Application.Interfaces;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Domain.Exceptions;
using ProductService.Application.Options;
using ProductService.Infrastructure.Data.Entities;
using LinqKit;

namespace ProductService.Application.Services
{
    public class ProductService(IProductRepository productRepository,
        ICategoryRepository categoryRepository, BrowsingOptions browsingOptions) : IProductService
    {
        public async Task<Product> CreateProductAsync(int userId, CreateProductReq req)
        {
            Category category = await categoryRepository.GetCategoryAsync(req.CategoryId) ??
                throw new CategoryNotFoundException(req.CategoryId);

            var product = new Product()
            {
                Category = category,
                Description = req.Description,
                Name = req.Name,
                Price = req.Price,
                IsAvailable = true,
                UserId = userId
            };

            product = await productRepository.CreateProductAsync(product);

            return product;
        }

        public async Task DeleteProductAsync(int userId, bool isAdmin, int productId)
        {
            Product product = await productRepository.GetProductAsync(productId)
                ?? throw new ProductNotFoundException(productId);

            if (product.UserId != userId && !isAdmin)
            {
                throw new OtherUserAdminOnlyException();
            }

            await productRepository.DeleteProductAsync(productId);
        }

        public Task<Category[]> GetCategoriesAsync()
        {
            return categoryRepository.GetCategoriesAsync();
        }

        public async Task<Product> GetProductAsync(int id)
        {
            Product? product = await productRepository.GetProductAsync(id) ??
                throw new ProductNotFoundException(id);

            return product;
        }

        public Task<Product[]> GetProductsAsync(GetProductsReq req)
        {
            var predicate = PredicateBuilder.New<Product>(true);

            if (req.SearchName != null)
            {
                predicate = predicate.And(p => p.Name.Contains(req.SearchName, 
                    StringComparison.InvariantCultureIgnoreCase));
            }

            if (req.SearchDesc != null)
            {
                predicate = predicate.And(p => p.Description.Contains(req.SearchDesc,
                    StringComparison.InvariantCultureIgnoreCase));
            }

            switch ((req.MinPrice, req.MaxPrice))
            {
                case (null, decimal maxP):
                    predicate = predicate.And(p => p.Price <= maxP);
                    break;
                case (decimal minP, null):
                    predicate = predicate.And(p => p.Price >= minP);
                    break;
                case (decimal minP, decimal maxP):
                    predicate = predicate.And(p => p.Price >= minP && p.Price <= maxP);
                    break;
            }

            if (req.UserId != null)
            {
                predicate = predicate.And(p => p.UserId == req.UserId);
            }

            if (req.Availability != null)
            {
                predicate = predicate.And(p => p.IsAvailable == req.Availability);
            }

            switch ((req.MinDate, req.MaxDate))
            {
                case (null, DateTime maxDate):
                    predicate = predicate.And(p => p.CreatedOn <= maxDate);
                    break;
                case (DateTime minDate, null):
                    predicate = predicate.And(p => p.CreatedOn >= minDate);
                    break;
                case (DateTime minDate, DateTime maxDate):
                    predicate = predicate.And(p => p.CreatedOn >= minDate && p.CreatedOn <= maxDate);
                    break;
            }

            if (req.CategoryId != null)
            {
                predicate = predicate.And(p => p.Category.CategoryId == req.CategoryId);
            }

            return productRepository.GetProductsAsync(predicate, req.Page ?? 1, browsingOptions.PageSize);
        }

        public async Task UpdateProductAsync(int userId, bool isAdmin, int productId,
            UpdateProductReq req)
        {
            Product product = await productRepository.GetProductAsync(productId)
                ?? throw new ProductNotFoundException(productId);

            if (product.UserId != userId && !isAdmin)
            {
                throw new OtherUserAdminOnlyException();
            }

            if (req.IsAvailable != null)
            {
                product.IsAvailable = (bool)req.IsAvailable;
            }

            if (req.Price != null)
            {
                product.Price = (decimal)req.Price;
            }

            if (req.Name != null)
            {
                product.Name = req.Name;
            }

            if (req.Description != null)
            {
                product.Description = req.Description;
            }

            await productRepository.UpdateProductAsync(product);
        }
    }
}
