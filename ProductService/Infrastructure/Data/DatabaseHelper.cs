using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data.Entities;

namespace ProductService.Infrastructure.Data
{
    public static class DatabaseHelper
    {
        public static void SetupDatabaseAndSeedData(ProductServiceDbContext context)
        {
            context.Database.Migrate();

            if (context.Categories.Any() || context.Products.Any())
            {
                return;
            }

            int categoryId = 1;

            EFCategory catFurniture = new()
            {
                CategoryId = categoryId++,
                Name = "Furniture"
            },
            catTools = new()
            {
                CategoryId = categoryId++,
                Name = "Tools"
            },
            catSports = new()
            {
                CategoryId = categoryId++,
                Name = "Sports"
            },
            catToys = new()
            {
                CategoryId = categoryId++,
                Name = "Toys"
            },
            catKitchenware = new()
            {
                CategoryId = categoryId++,
                Name = "Kitchenware"
            },
            catClothing = new()
            {
                CategoryId = categoryId++,
                Name = "Clothing"
            };

            int productId = 1;

            EFProduct[] products = [
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Plywood bed",
                    Description = "A bed made of the sturdiest plywood out there",
                    Price = 1399.99M,
                    UserId = 1,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2023, 5, 6),
                    Category = catFurniture
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Marble desk",
                    Description = "Shinier than a Hawaiian pearl",
                    Price = 800M,
                    UserId = 1,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2010, 1, 2),
                    Category = catFurniture
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Cast iron dresser",
                    Description = "Spell caster's robes not included",
                    Price = 759.5M,
                    UserId = 2,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2020, 9, 27),
                    Category = catFurniture
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Magnetic bottle opener",
                    Description = "Bottle corks find it very attractive",
                    Price = 15M,
                    UserId = 3,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2021, 9, 9),
                    Category = catTools
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Ceramic baking tray",
                    Description = "Bake to your heart's content!",
                    Price = 79.99M,
                    UserId = 3,
                    CreatedOn = new DateTime(2015, 12, 20),
                    Category = catKitchenware
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Metal skateboard",
                    Description = "More metal than your local punks",
                    Price = 60M,
                    UserId = 1,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2018, 1, 1),
                    Category = catSports
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Sound-absorbing surfboard",
                    Description = "Chill out while riding those waves",
                    Price = 240M,
                    UserId = 2,
                    CreatedOn = new DateTime(2022, 10, 11),
                    Category = catSports
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Nylon 8 ball",
                    Description = "Indispensable for the indecisive",
                    Price = 8M,
                    UserId = 2,
                    CreatedOn = new DateTime(2008, 8, 8),
                    Category = catToys
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "Copper coffee mug",
                    Description = "Mix some cuprum into those drinks of yours",
                    Price = 10M,
                    UserId = 3,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2012, 11, 11),
                    Category = catKitchenware
                },
                new EFProduct()
                {
                    ProductId = productId++,
                    Name = "3d printed running shoes",
                    Description = "Leave the footprints of your choosing",
                    Price = 100M,
                    UserId = 1,
                    IsAvailable = true,
                    CreatedOn = new DateTime(2023, 12, 20),
                    Category = catSports
                }
            ];

            using var transaction1 = context.Database.BeginTransaction();
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.[Categories] ON");

            context.Categories.AddRange(products
                .Select(p => p.Category)
                .DistinctBy(c => c.CategoryId));
            context.SaveChanges();
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.[Categories] OFF");

            transaction1.Commit();

            using var transaction2 = context.Database.BeginTransaction();

            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.[Products] ON");

            context.Products.AddRange(products);
            context.SaveChanges();

            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.[Products] OFF");

            transaction2.Commit();
        }
    }
}
