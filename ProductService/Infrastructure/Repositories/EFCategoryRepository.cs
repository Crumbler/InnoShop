using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories
{
    public class EFCategoryRepository(ProductServiceDbContext dbContext) : ICategoryRepository
    {
        public Task<Category[]> GetCategoriesAsync() =>
            dbContext.Categories
                .AsNoTracking()
                .Select(c => c.ToCategory())
                .ToArrayAsync();

        public async Task<Category?> GetCategoryAsync(int id)
        {
            return (await dbContext.Categories.FindAsync(id))?.ToCategory();
        }
    }
}
