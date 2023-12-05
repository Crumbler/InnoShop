namespace UserService.Application.Interfaces
{
    public interface IPasswordHelper
    {
        (string hash, string salt) HashPassword(string password);
    }
}
