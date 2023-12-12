using UserService.Application.Models;

namespace UserService.Application.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(Email email);
    }
}
