using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trips.Api.Migrations
{
    /// <inheritdoc />
    public partial class TripsDTORefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TripNumberHelper",
                table: "TripNumberHelper");

            migrationBuilder.RenameTable(
                name: "TripNumberHelper",
                newName: "TripNumberHelpers");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Trips",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "TripDestinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "TripDestinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripNumberHelpers",
                table: "TripNumberHelpers",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TripNumberHelpers",
                table: "TripNumberHelpers");

            migrationBuilder.RenameTable(
                name: "TripNumberHelpers",
                newName: "TripNumberHelper");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Trips",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "TripDestinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "TripDestinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripNumberHelper",
                table: "TripNumberHelper",
                column: "Date");
        }
    }
}
