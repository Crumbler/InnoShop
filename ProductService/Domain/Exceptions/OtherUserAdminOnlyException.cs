namespace ProductService.Domain.Exceptions
{
    public class OtherUserAdminOnlyException : UnauthorizedException
    {
        public OtherUserAdminOnlyException() : base("You are unauthorized to edit or delete other users' products")
        { }
    }
}
