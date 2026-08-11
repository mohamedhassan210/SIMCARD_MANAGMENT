using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSimUsbStatusWithIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Usbs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Sims");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Usbs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sims",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Usbs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sims");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Usbs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Sims",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
