namespace UserService.Application.Options
{
    public class EmailOptions
    {
        public const string Email = "Email";

        public required string SmtpServer { get; set; }
    }
}
