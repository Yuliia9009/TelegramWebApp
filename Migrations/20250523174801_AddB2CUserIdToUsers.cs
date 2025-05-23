using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddB2CUserIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "B2CUserId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "B2CUserId",
                table: "Users");
        }
    }
}
