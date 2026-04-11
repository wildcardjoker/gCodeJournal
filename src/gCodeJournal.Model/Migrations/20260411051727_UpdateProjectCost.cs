using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gCodeJournal.Model.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "PrintingProjects",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "PrintingProjects",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");
        }
    }
}
