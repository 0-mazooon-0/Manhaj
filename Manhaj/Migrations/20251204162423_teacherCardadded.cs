using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manhaj.Migrations
{
    /// <inheritdoc />
    public partial class teacherCardadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeacherFormalCard",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeacherFormalCard",
                table: "Users");
        }
    }
}
