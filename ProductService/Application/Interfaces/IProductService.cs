using ProductService.Application.Requests;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<Product> GetProductAsync(int id);
        Task<Product[]> GetProductsAsync(GetProductsReq req);
        Task<Category[]> GetCategoriesAsync();
        Task<Product> CreateProductAsync(int userId, CreateProductReq req);
        Task UpdateProductAsync(int userId, bool isAdmin, int productId, UpdateProductReq req);
        Task DeleteProductAsync(int userId, bool isAdmin, int productId);
    }
}
