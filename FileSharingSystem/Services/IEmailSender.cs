using System.Threading.Tasks;

namespace FileSharingSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
