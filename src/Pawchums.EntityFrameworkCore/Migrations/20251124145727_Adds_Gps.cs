using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawchums.Migrations
{
    /// <inheritdoc />
    public partial class Adds_Gps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AppRequestRescues",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AppRequestRescues",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapUrl",
                table: "AppRequestRescues",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RescuerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLocation = table.Column<string>(type: "text", nullable: true),
                    LocationUpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsLocationShared = table.Column<bool>(type: "boolean", nullable: false),
                    MaxNotificationRadius = table.Column<int>(type: "integer", nullable: false),
                    ReceiveEmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    ReceiveSmsNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescuerProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestRescues_Latitude_Longitude",
                table: "AppRequestRescues",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RescuerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestRescues_Latitude_Longitude",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "MapUrl",
                table: "AppRequestRescues");
        }
    }
}
