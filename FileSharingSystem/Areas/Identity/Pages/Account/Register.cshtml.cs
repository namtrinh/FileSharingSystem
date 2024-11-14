using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static FileSharingSystem.Areas.Identity.Pages.Account.ConfirmEmailModel;
namespace FileSharingSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMemoryCache _cache;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ILogger<RegisterModel> logger,
             IMemoryCache cache)

        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _cache = cache;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [RegularExpression(@"^[^/?""'*&%]+$", ErrorMessage = "Password cannot contain special characters like / ? \" ' * & %")]
            public string Password { get; set; }


            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {

                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "This email is already associated with an account.";
                    return Page();
                }


                var email = Input.Email ;
            
                // Tạo mã xác nhận ngẫu nhiên (Code)
                var code = Guid.NewGuid().ToString();

                    String data = $"{Input.Password}:{code}";
                    _cache.Set(email.Trim(), data, TimeSpan.FromMinutes(5));
                    _logger.LogInformation($"Cache entry set for {Input.Email} at {DateTime.UtcNow}");

                var callbackUrl = $"https://trinhnam1-001-site1.ntempurl.com/Identity/Account/ConfirmEmail?email={Input.Email}&code={code}&returnUrl={returnUrl}";


                try
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress("cunnconn01@gmail.com", "FireFlow"), // Đổi tên người gửi thành FireFlow
                        Subject = "Confirm your email",
                        Body = $"Please confirm your account by {HtmlEncoder.Default.Encode(callbackUrl)}",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(Input.Email);

                    SmtpClient smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        Credentials = new NetworkCredential("cunnconn01@gmail.com", "avro nali vpwj grip")
                    };

                    smtp.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending email: " + ex.Message);
                    throw new InvalidOperationException("Error sending email", ex);
                }

                _logger.LogInformation("Confirmation email sent.");

                return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
            }

            return Page();
        }



    }
}