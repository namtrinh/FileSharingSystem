using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMemoryCache _cache;

    public RegisterConfirmationModel(UserManager<IdentityUser> userManager, IMemoryCache cache)
    {
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        if (_cache.TryGetValue(code, out IdentityUser userData))
        {
            // Tạo tài khoản sau khi xác nhận
            var result = await _userManager.CreateAsync(userData, userData.PasswordHash);
            if (result.Succeeded)
            {
                _cache.Remove(code); // Xóa cache sau khi tạo tài khoản thành công
                return RedirectToPage("RegisterSuccess");
            }
        }
        return RedirectToPage("Error"); // Chuyển đến trang lỗi nếu mã không hợp lệ
    }
}
