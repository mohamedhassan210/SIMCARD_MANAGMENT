using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceProviderPhonePrefixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhonePrefixes",
                table: "ServiceProviders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhonePrefixes",
                table: "ServiceProviders");
        }
    }
}
