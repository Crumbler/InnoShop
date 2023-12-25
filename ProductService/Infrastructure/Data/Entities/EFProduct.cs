using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Infrastructure.Data.Entities
{
    [Table("Products")]
    public class EFProduct
    {
        [Key]
        public int ProductId { get; set; }

        [MaxLength(30)]
        public required string Name { get; set; }

        [MaxLength(300)]
        public required string Description { get; set; }

        [Precision(12, 4)]
        public decimal Price { get; set; }
        public int UserId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedOn { get; set; }
        public required EFCategory Category { get; set; }

        public Product ToProduct() => new()
        {
            ProductId = ProductId,
            Name = Name,
            Description = Description,
            Price = Price,
            UserId = UserId,
            IsAvailable = IsAvailable,
            CreatedOn = CreatedOn,
            Category = Category.ToCategory()
        };

        public static EFProduct FromProduct(Product product) => new()
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            UserId = product.UserId,
            IsAvailable = product.IsAvailable,
            CreatedOn = product.CreatedOn,
            Category = EFCategory.FromCategory(product.Category)
        };
    }
}
