using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zezame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Organization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isplatformadmin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "organizationid",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "organizationid",
                table: "time_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "organizationid",
                table: "projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "organizationid",
                table: "activities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_organizationid",
                table: "users",
                column: "organizationid");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_organizationid",
                table: "time_entries",
                column: "organizationid");

            migrationBuilder.CreateIndex(
                name: "IX_projects_organizationid",
                table: "projects",
                column: "organizationid");

            migrationBuilder.CreateIndex(
                name: "IX_activities_organizationid",
                table: "activities",
                column: "organizationid");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_organizations_organizationid",
                table: "activities",
                column: "organizationid",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_organizations_organizationid",
                table: "projects",
                column: "organizationid",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_time_entries_organizations_organizationid",
                table: "time_entries",
                column: "organizationid",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_organizationid",
                table: "users",
                column: "organizationid",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_organizations_organizationid",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_organizations_organizationid",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_time_entries_organizations_organizationid",
                table: "time_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_organizationid",
                table: "users");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_users_organizationid",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_time_entries_organizationid",
                table: "time_entries");

            migrationBuilder.DropIndex(
                name: "IX_projects_organizationid",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_activities_organizationid",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "isplatformadmin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "organizationid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "organizationid",
                table: "time_entries");

            migrationBuilder.DropColumn(
                name: "organizationid",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "organizationid",
                table: "activities");
        }
    }
}
