namespace ProductService.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; init; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int UserId { get; init; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public required Category Category { get; init; }
    }
}
