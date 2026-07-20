using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGE.SignatureHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExternalServiceConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalServiceConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Secret = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventsCsv = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastDeliveryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDeliverySucceeded = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalServiceConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalServiceConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceConnections_UserId",
                table: "ExternalServiceConnections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceConnections_UserId_Name",
                table: "ExternalServiceConnections",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalServiceConnections");
        }
    }
}
