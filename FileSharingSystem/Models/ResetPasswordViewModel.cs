// ResetPasswordModel.cs
using System.ComponentModel.DataAnnotations;

namespace FileSharingSystem.Models
{
    public class ResetPasswordModel
    {
        public ResetPasswordInputModel Input { get; set; } = new ResetPasswordInputModel();

        public class ResetPasswordInputModel
        {
            [Required]
            public string Code { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }
        }
    }
}
