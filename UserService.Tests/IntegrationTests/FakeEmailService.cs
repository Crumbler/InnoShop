using System.Threading.Channels;
using UserService.Application.Interfaces;
using UserService.Application.Models;

namespace UserService.Tests.IntegrationTests
{
    /// <summary>
    /// Designed to store at most one token parsed from an email
    /// </summary>
    public class FakeEmailService : IEmailService
    {
        private readonly Channel<string> channel;

        private readonly ChannelReader<string> reader;
        private readonly ChannelWriter<string> writer;

        public FakeEmailService()
        {
            channel = Channel.CreateUnbounded<string>(
                new UnboundedChannelOptions()
                {
                    SingleReader = true,
                    SingleWriter = true
                });

            reader = channel.Reader;
            writer = channel.Writer;
        }

        private static string ParseToken(ReadOnlySpan<char> emailBody)
        {
            int slashIndex = emailBody.LastIndexOf('/');

            return emailBody[(slashIndex + 1)..].ToString();
        }

        public ValueTask<string> GetToken() => reader.ReadAsync();

        public Task SendEmailAsync(Email email)
        {
            string token = ParseToken(email.Body);

            if (!writer.TryWrite(token))
            {
                throw new InvalidOperationException("Failed to write to channel");
            }

            return Task.CompletedTask;
        }
    }
}
