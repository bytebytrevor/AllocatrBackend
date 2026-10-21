using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationMessageActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationAttachments_StoredFileId",
                table: "ConversationAttachments");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ConversationMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "ConversationMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "ConversationMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForwardNote",
                table: "ConversationMessages",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForwardedCreatedAt",
                table: "ConversationMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForwardedFromName",
                table: "ConversationMessages",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "ConversationMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationAttachments_StoredFileId",
                table: "ConversationAttachments",
                column: "StoredFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationAttachments_StoredFileId",
                table: "ConversationAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "ForwardNote",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "ForwardedCreatedAt",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "ForwardedFromName",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ConversationMessages");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationAttachments_StoredFileId",
                table: "ConversationAttachments",
                column: "StoredFileId",
                unique: true);
        }
    }
}
