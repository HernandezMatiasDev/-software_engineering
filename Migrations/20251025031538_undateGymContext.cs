using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuMejorPeso.Migrations
{
    /// <inheritdoc />
    public partial class undateGymContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomCoaches_Classrooms_classroomsid",
                table: "ClassroomCoaches");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomCoaches_Coaches_coachesid",
                table: "ClassroomCoaches");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Assignment_assignmentid",
                table: "Classrooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassroomCoaches",
                table: "ClassroomCoaches");

            migrationBuilder.RenameTable(
                name: "ClassroomCoaches",
                newName: "ClassroomCoach");

            migrationBuilder.RenameColumn(
                name: "assignmentid",
                table: "Classrooms",
                newName: "assignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Classrooms_assignmentid",
                table: "Classrooms",
                newName: "IX_Classrooms_assignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomCoaches_coachesid",
                table: "ClassroomCoach",
                newName: "IX_ClassroomCoach_coachesid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassroomCoach",
                table: "ClassroomCoach",
                columns: new[] { "classroomsid", "coachesid" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomCoach_Classrooms_classroomsid",
                table: "ClassroomCoach",
                column: "classroomsid",
                principalTable: "Classrooms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomCoach_Coaches_coachesid",
                table: "ClassroomCoach",
                column: "coachesid",
                principalTable: "Coaches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Assignment_assignmentId",
                table: "Classrooms",
                column: "assignmentId",
                principalTable: "Assignment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomCoach_Classrooms_classroomsid",
                table: "ClassroomCoach");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomCoach_Coaches_coachesid",
                table: "ClassroomCoach");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Assignment_assignmentId",
                table: "Classrooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassroomCoach",
                table: "ClassroomCoach");

            migrationBuilder.RenameTable(
                name: "ClassroomCoach",
                newName: "ClassroomCoaches");

            migrationBuilder.RenameColumn(
                name: "assignmentId",
                table: "Classrooms",
                newName: "assignmentid");

            migrationBuilder.RenameIndex(
                name: "IX_Classrooms_assignmentId",
                table: "Classrooms",
                newName: "IX_Classrooms_assignmentid");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomCoach_coachesid",
                table: "ClassroomCoaches",
                newName: "IX_ClassroomCoaches_coachesid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassroomCoaches",
                table: "ClassroomCoaches",
                columns: new[] { "classroomsid", "coachesid" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomCoaches_Classrooms_classroomsid",
                table: "ClassroomCoaches",
                column: "classroomsid",
                principalTable: "Classrooms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomCoaches_Coaches_coachesid",
                table: "ClassroomCoaches",
                column: "coachesid",
                principalTable: "Coaches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Assignment_assignmentid",
                table: "Classrooms",
                column: "assignmentid",
                principalTable: "Assignment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
