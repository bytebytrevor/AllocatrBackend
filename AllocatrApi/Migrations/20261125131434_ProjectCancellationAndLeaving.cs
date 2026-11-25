using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class ProjectCancellationAndLeaving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Projects_Status", table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Projects",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Projects",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "LeaveReason",
                table: "ProjectAllocats",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "ProjectAllocats",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "ProjectLifecycleEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    Reason = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ProjectTitle = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    ProjectCode = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    ActorName = table.Column<string>(type: "text", nullable: false),
                    RecipientIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLifecycleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectLifecycleEvents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects",
                sql: "\"Status\" IN ('pending', 'active', 'completion_requested', 'completed', 'cancelled')"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLifecycleEvents_ProjectId_CreatedAt",
                table: "ProjectLifecycleEvents",
                columns: new[] { "ProjectId", "CreatedAt" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProjectLifecycleEvents");

            migrationBuilder.DropCheckConstraint(name: "CK_Projects_Status", table: "Projects");

            migrationBuilder.DropColumn(name: "CancellationReason", table: "Projects");

            migrationBuilder.DropColumn(name: "CancelledAt", table: "Projects");

            migrationBuilder.DropColumn(name: "CancelledByUserId", table: "Projects");

            migrationBuilder.DropColumn(name: "LeaveReason", table: "ProjectAllocats");

            migrationBuilder.DropColumn(name: "LeftAt", table: "ProjectAllocats");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                table: "Projects",
                sql: "\"Status\" IN ('pending', 'active', 'completion_requested', 'completed')"
            );
        }
    }
}
