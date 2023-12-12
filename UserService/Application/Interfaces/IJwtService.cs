using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IJwtService
    {
        public string GetJwtToken(User user);
    }
}
