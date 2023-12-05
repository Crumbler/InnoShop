using BCrypt.Net;

using UserService.Application.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace UserService.Application.Services
{
    public class PasswordHelper : IPasswordHelper
    {
        public (string hash, string salt) HashPassword(string password)
        {
            string salt = BC.GenerateSalt();
            string hash = BC.HashPassword(password, salt, true, HashType.SHA384);

            return (hash, salt);
        }
    }
}
