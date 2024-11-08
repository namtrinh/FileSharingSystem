using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSharingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFileCategoryToFileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileCategory",
                table: "Files",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileCategory",
                table: "Files");
        }
    }
}
