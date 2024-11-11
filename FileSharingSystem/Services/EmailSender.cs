using System.Net.Mail;
using System.Net;
using FileSharingSystem.Services;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("cunnconn01@gmail.com", "avro nali vpwj grip"), // Sử dụng mật khẩu ứng dụng nếu cần
            EnableSsl = false, // Đặt SSL thành false để kiểm tra
            Timeout = 20000 // Đặt thời gian chờ là 10 giây
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("cunnconn01@gmail.com"),
            Subject = subject,
            Body = message,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Ghi lại thông báo lỗi
            Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
            throw new InvalidOperationException("Gửi email không thành công", ex);
        }
    }
}