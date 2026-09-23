// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace AllocatrApi.Migrations
// {
//     /// <inheritdoc />
//     public partial class AddUserProfileFields : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_ProjectAllocats_AspNetUsers_AllocatId",
//                 table: "ProjectAllocats");

//             migrationBuilder.RenameColumn(
//                 name: "AssignedAt",
//                 table: "ProjectAllocats",
//                 newName: "InvitedAt");

//             migrationBuilder.RenameColumn(
//                 name: "AllocatId",
//                 table: "ProjectAllocats",
//                 newName: "AllocatProfileId");

//             migrationBuilder.RenameIndex(
//                 name: "IX_ProjectAllocats_AllocatId",
//                 table: "ProjectAllocats",
//                 newName: "IX_ProjectAllocats_AllocatProfileId");

//             migrationBuilder.AddColumn<DateTime>(
//                 name: "RemovedAt",
//                 table: "ProjectAllocats",
//                 type: "timestamp with time zone",
//                 nullable: true);

//             migrationBuilder.AddColumn<DateTime>(
//                 name: "RespondedAt",
//                 table: "ProjectAllocats",
//                 type: "timestamp with time zone",
//                 nullable: true);

//             migrationBuilder.AddColumn<string>(
//                 name: "Status",
//                 table: "ProjectAllocats",
//                 type: "character varying(20)",
//                 maxLength: 20,
//                 nullable: false,
//                 defaultValue: "");

//             migrationBuilder.AlterColumn<bool>(
//                 name: "IsAllocat",
//                 table: "AspNetUsers",
//                 type: "boolean",
//                 nullable: false,
//                 defaultValue: false,
//                 oldClrType: typeof(bool),
//                 oldType: "boolean");

//             migrationBuilder.AlterColumn<string>(
//                 name: "FullName",
//                 table: "AspNetUsers",
//                 type: "character varying(150)",
//                 maxLength: 150,
//                 nullable: false,
//                 oldClrType: typeof(string),
//                 oldType: "character varying(60)",
//                 oldMaxLength: 60);

//             migrationBuilder.AlterColumn<string>(
//                 name: "AvatarUrl",
//                 table: "AspNetUsers",
//                 type: "character varying(1000)",
//                 maxLength: 1000,
//                 nullable: true,
//                 oldClrType: typeof(string),
//                 oldType: "text",
//                 oldNullable: true);

//             migrationBuilder.AddColumn<DateTime>(
//                 name: "CreatedAt",
//                 table: "AspNetUsers",
//                 type: "timestamp with time zone",
//                 nullable: false,
//                 defaultValueSql: "CURRENT_TIMESTAMP");

//             migrationBuilder.AddColumn<string>(
//                 name: "Location",
//                 table: "AspNetUsers",
//                 type: "character varying(150)",
//                 maxLength: 150,
//                 nullable: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_ProjectAllocats_AllocatProfileId_Status",
//                 table: "ProjectAllocats",
//                 columns: new[] { "AllocatProfileId", "Status" });

//             migrationBuilder.CreateIndex(
//                 name: "IX_ProjectAllocats_ProjectId",
//                 table: "ProjectAllocats",
//                 column: "ProjectId");

//             migrationBuilder.CreateIndex(
//                 name: "IX_ProjectAllocats_ProjectId_Status",
//                 table: "ProjectAllocats",
//                 columns: new[] { "ProjectId", "Status" });

//             migrationBuilder.CreateIndex(
//                 name: "IX_ProjectAllocats_Status",
//                 table: "ProjectAllocats",
//                 column: "Status");

//             migrationBuilder.AddForeignKey(
//                 name: "FK_ProjectAllocats_AllocatProfiles_AllocatProfileId",
//                 table: "ProjectAllocats",
//                 column: "AllocatProfileId",
//                 principalTable: "AllocatProfiles",
//                 principalColumn: "AllocatrUserId",
//                 onDelete: ReferentialAction.Cascade);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_ProjectAllocats_AllocatProfiles_AllocatProfileId",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropIndex(
//                 name: "IX_ProjectAllocats_AllocatProfileId_Status",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropIndex(
//                 name: "IX_ProjectAllocats_ProjectId",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropIndex(
//                 name: "IX_ProjectAllocats_ProjectId_Status",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropIndex(
//                 name: "IX_ProjectAllocats_Status",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropColumn(
//                 name: "RemovedAt",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropColumn(
//                 name: "RespondedAt",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropColumn(
//                 name: "Status",
//                 table: "ProjectAllocats");

//             migrationBuilder.DropColumn(
//                 name: "CreatedAt",
//                 table: "AspNetUsers");

//             migrationBuilder.DropColumn(
//                 name: "Location",
//                 table: "AspNetUsers");

//             migrationBuilder.RenameColumn(
//                 name: "InvitedAt",
//                 table: "ProjectAllocats",
//                 newName: "AssignedAt");

//             migrationBuilder.RenameColumn(
//                 name: "AllocatProfileId",
//                 table: "ProjectAllocats",
//                 newName: "AllocatId");

//             migrationBuilder.RenameIndex(
//                 name: "IX_ProjectAllocats_AllocatProfileId",
//                 table: "ProjectAllocats",
//                 newName: "IX_ProjectAllocats_AllocatId");

//             migrationBuilder.AlterColumn<bool>(
//                 name: "IsAllocat",
//                 table: "AspNetUsers",
//                 type: "boolean",
//                 nullable: false,
//                 oldClrType: typeof(bool),
//                 oldType: "boolean",
//                 oldDefaultValue: false);

//             migrationBuilder.AlterColumn<string>(
//                 name: "FullName",
//                 table: "AspNetUsers",
//                 type: "character varying(60)",
//                 maxLength: 60,
//                 nullable: false,
//                 oldClrType: typeof(string),
//                 oldType: "character varying(150)",
//                 oldMaxLength: 150);

//             migrationBuilder.AlterColumn<string>(
//                 name: "AvatarUrl",
//                 table: "AspNetUsers",
//                 type: "text",
//                 nullable: true,
//                 oldClrType: typeof(string),
//                 oldType: "character varying(1000)",
//                 oldMaxLength: 1000,
//                 oldNullable: true);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_ProjectAllocats_AspNetUsers_AllocatId",
//                 table: "ProjectAllocats",
//                 column: "AllocatId",
//                 principalTable: "AspNetUsers",
//                 principalColumn: "Id",
//                 onDelete: ReferentialAction.Cascade);
//         }
//     }
// }


using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    public partial class AddUserProfileFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsAllocat",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAllocat",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}