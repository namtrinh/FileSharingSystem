using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FileSharingSystem.Services // Đảm bảo namespace khớp với nơi bạn đang sử dụng EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpClient _smtpClient;

        public EmailSender()
        {
            _smtpClient = new SmtpClient("smtp.example.com") // Địa chỉ SMTP của bạn
            {
                Port = 587, // Cổng SMTP
                Credentials = new NetworkCredential("username@example.com", "yourpassword"),
                EnableSsl = true,
            };
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("from@example.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
