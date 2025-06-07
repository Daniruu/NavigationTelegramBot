using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBotNavigation.Migrations
{
    /// <inheritdoc />
    public partial class AddParentMenuIdToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentMenuId",
                table: "Menus",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentMenuId",
                table: "Menus",
                column: "ParentMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Menus_ParentMenuId",
                table: "Menus",
                column: "ParentMenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Menus_ParentMenuId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_ParentMenuId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ParentMenuId",
                table: "Menus");
        }
    }
}
