namespace UserService.Domain.Exceptions
{
    public class OtherUserAdminOnlyException : UnauthorizedException
    {
        public OtherUserAdminOnlyException() : base("You are unauthorized to update and delete other users")
        { }
    }
}
