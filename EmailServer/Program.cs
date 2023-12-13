using MimeKit;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Storage;
using System.Buffers;
using System.Net;

namespace EmailServer
{
    public class MessageStore : IMessageStore
    {
        public async Task<SmtpServer.Protocol.SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }

            stream.Position = 0;

            var message = await MimeMessage.LoadAsync(stream, cancellationToken);

            Console.WriteLine(message);

            return SmtpServer.Protocol.SmtpResponse.Ok;
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var endpoint = IPEndPoint.Parse("127.0.0.1:25");

            var options = new SmtpServerOptionsBuilder()
                .ServerName("mail.com")
                .Endpoint(b => b.Endpoint(endpoint))
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new MessageStore());

            var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
            Task task = smtpServer.StartAsync(CancellationToken.None);

            Console.WriteLine("Email server started");

            task.Wait();
        }
    }
}
