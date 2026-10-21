using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationMessagePins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PinVersion",
                table: "ProjectConversations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinnedAt",
                table: "ConversationMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PinnedById",
                table: "ConversationMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ConversationId_PinnedAt",
                table: "ConversationMessages",
                columns: new[] { "ConversationId", "PinnedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_PinnedById",
                table: "ConversationMessages",
                column: "PinnedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMessages_AspNetUsers_PinnedById",
                table: "ConversationMessages",
                column: "PinnedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMessages_AspNetUsers_PinnedById",
                table: "ConversationMessages");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMessages_ConversationId_PinnedAt",
                table: "ConversationMessages");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMessages_PinnedById",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "PinVersion",
                table: "ProjectConversations");

            migrationBuilder.DropColumn(
                name: "PinnedAt",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "PinnedById",
                table: "ConversationMessages");
        }
    }
}
