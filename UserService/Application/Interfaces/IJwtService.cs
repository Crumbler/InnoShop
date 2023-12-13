using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IJwtService
    {
        public string GetAuthenticationToken(User user);
        public string GetEmailConfirmationToken(User user);
    }
}
