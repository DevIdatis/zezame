using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zezame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmailVerificationAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "emailconfirmed",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "usertokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    tokenhash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usertokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_usertokens_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usertokens_purpose_tokenhash",
                table: "usertokens",
                columns: new[] { "purpose", "tokenhash" });

            migrationBuilder.CreateIndex(
                name: "IX_usertokens_userid",
                table: "usertokens",
                column: "userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usertokens");

            migrationBuilder.DropColumn(
                name: "emailconfirmed",
                table: "users");
        }
    }
}
