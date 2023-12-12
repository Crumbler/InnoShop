namespace UserService.Application.Models
{
    public class Email
    {
        public required string RecepientAddress { get; set; }
        public required string RecipientName { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
