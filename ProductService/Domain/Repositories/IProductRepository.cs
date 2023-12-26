using ProductService.Domain.Entities;
using System.Linq.Expressions;

namespace ProductService.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetProductAsync(int id);
        Task<Product[]> GetProductsAsync(Expression<Func<Product, bool>> filter, SortBy sortType, int page, int pageSize);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
