using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Managment.Migrations
{
    /// <inheritdoc />
    public partial class edit_qouta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Period",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "Quotas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "Quotas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Quotas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "Quotas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "Quotas",
                type: "datetime2",
                nullable: true);
        }
    }
}
