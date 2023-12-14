namespace UserService.Domain.Exceptions
{
    public class SamePasswordException : ConflictException
    {
        public SamePasswordException() : base("The password you specified is identical to your current password") { }
    }
}
