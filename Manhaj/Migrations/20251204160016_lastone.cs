using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manhaj.Migrations
{
    /// <inheritdoc />
    public partial class lastone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Ratings_Courses_CourseID",
                table: "Course_Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Ratings_Ratings_RatingID",
                table: "Course_Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Registrations_Courses_CourseID",
                table: "Course_Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Registrations_Users_StudentID",
                table: "Course_Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_TeacherID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_CourseID",
                table: "Lectures");

            migrationBuilder.RenameColumn(
                name: "ParentPhone",
                table: "Users",
                newName: "ParentPhoneNumber");

            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "Lectures",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Lectures_CourseID",
                table: "Lectures",
                newName: "IX_Lectures_CourseId");

            migrationBuilder.RenameColumn(
                name: "level",
                table: "Courses",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "TeacherID",
                table: "Courses",
                newName: "TeacherId");

            migrationBuilder.RenameColumn(
                name: "Num_Of_Lectures",
                table: "Courses",
                newName: "NumberOfLectures");

            migrationBuilder.RenameColumn(
                name: "Creation_Date",
                table: "Courses",
                newName: "CreationDate");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_TeacherID",
                table: "Courses",
                newName: "IX_Courses_TeacherId");

            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "Course_Registrations",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "StudentID",
                table: "Course_Registrations",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "Registration_Date",
                table: "Course_Registrations",
                newName: "RegistrationDate");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Registrations_CourseID",
                table: "Course_Registrations",
                newName: "IX_Course_Registrations_CourseId");

            migrationBuilder.RenameColumn(
                name: "RatingID",
                table: "Course_Ratings",
                newName: "RatingId");

            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "Course_Ratings",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Ratings_RatingID",
                table: "Course_Ratings",
                newName: "IX_Course_Ratings_RatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Ratings_CourseID",
                table: "Course_Ratings",
                newName: "IX_Course_Ratings_CourseId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Course_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Course_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalGrade",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Ratings_Courses_CourseId",
                table: "Course_Ratings",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Ratings_Ratings_RatingId",
                table: "Course_Ratings",
                column: "RatingId",
                principalTable: "Ratings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Registrations_Courses_CourseId",
                table: "Course_Registrations",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Registrations_Users_StudentId",
                table: "Course_Registrations",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_TeacherId",
                table: "Courses",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_CourseId",
                table: "Lectures",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Ratings_Courses_CourseId",
                table: "Course_Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Ratings_Ratings_RatingId",
                table: "Course_Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Registrations_Courses_CourseId",
                table: "Course_Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Registrations_Users_StudentId",
                table: "Course_Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_TeacherId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_CourseId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Course_Registrations");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Course_Registrations");

            migrationBuilder.DropColumn(
                name: "TotalGrade",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "ParentPhoneNumber",
                table: "Users",
                newName: "ParentPhone");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Lectures",
                newName: "CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Lectures_CourseId",
                table: "Lectures",
                newName: "IX_Lectures_CourseID");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Courses",
                newName: "TeacherID");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Courses",
                newName: "level");

            migrationBuilder.RenameColumn(
                name: "NumberOfLectures",
                table: "Courses",
                newName: "Num_Of_Lectures");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "Courses",
                newName: "Creation_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses",
                newName: "IX_Courses_TeacherID");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Course_Registrations",
                newName: "CourseID");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Course_Registrations",
                newName: "StudentID");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Course_Registrations",
                newName: "Registration_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Registrations_CourseId",
                table: "Course_Registrations",
                newName: "IX_Course_Registrations_CourseID");

            migrationBuilder.RenameColumn(
                name: "RatingId",
                table: "Course_Ratings",
                newName: "RatingID");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Course_Ratings",
                newName: "CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Ratings_RatingId",
                table: "Course_Ratings",
                newName: "IX_Course_Ratings_RatingID");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Ratings_CourseId",
                table: "Course_Ratings",
                newName: "IX_Course_Ratings_CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Ratings_Courses_CourseID",
                table: "Course_Ratings",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Ratings_Ratings_RatingID",
                table: "Course_Ratings",
                column: "RatingID",
                principalTable: "Ratings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Registrations_Courses_CourseID",
                table: "Course_Registrations",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Registrations_Users_StudentID",
                table: "Course_Registrations",
                column: "StudentID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_TeacherID",
                table: "Courses",
                column: "TeacherID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_CourseID",
                table: "Lectures",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
