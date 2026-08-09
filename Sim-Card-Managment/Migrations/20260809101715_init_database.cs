using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sim_Card_Management.Migrations
{
    /// <inheritdoc />
    public partial class init_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatusesType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatusesType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ControllerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExtraAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fees = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotas_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NetworkType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sims_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usbs_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    PerformedBy = table.Column<int>(type: "int", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    VpnOverInternetStatus = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSerialOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimId = table.Column<int>(type: "int", nullable: false),
                    OldSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimId = table.Column<int>(type: "int", nullable: true),
                    UsbId = table.Column<int>(type: "int", nullable: true),
                    StatusTypeId = table.Column<int>(type: "int", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportedBy = table.Column<int>(type: "int", nullable: false),
                    ReplacedBySimId = table.Column<int>(type: "int", nullable: true),
                    ReplacedByUsbId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_DeviceStatusesType_StatusTypeId",
                        column: x => x.StatusTypeId,
                        principalTable: "DeviceStatusesType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_Sims_ReplacedBySimId",
                        column: x => x.ReplacedBySimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_Usbs_ReplacedByUsbId",
                        column: x => x.ReplacedByUsbId,
                        principalTable: "Usbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceStatuses_Usbs_UsbId",
                        column: x => x.UsbId,
                        principalTable: "Usbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    ToEmpId = table.Column<int>(type: "int", nullable: false),
                    SimId = table.Column<int>(type: "int", nullable: true),
                    UsbId = table.Column<int>(type: "int", nullable: true),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    NewSubscriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTransfers_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceTransfers_Usbs_UsbId",
                        column: x => x.UsbId,
                        principalTable: "Usbs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentDetails_ItemTypes_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumenttypeId = table.Column<int>(type: "int", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SignatureType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SignatureData = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_DocumenttypeId",
                        column: x => x.DocumenttypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NationalID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermissions",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermissions", x => new { x.GroupId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Serials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SimId = table.Column<int>(type: "int", nullable: true),
                    UsbId = table.Column<int>(type: "int", nullable: true),
                    DocumentDetailsId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Serials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Serials_DocumentDetails_DocumentDetailsId",
                        column: x => x.DocumentDetailsId,
                        principalTable: "DocumentDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Serials_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Serials_Usbs_UsbId",
                        column: x => x.UsbId,
                        principalTable: "Usbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Serials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpId = table.Column<int>(type: "int", nullable: true),
                    NonEmployeeId = table.Column<int>(type: "int", nullable: true),
                    SimId = table.Column<int>(type: "int", nullable: false),
                    UsbId = table.Column<int>(type: "int", nullable: true),
                    QuotaId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fees = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Employees_EmpId",
                        column: x => x.EmpId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_NonEmployees_NonEmployeeId",
                        column: x => x.NonEmployeeId,
                        principalTable: "NonEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Quotas_QuotaId",
                        column: x => x.QuotaId,
                        principalTable: "Quotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Usbs_UsbId",
                        column: x => x.UsbId,
                        principalTable: "Usbs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VpnConnectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnConnectionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VpnConnectionTypes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternetLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    SimId = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Bandwidth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RenewalDay = table.Column<int>(type: "int", nullable: true),
                    QuotaGB = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternetLines_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternetLines_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternetLines_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternetLines_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternetLines_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternetLines_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiverSignatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    SignedBy = table.Column<int>(type: "int", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignatureType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SignatureData = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiverSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiverSignatures_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiverSignatures_Users_SignedBy",
                        column: x => x.SignedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VpnConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ConnectionTypeId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    NID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineSpeed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VpnConnections_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VpnConnections_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VpnConnections_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VpnConnections_VpnConnectionTypes_ConnectionTypeId",
                        column: x => x.ConnectionTypeId,
                        principalTable: "VpnConnectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PerformedBy",
                table: "AuditLogs",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CreatedById",
                table: "Branches",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSerialOperations_CreatedById",
                table: "DeviceSerialOperations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSerialOperations_SimId",
                table: "DeviceSerialOperations",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_ReplacedBySimId",
                table: "DeviceStatuses",
                column: "ReplacedBySimId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_ReplacedByUsbId",
                table: "DeviceStatuses",
                column: "ReplacedByUsbId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_ReportedBy",
                table: "DeviceStatuses",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_SimId",
                table: "DeviceStatuses",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_StatusTypeId",
                table: "DeviceStatuses",
                column: "StatusTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatuses_UsbId",
                table: "DeviceStatuses",
                column: "UsbId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_CreatedBy",
                table: "DeviceTransfers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_FromSubscriptionId",
                table: "DeviceTransfers",
                column: "FromSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_NewSubscriptionId",
                table: "DeviceTransfers",
                column: "NewSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_SimId",
                table: "DeviceTransfers",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_ToEmpId",
                table: "DeviceTransfers",
                column: "ToEmpId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTransfers_UsbId",
                table: "DeviceTransfers",
                column: "UsbId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetails_DocumentId",
                table: "DocumentDetails",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetails_ItemTypeId",
                table: "DocumentDetails",
                column: "ItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumenttypeId",
                table: "Documents",
                column: "DocumenttypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ServiceProviderId",
                table: "Documents",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalID",
                table: "Employees",
                column: "NationalID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_PermissionId",
                table: "GroupPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedById",
                table: "Groups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_BranchId",
                table: "InternetLines",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_CreatedById",
                table: "InternetLines",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_PaymentTypeId",
                table: "InternetLines",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_ServiceProviderId",
                table: "InternetLines",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_ServiceTypeId",
                table: "InternetLines",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InternetLines_SimId",
                table: "InternetLines",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_CreatedById",
                table: "PaymentTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Quotas_ServiceProviderId",
                table: "Quotas",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiverSignatures_SignedBy",
                table: "ReceiverSignatures",
                column: "SignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiverSignatures_SubscriptionId",
                table: "ReceiverSignatures",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Serials_DocumentDetailsId",
                table: "Serials",
                column: "DocumentDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Serials_SimId",
                table: "Serials",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_Serials_UsbId",
                table: "Serials",
                column: "UsbId");

            migrationBuilder.CreateIndex(
                name: "IX_Serials_UserId",
                table: "Serials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sims_SerialNumber",
                table: "Sims",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sims_ServiceProviderId",
                table: "Sims",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ActionId",
                table: "Subscriptions",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CreatedBy",
                table: "Subscriptions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_EmpId",
                table: "Subscriptions",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_NonEmployeeId",
                table: "Subscriptions",
                column: "NonEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_QuotaId",
                table: "Subscriptions",
                column: "QuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SimId",
                table: "Subscriptions",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UsbId",
                table: "Subscriptions",
                column: "UsbId");

            migrationBuilder.CreateIndex(
                name: "IX_Usbs_SerialNumber",
                table: "Usbs",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usbs_ServiceProviderId",
                table: "Usbs",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VpnConnections_BranchId",
                table: "VpnConnections",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_VpnConnections_ConnectionTypeId",
                table: "VpnConnections",
                column: "ConnectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VpnConnections_CreatedById",
                table: "VpnConnections",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_VpnConnections_ServiceProviderId",
                table: "VpnConnections",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_VpnConnectionTypes_CreatedById",
                table: "VpnConnectionTypes",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_PerformedBy",
                table: "AuditLogs",
                column: "PerformedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_CreatedById",
                table: "Branches",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSerialOperations_Users_CreatedById",
                table: "DeviceSerialOperations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceStatuses_Users_ReportedBy",
                table: "DeviceStatuses",
                column: "ReportedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTransfers_Employees_ToEmpId",
                table: "DeviceTransfers",
                column: "ToEmpId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTransfers_Subscriptions_FromSubscriptionId",
                table: "DeviceTransfers",
                column: "FromSubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTransfers_Subscriptions_NewSubscriptionId",
                table: "DeviceTransfers",
                column: "NewSubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTransfers_Users_CreatedBy",
                table: "DeviceTransfers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetails_Documents_DocumentId",
                table: "DocumentDetails",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UserId",
                table: "Documents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_UserId",
                table: "Employees",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPermissions_Groups_GroupId",
                table: "GroupPermissions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatedById",
                table: "Groups",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatedById",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DeviceSerialOperations");

            migrationBuilder.DropTable(
                name: "DeviceStatuses");

            migrationBuilder.DropTable(
                name: "DeviceTransfers");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "InternetLines");

            migrationBuilder.DropTable(
                name: "ReceiverSignatures");

            migrationBuilder.DropTable(
                name: "Serials");

            migrationBuilder.DropTable(
                name: "UserOtps");

            migrationBuilder.DropTable(
                name: "VpnConnections");

            migrationBuilder.DropTable(
                name: "DeviceStatusesType");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "DocumentDetails");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "VpnConnectionTypes");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "NonEmployees");

            migrationBuilder.DropTable(
                name: "Quotas");

            migrationBuilder.DropTable(
                name: "Sims");

            migrationBuilder.DropTable(
                name: "Usbs");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "ItemTypes");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "ServiceProviders");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
