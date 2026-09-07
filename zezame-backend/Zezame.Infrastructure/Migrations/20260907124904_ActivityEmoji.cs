using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zezame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActivityEmoji : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emoji",
                table: "activities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emoji",
                table: "activities");
        }
    }
}
