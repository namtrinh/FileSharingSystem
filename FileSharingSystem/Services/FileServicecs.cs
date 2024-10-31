﻿using System.Collections.Generic;
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

                // Gửi yêu cầu phân tích
                var analysisResponse = await httpClient.GetAsync($"https://www.virustotal.com/api/v3/analyses/{analysisId}");

                if (!analysisResponse.IsSuccessStatusCode)
                {
                    throw new IOException("Phân tích virus không thành công.");
                }

                var analysisBody = await analysisResponse.Content.ReadAsStringAsync();
                var analysisJson = JsonDocument.Parse(analysisBody);

                // Kiểm tra kết quả phân tích
                var dataElement = analysisJson.RootElement.GetProperty("data");

                if (dataElement.TryGetProperty("attributes", out var outerAttributes))
                {
                    if (outerAttributes.TryGetProperty("results", out var results))
                    {
                        bool allUndetected = true; // Giả định ban đầu là tất cả đều không phát hiện
                        bool hasResults = results.ValueKind == JsonValueKind.Object;

                        if (hasResults)
                        {
                            foreach (var engine in results.EnumerateObject())
                            {
                                var category = engine.Value.GetProperty("category").GetString();

                                // Nếu có bất kỳ engine nào phát hiện là "malicious" hoặc "suspicious"
                                if (category == "malicious" || category == "suspicious")
                                {
                                    throw new IOException($"File '{file.FileName}' có thể chứa virus, không thể tải lên!");
                                }

                                // Nếu có bất kỳ engine nào có kết quả "undetected", không thay đổi giá trị
                                if (category == "undetected" || category == "type-unsupported" || category == "harmless")
                                {
                                    // Chỉ cần giữ nguyên allUndetected = true
                                    continue;
                                }
                                else
                                {
                                    allUndetected = false; // Nếu không phát hiện và không phải là undetected, đánh dấu không an toàn
                                    throw new IOException($"File '{file.FileName}' có thể chứa virus, không thể tải lên!");
                                }
                            }
                        }
                        else
                        {
                            throw new IOException($"File '{file.FileName}' có thể chứa virus, không thể tải lên!");
                        }

                        // Nếu tất cả đều "undetected", cho phép tải lên
                        if (allUndetected)
                        {
                            var filePath = Path.Combine(_fileStoragePath, file.FileName);

                            if (File.Exists(filePath))
                            {
                                throw new IOException($"File '{file.FileName}' đã tồn tại.");
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                memoryStream.Position = 0;
                                await memoryStream.CopyToAsync(fileStream);
                            }

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
                            throw new IOException($"File '{file.FileName}' có thể chứa virus, không thể tải lên!");
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
