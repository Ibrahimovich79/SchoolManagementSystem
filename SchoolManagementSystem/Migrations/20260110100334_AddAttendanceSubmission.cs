using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    TeacherId = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceSubmissions_GradeTable_ClassId",
                        column: x => x.ClassId,
                        principalTable: "GradeTable",
                        principalColumn: "GradeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceSubmissions_TeacherTb_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "TeacherTb",
                        principalColumn: "TeachID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSubmissions_ClassId",
                table: "AttendanceSubmissions",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSubmissions_TeacherId",
                table: "AttendanceSubmissions",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceSubmissions");
        }
    }
}
