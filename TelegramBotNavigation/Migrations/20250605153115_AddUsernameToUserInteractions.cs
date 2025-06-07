using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBotNavigation.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToUserInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserInteractions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserInteractions");
        }
    }
}
