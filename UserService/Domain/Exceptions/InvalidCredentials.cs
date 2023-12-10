namespace UserService.Domain.Exceptions
{
    public class InvalidCredentials : UnauthorizedException
    {
        public InvalidCredentials() : base("The specified credentials are invalid") { }
    }
}
