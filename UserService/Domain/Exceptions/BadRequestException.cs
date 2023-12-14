namespace UserService.Domain.Exceptions
{
    public abstract class BadRequestException(string message) : Exception(message)
    {

    }
}
