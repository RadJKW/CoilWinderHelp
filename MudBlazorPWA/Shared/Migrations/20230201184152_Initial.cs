using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorPWA.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeTypes",
                columns: table => new
                {
                    CodeTypeId = table.Column<int>(type: "int", nullable: false),
                    CodeType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeTypes", x => x.CodeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WindingCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FolderPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindingCodes", x => x.Code);
                    table.ForeignKey(
                        name: "FK_WindingCodes_CodeTypes_CodeTypeId",
                        column: x => x.CodeTypeId,
                        principalTable: "CodeTypes",
                        principalColumn: "CodeTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CodeTypes",
                columns: new[] { "CodeTypeId", "CodeType" },
                values: new object[,]
                {
                    { 0, "Stop" },
                    { 1, "Almost" },
                    { 2, "Data" },
                    { 3, "Layer" },
                    { 4, "Material" },
                    { 5, "None" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WindingCodes_CodeTypeId",
                table: "WindingCodes",
                column: "CodeTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WindingCodes");

            migrationBuilder.DropTable(
                name: "CodeTypes");
        }
    }
}
