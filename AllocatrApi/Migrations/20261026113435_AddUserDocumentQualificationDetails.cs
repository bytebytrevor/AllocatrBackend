using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocumentQualificationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IssuingInstitution",
                table: "UserDocuments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserDocuments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuingInstitution",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserDocuments");
        }
    }
}
