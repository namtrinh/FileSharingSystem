using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FileSharingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using FileSharingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Identity;

public interface IFileService
{
    Task<List<FileModel>> GetAllFilesAsync();
    Task<FileModel> UploadFileAsync(IFormFile file, string userId);
    Task<bool> DeleteFileAsync(int fileId);
    Task<Stream> DownloadFileAsync(int fileId);
    Task<FileModel> GetFileByIdAsync(int id);
    Task<List<FileModel>> GetFilesByUserIdAsync(string userId);
}

public class FileService : IFileService
{
    private readonly string _fileStoragePath;
    private readonly ApplicationDbContext _context;

    public FileService(string fileStoragePath, ApplicationDbContext context)
    {
        _fileStoragePath = fileStoragePath;
        _context = context;

        if (!Directory.Exists(_fileStoragePath))
        {
            Directory.CreateDirectory(_fileStoragePath);
        }
    }

    public async Task<List<FileModel>> GetAllFilesAsync()
    {
        return await _context.Files.ToListAsync(); // Lấy tất cả các tệp từ database
    }

    public async Task<FileModel> UploadFileAsync(IFormFile file, string userId)
    {
        // Kiểm tra xem tệp có hợp lệ không
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is not valid.");

        var allowedExtensions = new[]
        { 
            // Image formats
            ".jpg", ".jpeg", ".png", ".gif", 

            // Document formats
            ".pdf", ".docx", ".doc", ".xls", ".xlsx", ".txt", ".rtf", ".ppt", ".pptx", ".odt", ".ods", ".odp", 

            // Audio formats
            ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma", ".m4a", 

            // Video formats
            ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".mpeg", ".3gp", 

            // Compressed/archive formats
            ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", 

            // Font files
            ".ttf", ".otf", ".woff", ".woff2", 

            // Other common files
            ".csv", ".ics", ".apk", ".iso" , ".exe"
        };

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var filePath = Path.Combine(_fileStoragePath, file.FileName);

        // Kiểm tra xem tệp đã tồn tại chưa
        if (File.Exists(filePath))
            throw new IOException($"File '{file.FileName}' đã tồn tại.");

        const string vtApiUrl = "https://www.virustotal.com/api/v3/files";
        const string vtApiKey = "15ea616156e895bce63de9ab04304951848fa73b3b653dd84c3f48e9e9fb9c18";

       

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-apikey", vtApiKey);

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(memoryStream), "file", file.FileName);

                var response = await httpClient.PostAsync(vtApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseBody);
                var analysisId = jsonResponse.RootElement.GetProperty("data").GetProperty("id").GetString();

                var analysisResponse = await httpClient.GetAsync($"https://www.virustotal.com/api/v3/analyses/{analysisId}");
                var analysisBody = await analysisResponse.Content.ReadAsStringAsync();
                var analysisJson = JsonDocument.Parse(analysisBody);

                var dataElement = analysisJson.RootElement.GetProperty("data");

                if (dataElement.TryGetProperty("attributes", out var outerAttributes))
                {
                    if (outerAttributes.TryGetProperty("results", out var results))
                    {
                        bool allUndetected = true;
                        bool hasResults = results.ValueKind == JsonValueKind.Object;

                        if (hasResults)
                        {
                            if (results.EnumerateObject().Count() == 0)
                            {
                                throw new IOException("Hiện tại server quét virus đang quá tải, vui lòng thử lại sau ít phút !");
                            }

                            foreach (var engine in results.EnumerateObject())
                            {
                                var category = engine.Value.GetProperty("category").GetString();

                                if (category == "undetected" || category == "type-unsupported" || category == "harmless" || category == "timeout" || category == "failure" || category == "confirmed-timeout")
                                {
                                    continue;
                                }
                                else
                                {
                                    allUndetected = false;
                                    throw new IOException($"File '{file.FileName}' chứa virus, không thể tải lên!");
                                }
                            }
                        }
                        else
                        {
                            throw new IOException($"File '{file.FileName}' chứa virus, không thể tải lên!");
                        }


                        if (allUndetected)
                        {
                            if (File.Exists(filePath))
                            {
                                throw new IOException($"File '{file.FileName}' đã tồn tại.");
                            }
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                memoryStream.Position = 0;
                                await memoryStream.CopyToAsync(fileStream);
                            }

                            var fileType = FileTypeHelper.GetFileType(fileExtension);
                            var fileCategory = FileTypeHelper.GetFileCategory(fileExtension);
                            var fileModel = new FileModel
                            {
                                FileName = file.FileName,
                                FilePath = filePath,
                                UploadedAt = DateTime.Now,
                                FileSize = file.Length,
                                FileType = fileType,
                                UserId = userId, // Gán ID người dùng
                                FileCategory = fileCategory // Set FileCategory here
                            };

                            await _context.Files.AddAsync(fileModel);
                            await _context.SaveChangesAsync();

                            return fileModel;

                        }
                        else
                        {

                            throw new IOException($"File '{file.FileName}' chứa virus, không thể tải lên!");
                        }
                    }
                    else
                    {
                        throw new Exception("results not found in the analysis response.");
                    }
                }
                else
                {
                    throw new Exception("attributes not found in the analysis response.");
                }
            }
        }
    }


    //   var fileType = FileTypeHelper.GetFileType(fileExtension);
    //   var fileCategory = FileTypeHelper.GetFileCategory(fileExtension);
    //   var fileModel = new FileModel
    //    {
    //       FileName = file.FileName,
    //       FilePath = filePath,
    //       UploadedAt = DateTime.Now,
    //       FileSize = file.Length,
    //       FileType = fileType,
    //      UserId = userId, // Gán ID người dùng
    //      FileCategory = fileCategory // Set FileCategory here
    //  };

    // Lưu vào database
    //  await _context.Files.AddAsync(fileModel);
    //   await _context.SaveChangesAsync();

    //    return fileModel;
    //  }

    public async Task<FileModel> GetFileByIdAsync(int id)
    {
        return await _context.Files.FindAsync(id); // Lấy tệp theo ID từ database
    }

    public async Task<bool> DeleteFileAsync(int fileId)
    {
        var file = await GetFileByIdAsync(fileId);
        if (file == null || !File.Exists(file.FilePath))
        {
            return false;
        }

        File.Delete(file.FilePath);
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Stream> DownloadFileAsync(int fileId)
    {
        var file = await GetFileByIdAsync(fileId);
        if (file == null)
        {
            throw new FileNotFoundException("File not found.");
        }

        return new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<List<FileModel>> GetFilesByUserIdAsync(string userId)
    {
        return await _context.Files
            .Where(file => file.UserId == userId)
            .ToListAsync(); // Lấy tất cả các tệp của người dùng cụ thể
    }


}
