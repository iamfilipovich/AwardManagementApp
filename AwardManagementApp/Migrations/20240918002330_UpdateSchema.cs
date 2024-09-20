using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AwardManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Awards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Awards_UserId",
                table: "Awards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards");

            migrationBuilder.DropIndex(
                name: "IX_Awards_UserId",
                table: "Awards");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Awards");
        }
    }
}
