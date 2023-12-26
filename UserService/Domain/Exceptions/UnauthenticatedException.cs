namespace UserService.Domain.Exceptions
{
    public class UnauthenticatedException : Exception
    {
        protected UnauthenticatedException(string message) : base(message) { }
    }
}
