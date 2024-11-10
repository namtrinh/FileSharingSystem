using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace FileSharingSystem.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
           IMemoryCache cache,
            ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
            _logger = logger;
        }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string code, string returnUrl = null)
        {
            var userCache = _cache.Get(email.Trim());
            if (userCache == null)
            {
                TempData["ErrorMessage"] = "This link has expired.";
                return RedirectToPage("/Account/Login");
            }

            var cacheData = userCache.ToString();
                var parts = cacheData.Split(':');
                    var cachedPassword = parts[0];
                    var cachedCode = parts[1];    

            if (code != cachedCode)
            {
                TempData["ErrorMessage"] = "Mã xác nhận không hợp lệ.";

            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {         
                user = new IdentityUser { Email = email, UserName = email };
                var createResult = await _userManager.CreateAsync(user, cachedPassword.Trim());
                if (!createResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tạo tài khoản người dùng.";
                    _logger.LogInformation("Error creating user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));

                }
            }

                 var userv = await _userManager.FindByEmailAsync(email);
                 var codev = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmResult = await _userManager.ConfirmEmailAsync(user, codev);
            _logger.LogInformation($"3: {confirmResult}");

            if (confirmResult.Succeeded)
            {
                _cache.Remove(email);
                TempData["ErrorMessage"] = "Email của bạn đã được xác nhận và tài khoản của bạn đã được kích hoạt.";     
                return RedirectToPage("/Account/Login");
            }
            TempData["ErrorMessage"] = "Lỗi khi xác nhận email của bạn.";
            return Page();
        }
    }
}
