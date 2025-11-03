using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuMejorPeso.Migrations
{
    /// <inheritdoc />
    public partial class attendence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Members_Memberid",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "Memberid",
                table: "Attendance",
                newName: "memberId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_Memberid",
                table: "Attendance",
                newName: "IX_Attendance_memberId");

            migrationBuilder.AlterColumn<int>(
                name: "memberId",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "classroomid",
                table: "Attendance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_classroomid",
                table: "Attendance",
                column: "classroomid");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Classrooms_classroomid",
                table: "Attendance",
                column: "classroomid",
                principalTable: "Classrooms",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Members_memberId",
                table: "Attendance",
                column: "memberId",
                principalTable: "Members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Classrooms_classroomid",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Members_memberId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_classroomid",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "classroomid",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "memberId",
                table: "Attendance",
                newName: "Memberid");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_memberId",
                table: "Attendance",
                newName: "IX_Attendance_Memberid");

            migrationBuilder.AlterColumn<int>(
                name: "Memberid",
                table: "Attendance",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Members_Memberid",
                table: "Attendance",
                column: "Memberid",
                principalTable: "Members",
                principalColumn: "id");
        }
    }
}
