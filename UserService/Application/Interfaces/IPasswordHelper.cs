namespace UserService.Application.Interfaces
{
    public interface IPasswordHelper
    {
        public string HashPassword(string password);
        public bool IsValid(string password, string hash);
    }
}
