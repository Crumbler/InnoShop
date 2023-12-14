namespace UserService.Domain.Exceptions
{
    public class InvalidTokenException : BadRequestException
    {
        public InvalidTokenException() : base("The specified security token is invalid") { }
    }
}
