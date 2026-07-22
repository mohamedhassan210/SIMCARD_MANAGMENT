using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Managment.Migrations
{
    /// <inheritdoc />
    public partial class QuotaEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderId",
                table: "Quotas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Quotas_ServiceProviderId",
                table: "Quotas",
                column: "ServiceProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotas_ServiceProviders_ServiceProviderId",
                table: "Quotas",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotas_ServiceProviders_ServiceProviderId",
                table: "Quotas");

            migrationBuilder.DropIndex(
                name: "IX_Quotas_ServiceProviderId",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Quotas");
        }
    }
}
