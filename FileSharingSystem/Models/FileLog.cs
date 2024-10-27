// File: Models/FileLog.cs

namespace FileSharingSystem.Models
{
    public class FileLog
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } // "upload", "download", "delete"
        public DateTime Timestamp { get; set; }

        public FileRecord FileRecord { get; set; } // Liên kết tới FileRecord
        public User User { get; set; } // Liên kết tới User
    }

}
