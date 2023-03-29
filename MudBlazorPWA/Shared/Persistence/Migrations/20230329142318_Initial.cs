using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorPWA.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeType",
                columns: table => new
                {
                    CodeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
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
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Division = table.Column<int>(type: "INTEGER", nullable: false),
                    Stop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FolderPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CodeTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Video = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Pdf = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RefMedia = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "IX_WindingCodes_Code_Division",
                table: "WindingCodes",
                columns: new[] { "Code", "Division" },
                unique: true);

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
