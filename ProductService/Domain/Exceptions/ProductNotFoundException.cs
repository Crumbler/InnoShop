namespace ProductService.Domain.Exceptions
{
    public class ProductNotFoundException(int productId) : 
        NotFoundException($"The product with the identifier {productId} was not found.")
    { }
}
