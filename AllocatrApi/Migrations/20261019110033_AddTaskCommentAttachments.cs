using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCommentAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientRequestId",
                table: "TaskComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "ProjectFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_CreatedById_ClientRequestId",
                table: "TaskComments",
                columns: new[] { "CreatedById", "ClientRequestId" },
                unique: true,
                filter: "\"ClientRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_CommentId",
                table: "ProjectFiles",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectFiles_TaskComments_CommentId",
                table: "ProjectFiles",
                column: "CommentId",
                principalTable: "TaskComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectFiles_TaskComments_CommentId",
                table: "ProjectFiles");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_CreatedById_ClientRequestId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectFiles_CommentId",
                table: "ProjectFiles");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "ProjectFiles");
        }
    }
}
