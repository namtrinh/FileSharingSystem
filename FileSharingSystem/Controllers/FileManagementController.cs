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

    public async Task<IActionResult> Index(string searchQuery, string fileType)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng từ Claims
        var model = await _fileService.GetFilesByUserIdAsync(userId);

        if (model == null)
        {
            model = new List<FileModel>();
        }
        foreach (var file in model)
        {
            file.FileCategory = FileTypeHelper.GetFileCategory(Path.GetExtension(file.FileName));
        }
        // Áp dụng bộ lọc tìm kiếm theo tên tệp nếu có
        if (!string.IsNullOrEmpty(searchQuery))
        {
            model = model.Where(f => f.FileName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        // Áp dụng bộ lọc theo loại tệp nếu có
        if (!string.IsNullOrEmpty(fileType))
        {
            model = model.Where(f => f.FileCategory == fileType).ToList();
        }


        return View(model);
    }
    public async Task<IActionResult> UserFiles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        var model = await _fileService.GetFilesByUserIdAsync(userId);
        return View(model); 
    }

    public async Task<IActionResult> Open(int id)
    {
        try
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            var fileStream = await _fileService.DownloadFileAsync(id);
            var contentType = GetContentType(file.FileName);
            var encodedFileName = Uri.EscapeDataString(file.FileName);
            if (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" || contentType == "application/msword")
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var doc = new Aspose.Words.Document(memoryStream);
                    var pdfStream = new MemoryStream();
                    doc.Save(pdfStream, Aspose.Words.SaveFormat.Pdf);
                    pdfStream.Position = 0;

                    Response.Headers.Add("Content-Disposition", $"inline; filename=\"{encodedFileName}\"");
                    return File(pdfStream, "application/pdf");
                }
            }

            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{encodedFileName}\"");
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
        const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        var allowedExtensions = new[]
        {
        ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".doc", ".xls", ".xlsx", ".txt", ".rtf", ".ppt", ".pptx", ".odt", ".ods", ".odp",
        ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma", ".m4a", ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".mpeg", ".3gp",
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".ttf", ".otf", ".woff", ".woff2",
        ".csv", ".ics", ".apk", ".iso", ".exe"
    };

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn tệp để tải lên.";
            return RedirectToAction("Index", "Home");
        }

        if (file.Length > MaxFileSize)
        {
            TempData["ErrorMessage"] = "Kích thước tệp vượt quá giới hạn 5MB.";
            return RedirectToAction("Index", "Home");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            TempData["ErrorMessage"] = "Tệp này không được phép tải lên. Vui lòng chọn tệp có định dạng hợp lệ.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadedFile = await _fileService.UploadFileAsync(file, userId);
            TempData["SuccessMessage"] = $"{file.FileName} được xác nhận an toàn. Đã tải lên thành công !";
            TempData["uploadinfo"] = "SAFE" +
                "SCANNER by VIRUSTOTAL";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message; // Lưu thông báo lỗi vào TempData
        }
        return RedirectToAction("Index", "Home");
    }




    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            var fileStream = await _fileService.DownloadFileAsync(id);
            var contentType = GetContentType(file.FileName);

            var encodedFileName = Uri.EscapeDataString(file.FileName);

            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{encodedFileName}\"");

            return File(fileStream, contentType);
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
