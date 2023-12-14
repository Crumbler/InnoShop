namespace UserService.Domain.Exceptions
{
    public class UserAlreadyConfirmedException : ConflictException
    {
        public UserAlreadyConfirmedException() : base("Your account is already confirmed") { }
    }
}
