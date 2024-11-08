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
        public string UserId { get; set; }
        public string FileCategory { get; set; }
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
                // Documents
                ".txt" => "Text Document",
                ".pdf" => "PDF Document",
                ".doc" or ".docx" => "Word Document",
                ".xls" or ".xlsx" => "Excel Spreadsheet",
                ".ppt" or ".pptx" => "PowerPoint Presentation",

                // Images
                ".jpg" or ".jpeg" => "JPEG Image",
                ".png" => "PNG Image",
                ".gif" => "GIF Image",
                ".bmp" => "Bitmap Image",
                ".svg" => "Scalable Vector Graphics",

                // Audio
                ".mp3" => "MP3 Audio",
                ".wav" => "WAV Audio",
                ".aac" => "AAC Audio",
                ".flac" => "FLAC Audio",

                // Video
                ".mp4" => "MP4 Video",
                ".avi" => "AVI Video",
                ".mov" => "QuickTime Movie",
                ".mkv" => "Matroska Video",

                // Archives
                ".zip" => "ZIP Archive",
                ".rar" => "RAR Archive",
                ".7z" => "7-Zip Archive",
                ".tar" => "TAR Archive",
                ".gz" => "GZIP Archive",

                _ => "Unknown File Type" // Trường hợp loại tệp không xác định
            };
        }
        public static string GetFileCategory(string extension)
        {
            return extension switch
            {
                // Image formats
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".tiff" or ".ico" => "Hình ảnh",

                // Document formats
                ".doc" or ".docx" or ".pdf" or ".txt" or ".rtf" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".odt" or ".ods" or ".odp" => "Tài liệu",

                // Audio formats
                ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".wma" or ".m4a" => "Âm thanh",

                // Video formats
                ".mp4" or ".avi" or ".mov" or ".mkv" or ".flv" or ".wmv" or ".mpeg" or ".3gp" => "Video",

                // Compressed/archive formats
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" or ".xz" => "Lưu trữ",

                // Font files
                ".ttf" or ".otf" or ".woff" or ".woff2" => "Phông chữ",

                // Other common files
                ".csv" => "Bảng tính CSV",
                ".ics" => "Tệp lịch",
                ".apk" => "Ứng dụng Android",
                ".iso" => "Tệp ảnh đĩa",

                // Default case
                _ => "Khác"
            };
        }

    }
}
