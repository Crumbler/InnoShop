namespace UserService.Domain.Exceptions
{
    public class UserNotFoundException(int userId) : 
        NotFoundException($"The user with the identifier {userId} was not found.")
    { }
}
