using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AwardManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class SecondUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Awards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Awards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Users_UserId",
                table: "Awards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
