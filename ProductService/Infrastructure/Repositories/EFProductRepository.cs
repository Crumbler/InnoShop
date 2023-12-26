using LinqKit.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Data.Entities;
using System.Linq.Expressions;

namespace ProductService.Infrastructure.Repositories
{
    public class EFProductRepository(ProductServiceDbContext dbContext) : IProductRepository
    {
        public async Task<Product> CreateProductAsync(Product product)
        {
            var efProduct = EFProduct.FromProduct(product);

            dbContext.Products.Entry(efProduct).State = EntityState.Added;

            await dbContext.SaveChangesAsync();

            return efProduct.ToProduct();
        }

        public Task DeleteProductAsync(int id)
        {
            return dbContext.Products.Where(p => p.ProductId == id).ExecuteDeleteAsync();
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            EFProduct? product = await dbContext.Products
                .Include(p => p.Category)
                .SingleOrDefaultAsync(p => p.ProductId == id);

            return product?.ToProduct();
        }

        public Task<Product[]> GetProductsAsync(Expression<Func<Product, bool>> filter, int page, int pageSize)
        {
            int toSkip = (page - 1) * pageSize;

            return dbContext.Products
                .AsNoTracking()
                .AsExpandable()
                .Include(p => p.Category)
                .Select(p => p.ToProduct())
                .Where(filter)
                .Skip(toSkip)
                .Take(pageSize)
                .ToArrayAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            EFProduct oldProduct = await dbContext.Products.FindAsync(product.ProductId) ??
                throw new Exception("Product not found");

            var efUser = EFProduct.FromProduct(product);
            dbContext.Products.Entry(oldProduct).CurrentValues.SetValues(efUser);

            await dbContext.SaveChangesAsync();
        }
    }
}
