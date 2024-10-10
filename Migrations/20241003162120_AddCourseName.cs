using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Geology_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "LectureNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "LectureNotes");
        }
    }
}
