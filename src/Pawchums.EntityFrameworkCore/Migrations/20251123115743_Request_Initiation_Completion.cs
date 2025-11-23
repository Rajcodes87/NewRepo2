using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawchums.Migrations
{
    /// <inheritdoc />
    public partial class Request_Initiation_Completion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "CompletionDescription",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "CompletionProofPicture",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "RescuerId",
                table: "AppRequestRescues");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AppRequestRescues",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "AppRequestRescues",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppRescueCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestRescueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionProofPicture = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedByRescuerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppRescueCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRescueCompletions_AbpUsers_CompletedByRescuerId",
                        column: x => x.CompletedByRescuerId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppRescueCompletions_AppRequestRescues_RequestRescueId",
                        column: x => x.RequestRescueId,
                        principalTable: "AppRequestRescues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRescueInitiations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestRescueId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescuerId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsSelected = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppRescueInitiations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRescueInitiations_AbpUsers_RescuerId",
                        column: x => x.RescuerId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppRescueInitiations_AppRequestRescues_RequestRescueId",
                        column: x => x.RequestRescueId,
                        principalTable: "AppRequestRescues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRescuerNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestRescueId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescuerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRescuerNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRescuerNotifications_AbpUsers_RescuerId",
                        column: x => x.RescuerId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRescuerNotifications_AppRequestRescues_RequestRescueId",
                        column: x => x.RequestRescueId,
                        principalTable: "AppRequestRescues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestRescues_IsActive",
                table: "AppRequestRescues",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestRescues_RequestDate",
                table: "AppRequestRescues",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestRescues_Status",
                table: "AppRequestRescues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueCompletions_CompletedByRescuerId",
                table: "AppRescueCompletions",
                column: "CompletedByRescuerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueCompletions_IsVerified",
                table: "AppRescueCompletions",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueCompletions_RequestRescueId",
                table: "AppRescueCompletions",
                column: "RequestRescueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueInitiations_IsSelected",
                table: "AppRescueInitiations",
                column: "IsSelected");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueInitiations_RequestRescueId",
                table: "AppRescueInitiations",
                column: "RequestRescueId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueInitiations_RequestRescueId_RescuerId",
                table: "AppRescueInitiations",
                columns: new[] { "RequestRescueId", "RescuerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueInitiations_RescuerId",
                table: "AppRescueInitiations",
                column: "RescuerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescueInitiations_Status",
                table: "AppRescueInitiations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescuerNotifications_IsRead",
                table: "AppRescuerNotifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescuerNotifications_RequestRescueId",
                table: "AppRescuerNotifications",
                column: "RequestRescueId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescuerNotifications_RescuerId",
                table: "AppRescuerNotifications",
                column: "RescuerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRescuerNotifications_RescuerId_IsRead",
                table: "AppRescuerNotifications",
                columns: new[] { "RescuerId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRescueCompletions");

            migrationBuilder.DropTable(
                name: "AppRescueInitiations");

            migrationBuilder.DropTable(
                name: "AppRescuerNotifications");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestRescues_IsActive",
                table: "AppRequestRescues");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestRescues_RequestDate",
                table: "AppRequestRescues");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestRescues_Status",
                table: "AppRequestRescues");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "AppRequestRescues");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AppRequestRescues",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "AppRequestRescues",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionDescription",
                table: "AppRequestRescues",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionProofPicture",
                table: "AppRequestRescues",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RescuerId",
                table: "AppRequestRescues",
                type: "uuid",
                nullable: true);
        }
    }
}
