using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gCodeJournal.Model.Migrations
{
    /// <inheritdoc />
    public partial class AddCostToPrinterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPerHour",
                table: "Printer",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPerHour",
                table: "Printer");
        }
    }
}
