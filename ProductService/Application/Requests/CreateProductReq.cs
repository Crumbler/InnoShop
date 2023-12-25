using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests
{
    public class CreateProductReq
    {
        [MaxLength(30)]
        public required string Name { get; set; }

        [MaxLength(300)]
        public required string Description { get; set; }

        [Range(1, 100000)]
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
