using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class editDeviceAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Actions_ActionId",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.RenameTable(
                name: "Actions",
                newName: "DeviceActions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceActions",
                table: "DeviceActions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_DeviceActions_ActionId",
                table: "Subscriptions",
                column: "ActionId",
                principalTable: "DeviceActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_DeviceActions_ActionId",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceActions",
                table: "DeviceActions");

            migrationBuilder.RenameTable(
                name: "DeviceActions",
                newName: "Actions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Actions_ActionId",
                table: "Subscriptions",
                column: "ActionId",
                principalTable: "Actions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
