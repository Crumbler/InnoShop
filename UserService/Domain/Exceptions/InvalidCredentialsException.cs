namespace UserService.Domain.Exceptions
{
    public class InvalidCredentialsException : UnauthorizedException
    {
        public InvalidCredentialsException() : base("The specified credentials are invalid") { }
    }
}
