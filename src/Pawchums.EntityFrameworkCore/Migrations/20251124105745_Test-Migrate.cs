using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawchums.Migrations
{
    /// <inheritdoc />
    public partial class TestMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppRescuerApplications",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppRescuerApplications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppRescuerApplications");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppRescuerApplications");
        }
    }
}
