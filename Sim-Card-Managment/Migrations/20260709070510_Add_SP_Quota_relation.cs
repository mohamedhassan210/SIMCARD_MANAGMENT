using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Managment.Migrations
{
    /// <inheritdoc />
    public partial class Add_SP_Quota_relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fees",
                table: "Sims");

            migrationBuilder.AddColumn<decimal>(
                name: "Fees",
                table: "Subscriptions",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Quotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderId",
                table: "Quotas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Quotas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
                onDelete: ReferentialAction.Restrict);
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
                name: "Fees",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Quotas");

            migrationBuilder.AddColumn<decimal>(
                name: "Fees",
                table: "Sims",
                type: "decimal(10,2)",
                nullable: true);
        }
    }
}
