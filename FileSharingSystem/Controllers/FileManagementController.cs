using FileSharingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Security.Claims;

[Authorize]
public class FileManagementController : Controller
{
    private readonly IFileService _fileService;

    public FileManagementController(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _fileService.GetAllFilesAsync();
        if (model == null)
        {
            model = new List<FileModel>();
        }
        return View(model);
    }
    public async Task<IActionResult> UserFiles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng từ Claims
        var model = await _fileService.GetFilesByUserIdAsync(userId);
        return View(model); // Trả về danh sách tệp của người dùng
    }

    public async Task<IActionResult> Open(int id)
    {
        try
        {
            // Tìm kiếm tệp dựa trên id
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy tệp
            }

            // Lấy stream của tệp từ FileService
            var fileStream = await _fileService.DownloadFileAsync(id);
            var contentType = GetContentType(file.FileName);

            // Trả về tệp với Content-Type
            return File(fileStream, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // Maximum file size allowed (5MB)
        const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        // Danh sách các phần mở rộng tệp không được phép tải lên
        var forbiddenExtensions = new[] { ".bat", ".sh", ".py", ".exe" };

        // Kiểm tra tệp có được chọn không
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Vui lòng chọn tệp để tải lên.");
            // Lấy lại danh sách các tệp đã tải lên để hiển thị lại
            var model = await _fileService.GetAllFilesAsync();
            return View(model);
        }

        // Check if file exceeds the maximum file size
        if (file.Length > MaxFileSize)
        {
            TempData["ErrorMessage"] = "Kích thước tệp vượt quá giới hạn 5KB.";
            return RedirectToAction("Index", "Home"); // Redirect to HomeController's Index with an error message
        }

        // Kiểm tra phần mở rộng của tệp tải lên
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (forbiddenExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("", "Tệp này không được phép tải lên: .exe, .bat, .sh, .py không được phép.");
            // Lấy lại danh sách các tệp đã tải lên để hiển thị lại
            var model = await _fileService.GetAllFilesAsync();
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng từ Claims
        await _fileService.UploadFileAsync(file, userId); // Gọi UploadFileAsync với userId
        TempData["SuccessMessage"] = "Đã cập nhật thành công tệp!";
        return RedirectToAction("Index", "Home");
    }




    public async Task<IActionResult> Download(int id)
    {
        try
        {
            // Tìm kiếm tệp dựa trên fileId
            var files = await _fileService.GetAllFilesAsync();
            var file = files.FirstOrDefault(f => f.Id == id);

            if (file == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy tệp
            }

            // Lấy stream của tệp từ FileService
            var fileStream = await _fileService.DownloadFileAsync(id);
            var contentType = GetContentType(file.FileName);

            // Trả về tệp với Content-Type và tên tệp
            return File(fileStream, contentType, file.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        // Lấy ID người dùng từ Claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Tìm kiếm tệp dựa trên fileId
        var file = await _fileService.GetFileByIdAsync(id);

        if (file == null)
        {
            return NotFound(); // Trả về 404 nếu không tìm thấy tệp
        }

        // Kiểm tra xem người dùng có quyền xóa tệp không
        if (file.UserId != userId) // Giả định rằng file.UserId lưu ID của người dùng đã tải lên
        {
            TempData["ErrorMessage"] = "Bạn không có quyền xóa tệp này."; // Lưu thông báo vào TempData
            return RedirectToAction("Index"); // Chuyển hướng về trang chính
        }

        await _fileService.DeleteFileAsync(id);
        return RedirectToAction("Index");
    }



    // Hàm xác định Content-Type dựa vào phần mở rộng của tệp
    private string GetContentType(string fileName)
    {
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream"; // Sử dụng cho các loại tệp không xác định
        }
        return contentType;
    }

}
