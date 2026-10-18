using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resources.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnitPriceRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PartUsages");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Resources",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Resources");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PartUsages",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
