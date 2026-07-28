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
            migrationBuilder.AddColumn<decimal>(
         name: "Fees",
         table: "Quotas",
         type: "decimal(10,2)",
         nullable: false,
         defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
        name: "Fees",
        table: "Quotas");
        }
    }
}
