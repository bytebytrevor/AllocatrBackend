using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNewAllocatProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "RatingCount",
                table: "AllocatProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVisible",
                table: "AllocatProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AllocatProfiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "AllocatProfiles",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AllocatProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "AllocatProfiles",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Availability",
                table: "AllocatProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "available",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AverageResponseTimeMinutes",
                table: "AllocatProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "AllocatProfiles",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "Headline",
                table: "AllocatProfiles",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "AllocatProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "AllocatProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AllocatProfiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_Availability",
                table: "AllocatProfiles",
                column: "Availability");

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_HourlyRate",
                table: "AllocatProfiles",
                column: "HourlyRate");

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_IdNumber",
                table: "AllocatProfiles",
                column: "IdNumber",
                unique: true,
                filter: "\"IdNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_IsVisible",
                table: "AllocatProfiles",
                column: "IsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_IsVisible_Availability",
                table: "AllocatProfiles",
                columns: new[] { "IsVisible", "Availability" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_Availability",
                table: "AllocatProfiles",
                sql: "\"Availability\" IN ('available', 'busy', 'unavailable')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_AverageRating",
                table: "AllocatProfiles",
                sql: "\"AverageRating\" >= 0 AND \"AverageRating\" <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_HourlyRate",
                table: "AllocatProfiles",
                sql: "\"HourlyRate\" IS NULL OR \"HourlyRate\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_Level",
                table: "AllocatProfiles",
                sql: "\"Level\" >= 1 AND \"Level\" <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_RatingCount",
                table: "AllocatProfiles",
                sql: "\"RatingCount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_ResponseTime",
                table: "AllocatProfiles",
                sql: "\"AverageResponseTimeMinutes\" IS NULL OR \"AverageResponseTimeMinutes\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_YearsExperience",
                table: "AllocatProfiles",
                sql: "\"YearsExperience\" IS NULL OR (\"YearsExperience\" >= 0 AND \"YearsExperience\" <= 80)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_Availability",
                table: "AllocatProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_HourlyRate",
                table: "AllocatProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_IdNumber",
                table: "AllocatProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_IsVisible",
                table: "AllocatProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_IsVisible_Availability",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_Availability",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_AverageRating",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_HourlyRate",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_Level",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_RatingCount",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_ResponseTime",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_YearsExperience",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "AverageResponseTimeMinutes",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "Headline",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AllocatProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<int>(
                name: "RatingCount",
                table: "AllocatProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsVisible",
                table: "AllocatProfiles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AllocatProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "AllocatProfiles",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AllocatProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageRating",
                table: "AllocatProfiles",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldPrecision: 3,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Availability",
                table: "AllocatProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "available");
        }
    }
}
