using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FileSharingSystem.Models;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<List<FileModel>> GetAllFilesAsync();
    Task<FileModel> UploadFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(int fileId);
    Task<Stream> DownloadFileAsync(int fileId);
}

public class FileService : IFileService
{
    private readonly string _fileStoragePath;

    // Constructor để khởi tạo đường dẫn lưu trữ tệp
    public FileService(string fileStoragePath)
    {
        _fileStoragePath = fileStoragePath;

        // Tạo thư mục nếu không tồn tại
        if (!Directory.Exists(_fileStoragePath))
        {
            Directory.CreateDirectory(_fileStoragePath);
        }
    }

    // Lấy danh sách tất cả các tệp
    public async Task<List<FileModel>> GetAllFilesAsync()
    {
        // Lấy tất cả tệp trong thư mục lưu trữ và chuyển đổi thành danh sách FileModel với Id duy nhất
        var files = Directory.GetFiles(_fileStoragePath)
            .Select((filePath, index) => new FileModel
            {
                Id = index, // Gán Id là chỉ mục của tệp trong danh sách, duy nhất cho mỗi lần tải lại
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                UploadedAt = System.IO.File.GetCreationTime(filePath),
                FileSize = new FileInfo(filePath).Length // Lấy kích thước tệp
            })
            .ToList();

        return await Task.FromResult(files);
    }

    // Tải lên tệp
    public async Task<FileModel> UploadFileAsync(IFormFile file)
    {
        const string vtApiUrl = "https://www.virustotal.com/api/v3/files";
        const string vtApiKey = "15ea616156e895bce63de9ab04304951848fa73b3b653dd84c3f48e9e9fb9c18";

        var filePath = Path.Combine(_fileStoragePath, file.FileName);

        if (File.Exists(filePath))
        {
            throw new IOException($"File '{file.FileName}' already exists.");
        }

        // Lưu tệp vào thư mục tạm
        using (var tempStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(tempStream);
        }

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("x-apikey", vtApiKey);

            // Gửi tệp lên VirusTotal
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Delete))
            {
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fileStream), "file", file.FileName);

                var response = await httpClient.PostAsync(vtApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseBody);
                var analysisId = jsonResponse.RootElement.GetProperty("data").GetProperty("id").GetString();

                // Kiểm tra kết quả phân tích
                var analysisResponse = await httpClient.GetAsync($"https://www.virustotal.com/api/v3/analyses/{analysisId}");
                analysisResponse.EnsureSuccessStatusCode();

                var analysisBody = await analysisResponse.Content.ReadAsStringAsync();
                var analysisJson = JsonDocument.Parse(analysisBody);
                var dataElement = analysisJson.RootElement.GetProperty("data");

                // Kiểm tra xem thuộc tính attributes có tồn tại không
                if (dataElement.TryGetProperty("attributes", out var attributes))
                {
                    // Kiểm tra xem thuộc tính results có tồn tại không
                    if (attributes.TryGetProperty("results", out var results))
                    {
                        bool allUndetected = true; // Biến để kiểm tra xem tất cả các công cụ quét đều là undetected
                        bool hasResults = results.ValueKind == JsonValueKind.Object; // Kiểm tra nếu results là một object

                        // Nếu results là một đối tượng, kiểm tra từng công cụ quét
                        if (hasResults)
                        {
                            foreach (var engine in results.EnumerateObject())
                            {
                                var category = engine.Value.GetProperty("category").GetString();                           
                                if (category == "malicious")
                                {
                                    File.Delete(filePath);
                                    throw new IOException($"File '{file.FileName}'có thể chứa virus, không thể tải lên ! ");
                                }
                                else if (category != "undetected")
                                {
                                    File.Delete(filePath);
                                    allUndetected = false; // Nếu có bất kỳ engine nào không phải là undetected, đánh dấu
                                    break; // Thoát khỏi vòng lặp
                                }
                                else if (category == null)
                                {
                                    File.Delete(filePath);
                                    throw new IOException($"File '{file.FileName}'có thể chứa virus, không thể tải lên ! ");
                                }
                                

                                
                            }
                        }
                        else
                        {
                            File.Delete(filePath);
                            // Không có kết quả quét
                            throw new IOException($"File '{file.FileName}'có thể chứa virus, không thể tải lên !");
                        }

                        // Nếu tất cả các kết quả đều là undetected, cho phép tải lên
                        if (allUndetected)
                        {
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            var fileType = FileTypeHelper.GetFileType(fileExtension);
                            return new FileModel
                            {
                                FileName = file.FileName,
                                FilePath = filePath,
                                UploadedAt = DateTime.Now,
                                FileSize = file.Length,
                                FileType = fileType
                            };
                        }
                        else
                        {
                            // Nếu có ít nhất một kết quả không phải undetected, xóa tệp và ném ngoại lệ
                            File.Delete(filePath);
                            throw new IOException($"File '{file.FileName}'có thể chứa virus, không thể tải lên !");
                        }
                    }
                    else
                    {
                        File.Delete(filePath);
                        throw new Exception("results not found in the analysis response.");
                    }
                }
                else
                {
                    File.Delete(filePath);
                    throw new Exception("attributes not found in the analysis response.");
                }
            }
        }
    }




    // Xóa tệp theo ID
    public async Task<bool> DeleteFileAsync(int fileId)
    {
        var files = await GetAllFilesAsync();
        var file = files.FirstOrDefault(f => f.Id == fileId);

        if (file == null || !File.Exists(file.FilePath))
        {
            return await Task.FromResult(false);
        }

        File.Delete(file.FilePath);
        return await Task.FromResult(true);
    }

    // Tải xuống tệp theo ID
    public async Task<Stream> DownloadFileAsync(int fileId)
    {
        var files = await GetAllFilesAsync();
        var file = files.FirstOrDefault(f => f.Id == fileId);

        if (file == null)
        {
            throw new FileNotFoundException("File not found.");
        }

        return new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
    }




}
