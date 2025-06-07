using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBotNavigation.Migrations
{
    /// <inheritdoc />
    public partial class AddSubMenuToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubMenuId",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_SubMenuId",
                table: "MenuItems",
                column: "SubMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Menus_SubMenuId",
                table: "MenuItems",
                column: "SubMenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Menus_SubMenuId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_SubMenuId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "SubMenuId",
                table: "MenuItems");
        }
    }
}
