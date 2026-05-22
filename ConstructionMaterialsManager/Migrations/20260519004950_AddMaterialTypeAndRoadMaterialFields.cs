using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionMaterialsManager.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialTypeAndRoadMaterialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Density",
                table: "Materials",
                type: "decimal(8,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fraction",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gost",
                table: "Materials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialTypeId",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialTypes",
                columns: table => new
                {
                    MaterialTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MaterialTypes", x => x.MaterialTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialTypeId",
                table: "Materials",
                column: "MaterialTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK__Materials__MaterialType",
                table: "Materials",
                column: "MaterialTypeId",
                principalTable: "MaterialTypes",
                principalColumn: "MaterialTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Materials__MaterialType",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "MaterialTypes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_MaterialTypeId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Density",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Fraction",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Gost",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "MaterialTypeId",
                table: "Materials");
        }
    }
}
