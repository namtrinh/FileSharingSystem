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
        public async Task<IActionResult> Index()
        {
            var model = await _fileService.GetAllFilesAsync();
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(model);

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
