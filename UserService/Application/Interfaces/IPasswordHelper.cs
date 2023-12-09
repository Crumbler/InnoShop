namespace UserService.Application.Interfaces
{
    public interface IPasswordHelper
    {
        public string HashPassword(string password);
    }
}
