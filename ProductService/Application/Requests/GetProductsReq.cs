using ProductService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Requests
{
    public class GetProductsReq : IValidatableObject
    {
        [MaxLength(30)]
        public string? SearchName { get; set; }

        [MaxLength(50)]
        public string? SearchDesc { get; set; }

        [Range(1.0, 100000.0)]
        public decimal? MinPrice { get; set; }

        [Range(1.0, 100000.0)]
        public decimal? MaxPrice { get; set; }
        public int? UserId { get; set; }
        public bool? Availability { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int? CategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        public SortBy? SortBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice != null && MaxPrice != null && MinPrice > MaxPrice)
            {
                yield return new ValidationResult("The minimum price must not be higher than the maximum price",
                    [nameof(MinPrice), nameof(MaxPrice)]);
            }

            if (MinDate != null && MaxDate != null && MinDate > MaxDate)
            {
                yield return new ValidationResult("The minimum date must not be higher than the maximum date",
                    [nameof(MinDate), nameof(MaxDate)]);
            }
        }
    }
}
