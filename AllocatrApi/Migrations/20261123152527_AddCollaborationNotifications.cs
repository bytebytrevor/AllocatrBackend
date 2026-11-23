using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaborationNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActorId",
                table: "UserNotifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemCount",
                table: "UserNotifications",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "UserNotifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "UserNotifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToCommentId",
                table: "TaskComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_ReplyToCommentId",
                table: "TaskComments",
                column: "ReplyToCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_TaskComments_ReplyToCommentId",
                table: "TaskComments",
                column: "ReplyToCommentId",
                principalTable: "TaskComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_TaskComments_ReplyToCommentId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_ReplyToCommentId",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "ActorId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "ItemCount",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "ReplyToCommentId",
                table: "TaskComments");
        }
    }
}
