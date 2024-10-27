using Microsoft.AspNetCore.Identity;

namespace FileSharingSystem.Models
{
    public class FileRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; } // Đây là thuộc tính bạn cần lấy
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string UserId { get; set; } // Giả định bạn có thuộc tính này
    }
}
