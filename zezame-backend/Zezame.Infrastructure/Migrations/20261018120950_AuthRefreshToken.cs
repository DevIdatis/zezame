using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zezame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuthRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isactive",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "isactive",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "refreshtokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    tokenhash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revokedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refreshtokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refreshtokens_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refreshtokens_tokenhash",
                table: "refreshtokens",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refreshtokens_userid",
                table: "refreshtokens",
                column: "userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "isactive",
                table: "users");

            migrationBuilder.DropColumn(
                name: "isactive",
                table: "organizations");
        }
    }
}
