using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBotNavigation.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTranslationKeyToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "MessageTranslationKey",
                table: "MenuItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "MenuItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageTranslationKey",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
