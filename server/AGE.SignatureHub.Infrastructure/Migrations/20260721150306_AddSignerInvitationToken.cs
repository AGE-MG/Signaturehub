using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGE.SignatureHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignerInvitationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationToken",
                table: "Signers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitationToken",
                table: "Signers");
        }
    }
}
