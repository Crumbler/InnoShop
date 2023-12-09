using BCrypt.Net;

using UserService.Application.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace UserService.Application.Services
{
    public class PasswordHelper : IPasswordHelper
    {
        public string HashPassword(string password)
        {
            string hash = BC.EnhancedHashPassword(password, HashType.SHA384);

            return hash;
        }
    }
}
