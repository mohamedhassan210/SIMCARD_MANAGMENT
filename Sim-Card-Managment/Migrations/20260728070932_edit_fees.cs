using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Managment.Migrations
{
    /// <inheritdoc />
    public partial class edit_fees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmpCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpCode",
                table: "Employees");
        }
    }
}
