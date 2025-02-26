using Skincare.Services.Interfaces;

namespace Skincare.Services.Implements
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // TODO: Thay thế bằng SMTP, SendGrid, hay dịch vụ email khác
            Console.WriteLine($"Email sent to: {to}\nSubject: {subject}\nBody: {body}");
            await Task.CompletedTask;
        }
    }
}
