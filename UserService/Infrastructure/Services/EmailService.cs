using MailKit.Net.Smtp;
using MimeKit;
using UserService.Application.Interfaces;
using UserService.Application.Models;
using UserService.Application.Options;

namespace UserService.Infrastructure.Services
{
    public class EmailService(EmailOptions emailOptions, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendEmailAsync(Email email)
        {
            var message = new MimeMessage();
            
            message.From.Add(new MailboxAddress("UserService", "no-reply@mail.com"));
            message.To.Add(new MailboxAddress(email.RecipientName, email.RecepientAddress));
            message.Subject = email.Subject;

            message.Body = new TextPart("plain")
            {
                Text = email.Body
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(emailOptions.SmtpServer, 25, false);

                await client.SendAsync(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send email");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
