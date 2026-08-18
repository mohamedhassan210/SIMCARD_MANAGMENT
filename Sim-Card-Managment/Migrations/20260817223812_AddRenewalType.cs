using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RenewalDay",
                table: "InternetLines",
                newName: "RenewaltypeId");

            // Make the renamed column nullable so old RenewalDay values (which
            // don't correspond to any RenewalTypes.Id) can be cleared out below.
            migrationBuilder.AlterColumn<int>(
                name: "RenewaltypeId",
                table: "InternetLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Clear stale day-of-month values so they don't violate the new FK.
            migrationBuilder.Sql("UPDATE [InternetLines] SET [RenewaltypeId] = NULL;");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastRenewalDate",
                table: "InternetLines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NextRenewalDate",
                table: "InternetLines",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RenewalTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_RenewaltypeId",
                table: "InternetLines",
                column: "RenewaltypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternetLines_RenewalTypes_RenewaltypeId",
                table: "InternetLines",
                column: "RenewaltypeId",
                principalTable: "RenewalTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternetLines_RenewalTypes_RenewaltypeId",
                table: "InternetLines");

            migrationBuilder.DropTable(
                name: "RenewalTypes");

            migrationBuilder.DropIndex(
                name: "IX_InternetLines_RenewaltypeId",
                table: "InternetLines");

            migrationBuilder.DropColumn(
                name: "LastRenewalDate",
                table: "InternetLines");

            migrationBuilder.DropColumn(
                name: "NextRenewalDate",
                table: "InternetLines");

            migrationBuilder.AlterColumn<int>(
                name: "RenewaltypeId",
                table: "InternetLines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "RenewaltypeId",
                table: "InternetLines",
                newName: "RenewalDay");
        }
    }
}
