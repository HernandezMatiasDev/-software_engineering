using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuMejorPeso.Migrations
{
    /// <inheritdoc />
    public partial class updatePay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pay_memberId",
                table: "Pay",
                column: "memberId");

            migrationBuilder.CreateIndex(
                name: "IX_Pay_membershipId",
                table: "Pay",
                column: "membershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pay_Members_memberId",
                table: "Pay",
                column: "memberId",
                principalTable: "Members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pay_Memberships_membershipId",
                table: "Pay",
                column: "membershipId",
                principalTable: "Memberships",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pay_Members_memberId",
                table: "Pay");

            migrationBuilder.DropForeignKey(
                name: "FK_Pay_Memberships_membershipId",
                table: "Pay");

            migrationBuilder.DropIndex(
                name: "IX_Pay_memberId",
                table: "Pay");

            migrationBuilder.DropIndex(
                name: "IX_Pay_membershipId",
                table: "Pay");
        }
    }
}
