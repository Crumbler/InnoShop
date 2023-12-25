using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category[]> GetCategoriesAsync();
    }
}
