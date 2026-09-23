using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* =====================================================
               CALENDAR FOCUS TASKS
            ===================================================== */

            migrationBuilder.CreateTable(
                name: "CalendarFocusTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false
                    ),

                    UserId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false
                    ),

                    TaskId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false
                    ),

                    WeekStart = table.Column<DateOnly>(
                        type: "date",
                        nullable: false
                    ),

                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_CalendarFocusTasks",
                        x => x.Id
                    );

                    table.ForeignKey(
                        name: "FK_CalendarFocusTasks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );

                    table.ForeignKey(
                        name: "FK_CalendarFocusTasks_TaskItems_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            /* =====================================================
               CALENDAR PLANNING BLOCKS
            ===================================================== */

            migrationBuilder.CreateTable(
                name: "CalendarPlanningBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false
                    ),

                    UserId = table.Column<Guid>(
                        type: "uuid",
                        nullable: false
                    ),

                    ProjectId = table.Column<Guid>(
                        type: "uuid",
                        nullable: true
                    ),

                    TaskId = table.Column<Guid>(
                        type: "uuid",
                        nullable: true
                    ),

                    Title = table.Column<string>(
                        type: "character varying(180)",
                        maxLength: 180,
                        nullable: false
                    ),

                    Notes = table.Column<string>(
                        type: "character varying(1200)",
                        maxLength: 1200,
                        nullable: true
                    ),

                    StartAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),

                    EndAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),

                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),

                    UpdatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_CalendarPlanningBlocks",
                        x => x.Id
                    );

                    table.ForeignKey(
                        name: "FK_CalendarPlanningBlocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );

                    table.ForeignKey(
                        name: "FK_CalendarPlanningBlocks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );

                    table.ForeignKey(
                        name: "FK_CalendarPlanningBlocks_TaskItems_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            /* =====================================================
               INDEXES
            ===================================================== */

            migrationBuilder.CreateIndex(
                name: "IX_CalendarFocusTasks_TaskId",
                table: "CalendarFocusTasks",
                column: "TaskId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CalendarFocusTasks_UserId_TaskId_WeekStart",
                table: "CalendarFocusTasks",
                columns: new[]
                {
                    "UserId",
                    "TaskId",
                    "WeekStart"
                },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_CalendarPlanningBlocks_ProjectId",
                table: "CalendarPlanningBlocks",
                column: "ProjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CalendarPlanningBlocks_TaskId",
                table: "CalendarPlanningBlocks",
                column: "TaskId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CalendarPlanningBlocks_UserId_StartAt",
                table: "CalendarPlanningBlocks",
                columns: new[]
                {
                    "UserId",
                    "StartAt"
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarFocusTasks"
            );

            migrationBuilder.DropTable(
                name: "CalendarPlanningBlocks"
            );
        }
    }
}