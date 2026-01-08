using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentAttendanceUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAttendance_StudentId_ClassId_AttendanceDate",
                table: "StudentAttendance");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendance_StudentId_AttendanceDate",
                table: "StudentAttendance",
                columns: new[] { "StudentId", "AttendanceDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAttendance_StudentId_AttendanceDate",
                table: "StudentAttendance");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendance_StudentId_ClassId_AttendanceDate",
                table: "StudentAttendance",
                columns: new[] { "StudentId", "ClassId", "AttendanceDate" },
                unique: true);
        }
    }
}
