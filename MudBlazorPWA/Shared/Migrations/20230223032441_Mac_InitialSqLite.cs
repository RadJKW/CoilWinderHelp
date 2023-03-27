#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorPWA.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Mac_InitialSqLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeType",
                columns: table => new
                {
                    CodeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeType", x => x.CodeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WindingCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    D1FolderPath = table.Column<string>(type: "TEXT", nullable: true),
                    D2FolderPath = table.Column<string>(type: "TEXT", nullable: true),
                    D3FolderPath = table.Column<string>(type: "TEXT", nullable: true),
                    CodeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Media_Video = table.Column<string>(type: "TEXT", nullable: true),
                    Media_Pdf = table.Column<string>(type: "TEXT", nullable: true),
                    Media_ReferenceFolder = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindingCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WindingCodes_CodeType_CodeTypeId",
                        column: x => x.CodeTypeId,
                        principalTable: "CodeType",
                        principalColumn: "CodeTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CodeType",
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
                name: "CodeType");
        }
    }
}
