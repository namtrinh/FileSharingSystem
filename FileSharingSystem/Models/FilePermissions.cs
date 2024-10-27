// File: Models/FilePermission.cs

namespace FileSharingSystem.Models 
{
    public class FilePermission
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        public FileRecord FileRecord { get; set; } // Liên kết tới FileRecord
        public User User { get; set; } // Liên kết tới User
    }
}

