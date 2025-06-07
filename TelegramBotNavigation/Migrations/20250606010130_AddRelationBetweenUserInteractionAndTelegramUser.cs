using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBotNavigation.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationBetweenUserInteractionAndTelegramUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserInteractions");

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "TelegramUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractions_TelegramUserId",
                table: "UserInteractions",
                column: "TelegramUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInteractions_TelegramUsers_TelegramUserId",
                table: "UserInteractions",
                column: "TelegramUserId",
                principalTable: "TelegramUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInteractions_TelegramUsers_TelegramUserId",
                table: "UserInteractions");

            migrationBuilder.DropIndex(
                name: "IX_UserInteractions_TelegramUserId",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "TelegramUsers");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserInteractions",
                type: "text",
                nullable: true);
        }
    }
}
