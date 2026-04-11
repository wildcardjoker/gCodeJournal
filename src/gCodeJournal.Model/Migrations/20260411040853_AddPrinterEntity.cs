using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gCodeJournal.Model.Migrations
{
    /// <inheritdoc />
    public partial class AddPrinterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFilamentManufacturer",
                table: "Manufacturers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinterManufacturer",
                table: "Manufacturers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Printer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ManufacturerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Printer_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true,true });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, true });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, true });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "IsFilamentManufacturer", "IsPrinterManufacturer" },
                values: new object[] { true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Printer_ManufacturerId",
                table: "Printer",
                column: "ManufacturerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Printer");

            migrationBuilder.DropColumn(
                name: "IsFilamentManufacturer",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "IsPrinterManufacturer",
                table: "Manufacturers");
        }
    }
}
