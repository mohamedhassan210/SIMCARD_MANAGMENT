using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class FireWallType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPhoneNumber",
                table: "ServiceTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteCode",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FireWallTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FireWallTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BranchFireWallType",
                columns: table => new
                {
                    BranchesId = table.Column<int>(type: "int", nullable: false),
                    FireWallTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchFireWallType", x => new { x.BranchesId, x.FireWallTypesId });
                    table.ForeignKey(
                        name: "FK_BranchFireWallType_Branches_BranchesId",
                        column: x => x.BranchesId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchFireWallType_FireWallTypes_FireWallTypesId",
                        column: x => x.FireWallTypesId,
                        principalTable: "FireWallTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchFireWallType_FireWallTypesId",
                table: "BranchFireWallType",
                column: "FireWallTypesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchFireWallType");

            migrationBuilder.DropTable(
                name: "FireWallTypes");

            migrationBuilder.DropColumn(
                name: "HasPhoneNumber",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "SiteCode",
                table: "Branches");
        }
    }
}
