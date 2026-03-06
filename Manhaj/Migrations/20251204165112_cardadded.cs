using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manhaj.Migrations
{
    /// <inheritdoc />
    public partial class cardadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeacherFormalCard",
                table: "Users",
                newName: "TeacherFormalCardPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeacherFormalCardPath",
                table: "Users",
                newName: "TeacherFormalCard");
        }
    }
}
