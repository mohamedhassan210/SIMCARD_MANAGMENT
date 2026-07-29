using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class addDeviceSerialOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceSerialOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimId = table.Column<int>(type: "int", nullable: false),
                    OldSerialNumber = table.Column<int>(type: "int", nullable: false),
                    NewSerialNumber = table.Column<int>(type: "int", nullable: false),
                    NetworkTypeChange = table.Column<bool>(type: "bit", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSerialOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSerialOperations_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceSerialOperations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSerialOperations_CreatedById",
                table: "DeviceSerialOperations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSerialOperations_SimId",
                table: "DeviceSerialOperations",
                column: "SimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceSerialOperations");
        }
    }
}
