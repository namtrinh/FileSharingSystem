using System.Threading.Tasks;

namespace FileSharingSystem.Services // Namespace phải khớp với EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
