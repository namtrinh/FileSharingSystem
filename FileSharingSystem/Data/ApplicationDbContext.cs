using FileSharingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileSharingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FileModel> Files { get; set; }
        public DbSet<FileRecord> FileRecords { get; set; }
        public DbSet<FileLog> FileLog { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Thay đổi kiểu dữ liệu cho các trường không tương thích
            builder.Entity<IdentityRole>()
                .Property(r => r.ConcurrencyStamp)
                .HasColumnType("text"); // Thay đổi từ nvarchar(max) thành text


            builder.Entity<IdentityRole>()
                .Property(r => r.Name)
                .HasColumnType("varchar(256)"); // Sử dụng varchar thay cho nvarchar

            builder.Entity<IdentityRole>()
                .Property(r => r.NormalizedName)
                .HasColumnType("varchar(256)"); // Sử dụng varchar thay cho nvarchar
        }


    }
}
