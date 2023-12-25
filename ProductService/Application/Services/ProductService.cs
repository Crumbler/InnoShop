using ProductService.Application.Interfaces;
using ProductService.Application.Requests;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class ProductService(IProductRepository productRepository) : IProductService
    {
        public Task<Product> CreateProductAsync(int userId, CreateProductReq req)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProductAsync(int userId, bool isAdmin, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<Category[]> GetCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Product> GetProductAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Product[]> GetProductsAsync(GetProductsReq req)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProductAsync(int userId, bool isAdmin, int productId, UpdateProductReq req)
        {
            throw new NotImplementedException();
        }
    }
}
