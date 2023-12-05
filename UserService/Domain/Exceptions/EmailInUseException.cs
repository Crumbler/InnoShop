namespace UserService.Domain.Exceptions
{
    public class EmailInUseException(string email) : 
        ConflictException($"The email {email} is already in use.")
    { }
}
