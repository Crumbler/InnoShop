using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests
{
    public class UpdateProductReq
    {
        [MaxLength(30)]
        public string? Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        [Range(1.0, 100000.0)]
        public decimal? Price { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
