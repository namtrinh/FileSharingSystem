// ForgotPasswordModel.cs
using System.ComponentModel.DataAnnotations;

namespace FileSharingSystem.Models
{
    public class ForgotPasswordModel
    {
        public ForgotPasswordInputModel Input { get; set; } = new ForgotPasswordInputModel();

        public class ForgotPasswordInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}
