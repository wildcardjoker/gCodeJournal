using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gCodeJournal.Model.Migrations
{
    /// <inheritdoc />
    public partial class MultipleFilamentsPerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Filaments_PrintingProjects_PrintingProjectId",
                table: "Filaments");

            migrationBuilder.DropIndex(
                name: "IX_Filaments_PrintingProjectId",
                table: "Filaments");

            migrationBuilder.DropColumn(
                name: "FilamentId",
                table: "PrintingProjects");

            migrationBuilder.DropColumn(
                name: "PrintingProjectId",
                table: "Filaments");

            migrationBuilder.CreateTable(
                name: "PrintingProjectFilaments",
                columns: table => new
                {
                    FilamentsId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintingProjectFilaments", x => new { x.FilamentsId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_PrintingProjectFilaments_Filaments_FilamentsId",
                        column: x => x.FilamentsId,
                        principalTable: "Filaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrintingProjectFilaments_PrintingProjects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "PrintingProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrintingProjectFilaments_ProjectsId",
                table: "PrintingProjectFilaments",
                column: "ProjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintingProjectFilaments");

            migrationBuilder.AddColumn<int>(
                name: "FilamentId",
                table: "PrintingProjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrintingProjectId",
                table: "Filaments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Filaments",
                keyColumn: "Id",
                keyValue: 1,
                column: "PrintingProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Filaments",
                keyColumn: "Id",
                keyValue: 2,
                column: "PrintingProjectId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Filaments_PrintingProjectId",
                table: "Filaments",
                column: "PrintingProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Filaments_PrintingProjects_PrintingProjectId",
                table: "Filaments",
                column: "PrintingProjectId",
                principalTable: "PrintingProjects",
                principalColumn: "Id");
        }
    }
}
