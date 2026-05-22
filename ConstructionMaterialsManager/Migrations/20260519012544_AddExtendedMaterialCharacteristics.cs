using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionMaterialsManager.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedMaterialCharacteristics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinenessModule",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrostResistance",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Leshchadness",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Materials",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadioactivityClass",
                table: "Materials",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageType",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Открытый");

            migrationBuilder.AddColumn<string>(
                name: "StrengthGrade",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaterResistance",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "Deliveries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CertificateDate",
                table: "Deliveries",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "Deliveries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Deliveries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Deliveries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QualityChecks",
                columns: table => new
                {
                    QualityCheckId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    DeliveryId = table.Column<int>(type: "int", nullable: true),
                    CheckDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "На проверке"),
                    InspectorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TestResults = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__QualityChecks", x => x.QualityCheckId);
                    table.ForeignKey(
                        name: "FK__QualityChecks__Delivery",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "DeliveryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__QualityChecks__Material",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_DeliveryId",
                table: "QualityChecks",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_MaterialId",
                table: "QualityChecks",
                column: "MaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "FinenessModule",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "FrostResistance",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Leshchadness",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "RadioactivityClass",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "StrengthGrade",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "WaterResistance",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CertificateDate",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Deliveries");
        }
    }
}
