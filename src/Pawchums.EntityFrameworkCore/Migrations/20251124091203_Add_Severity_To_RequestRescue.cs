using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawchums.Migrations
{
    /// <inheritdoc />
    public partial class Add_Severity_To_RequestRescue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "AppRequestRescues",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestRescues_Severity",
                table: "AppRequestRescues",
                column: "Severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRequestRescues_Severity",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "AppRequestRescues");
        }
    }
}
