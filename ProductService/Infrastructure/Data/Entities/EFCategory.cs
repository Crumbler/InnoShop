using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Infrastructure.Data.Entities
{
    [Table("Categories")]
    public class EFCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [MaxLength(30)]
        public required string Name { get; set; }

        public Category ToCategory() => new()
        {
            CategoryId = CategoryId,
            Name = Name
        };

        public static EFCategory FromCategory(Category category) => new()
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }
}
