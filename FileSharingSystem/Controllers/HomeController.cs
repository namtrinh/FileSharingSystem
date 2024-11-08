using FileSharingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileSharingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileService _fileService;
        private readonly ILogger<HomeController> _logger;

        // G?p c? hai constructor l?i
        public HomeController(ILogger<HomeController> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }
        public async Task<IActionResult> Index(string searchQuery, string fileType)
        {
            var model = await _fileService.GetAllFilesAsync();
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            if (model == null)
            {
                model = new List<FileModel>();
            }
            foreach (var file in model)
            {
                file.FileCategory = FileTypeHelper.GetFileCategory(Path.GetExtension(file.FileName));
            }

            // Áp d?ng b? l?c tìm ki?m theo tên t?p n?u có
            if (!string.IsNullOrEmpty(searchQuery))
            {
                model = model.Where(f => f.FileName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Áp d?ng b? l?c theo lo?i t?p n?u có
            if (!string.IsNullOrEmpty(fileType))
            {
                model = model.Where(f => f.FileCategory == fileType).ToList();
            }

            return View("Index", model); // ??m b?o có view "Index" trong th? m?c Home
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}