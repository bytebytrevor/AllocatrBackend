using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCompletionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Projects"
                SET "Status" = LOWER(TRIM("Status"));

                UPDATE "Projects"
                SET "Status" = 'pending'
                WHERE "Status" = 'draft';
                """
            );
            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionRequestedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletionRequestedByAllocatId",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CompletionRequestedByAllocatId",
                table: "Projects",
                column: "CompletionRequestedByAllocatId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects",
                sql: "\"Status\" IN ('pending', 'active', 'completion_requested', 'completed')");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AllocatProfiles_CompletionRequestedByAllocatId",
                table: "Projects",
                column: "CompletionRequestedByAllocatId",
                principalTable: "AllocatProfiles",
                principalColumn: "AllocatrUserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AllocatProfiles_CompletionRequestedByAllocatId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CompletionRequestedByAllocatId",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedByAllocatId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "pending");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects",
                sql: "\"Status\" IN ('pending', 'active', 'completed')");
        }
    }
}
