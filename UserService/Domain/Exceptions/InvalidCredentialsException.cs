namespace UserService.Domain.Exceptions
{
    public class InvalidCredentialsException : UnauthenticatedException
    {
        public InvalidCredentialsException() : base("The specified credentials are invalid") { }
    }
}
