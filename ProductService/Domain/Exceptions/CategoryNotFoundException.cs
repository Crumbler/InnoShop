namespace ProductService.Domain.Exceptions
{
    public class CategoryNotFoundException(int categoryId) : 
        NotFoundException($"Category with id {categoryId} was not found")
    { }
}
