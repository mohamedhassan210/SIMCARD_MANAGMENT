using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Managment.Migrations
{
    /// <inheritdoc />
    public partial class add_ServiceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderId",
                table: "Usbs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderId",
                table: "Sims",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ServiceProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usbs_ServiceProviderId",
                table: "Usbs",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Sims_ServiceProviderId",
                table: "Sims",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ServiceProviderId",
                table: "Documents",
                column: "ServiceProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ServiceProviders_ServiceProviderId",
                table: "Documents",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sims_ServiceProviders_ServiceProviderId",
                table: "Sims",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usbs_ServiceProviders_ServiceProviderId",
                table: "Usbs",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ServiceProviders_ServiceProviderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Sims_ServiceProviders_ServiceProviderId",
                table: "Sims");

            migrationBuilder.DropForeignKey(
                name: "FK_Usbs_ServiceProviders_ServiceProviderId",
                table: "Usbs");

            migrationBuilder.DropTable(
                name: "ServiceProviders");

            migrationBuilder.DropIndex(
                name: "IX_Usbs_ServiceProviderId",
                table: "Usbs");

            migrationBuilder.DropIndex(
                name: "IX_Sims_ServiceProviderId",
                table: "Sims");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ServiceProviderId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Usbs");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Sims");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Documents");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
