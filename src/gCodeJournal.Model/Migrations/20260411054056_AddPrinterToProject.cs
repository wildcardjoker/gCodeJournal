using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gCodeJournal.Model.Migrations
{
    /// <inheritdoc />
    public partial class AddPrinterToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrinterId",
                table: "PrintingProjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PrintingProjects_PrinterId",
                table: "PrintingProjects",
                column: "PrinterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintingProjects_Printer_PrinterId",
                table: "PrintingProjects",
                column: "PrinterId",
                principalTable: "Printer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintingProjects_Printer_PrinterId",
                table: "PrintingProjects");

            migrationBuilder.DropIndex(
                name: "IX_PrintingProjects_PrinterId",
                table: "PrintingProjects");

            migrationBuilder.DropColumn(
                name: "PrinterId",
                table: "PrintingProjects");
        }
    }
}
