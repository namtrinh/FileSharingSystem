namespace FileSharingSystem.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
    }

    public static class FileSizeFormatter
    {
        public static string FormatFileSize(long sizeInBytes)
        {
            if (sizeInBytes >= 1073741824) // > 1 GB
                return $"{(sizeInBytes / 1073741824.0):F2} GB";
            else if (sizeInBytes >= 1048576) // > 1 MB
                return $"{(sizeInBytes / 1048576.0):F2} MB";
            else if (sizeInBytes >= 1024) // > 1 KB
                return $"{(sizeInBytes / 1024.0):F2} KB";
            else
                return $"{sizeInBytes} bytes"; // Dưới 1 KB
        }
    }

    public static class FileTypeHelper // Tạo lớp tĩnh cho phương thức GetFileType
    {
        public static string GetFileType(string fileExtension)
        {
            return fileExtension switch
            {
                ".doc" or ".docx" => "Word Document",
                ".xls" or ".xlsx" => "Excel Spreadsheet",
                ".ppt" or ".pptx" => "PowerPoint Presentation",
                ".jpg" or ".jpeg" => "JPEG Image",
                ".png" => "PNG Image",
                ".gif" => "GIF Image",
                ".pdf" => "PDF Document",
                ".txt" => "Text File",
                ".zip" => "ZIP Archive",
                ".rar" => "RAR Archive",
                ".csv" => "CSV File",
                _ => "Unknown File Type" // Trường hợp loại tệp không xác định
            };
        }
    }

}
