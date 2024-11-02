using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FileSharingSystem.Controllers
{
    public class UserFilesController : Controller
    {
        private readonly IFileService _fileService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserFilesController(IFileService fileService, UserManager<IdentityUser> userManager)
        {
            _fileService = fileService;
            _userManager = userManager;
        }

        // Phương thức cho trang cá nhân
        public async Task<IActionResult> MyFiles()
        {
            var userId = _userManager.GetUserId(User);
            var files = await _fileService.GetFilesByUserIdAsync(userId);
            return View(files);
        }
    }

}
