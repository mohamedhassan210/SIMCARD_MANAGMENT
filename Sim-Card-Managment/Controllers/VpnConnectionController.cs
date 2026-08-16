using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.LookupRepos;
using Sim_Card_Managment.Repos.VpnConnectionRepos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class VpnConnectionController : Controller
    {
        private readonly IVpnConnectionRepo _vpnRepo;
        private readonly IBranchRepo _branchRepo;
        private readonly ILookupRepo _lookupRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;

        public VpnConnectionController(
            IVpnConnectionRepo vpnRepo,
            IBranchRepo branchRepo,
            ILookupRepo lookupRepo,
            IServiceProviderRepository serviceProviderRepo)
        {
            _vpnRepo = vpnRepo;
            _branchRepo = branchRepo;
            _lookupRepo = lookupRepo;
            _serviceProviderRepo = serviceProviderRepo;
        }

        private async Task PopulateDropdowns(VpnConnectionCreateViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var connectionTypes = await _lookupRepo.GetVpnConnectionTypesAsync();

            model.Branches = branches
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                });

            model.ServiceProviders = providers
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                });

            model.ConnectionTypes = connectionTypes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
        }

        private async Task PopulateDropdowns(VpnConnectionEditViewModel model)
        {
            var branches = await _branchRepo.GetAllAsync();
            var providers = await _serviceProviderRepo.GetAllAsync();
            var connectionTypes = await _lookupRepo.GetVpnConnectionTypesAsync();

            model.Branches = branches
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                });

            model.ServiceProviders = providers
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                });

            model.ConnectionTypes = connectionTypes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vpns = await _vpnRepo.GetAllAsync();

            return View(vpns);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vpn = await _vpnRepo.GetByIdWithDetailsAsync(id);

            if (vpn == null)
                return NotFound();

            return View(vpn);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new VpnConnectionCreateViewModel();

            await PopulateDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VpnConnectionCreateViewModel model)
        {
            // Get the currently logged-in user's ID
            if (int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int currentUserId))
            {
                model.CreatedById = currentUserId;
            }
            else
            {
                // Development fallback
                model.CreatedById = 1;
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);

                return View(model);
            }

            await _vpnRepo.AddAsync(model);

            TempData["Success"] = "VPN connection created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _vpnRepo.GetForEditAsync(id);

            if (model == null)
                return NotFound();

            await PopulateDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VpnConnectionEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);

                return View(model);
            }

            await _vpnRepo.UpdateAsync(model);

            TempData["Success"] = "VPN connection updated successfully.";

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        #region Reports

        [HttpGet]
        public async Task<IActionResult> Report(string? searchTerm)
        {
            var branches = await _branchRepo.GetAllAsync();
            var allVpnConnections = await _vpnRepo.GetAllAsync();

            var model = new VpnConnectionReportViewModel();

            foreach (var branch in branches)
            {
                var vpnConnections = allVpnConnections
                    .Where(v => v.BranchName == branch.Name).ToList();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    vpnConnections = vpnConnections.Where(v =>
                        v.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (v.ServiceProviderName != null && v.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (v.NID != null && v.NID.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                model.Branches.Add(new BranchVpnItem
                {
                    BranchName = branch.Name,
                    IsActive = branch.IsActive,
                    VpnOverInternetStatus = branch.VpnOverInternetStatus,
                    VpnConnections = vpnConnections
                });
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? searchTerm)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            // =========================================================
            // GET DATA
            // =========================================================

            var data = await _vpnRepo.GetForExcelAsync();

            // Get ALL VPN connection types dynamically
            var connectionTypes = (await _lookupRepo.GetVpnConnectionTypesAsync())
                .OrderBy(x => x.Id)
                .ToList();

            // Branch name -> comma-separated firewall type names, for
            // branches with VPN over internet configured.
            var fireWallTypesByBranch = await _branchRepo.GetFireWallTypeNamesByBranchNameAsync();

            // =========================================================
            // SEARCH
            // =========================================================

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                data = data
                    .Where(b =>
                        b.BranchName.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase)

                        ||

                        b.Connections.Any(v =>
                            v.ServiceProviderName.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)

                            ||

                            v.NID?.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase) == true

                            ||

                            v.ConnectionTypeName.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToList();
            }

            // =========================================================
            // CREATE EXCEL
            // =========================================================

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Vpn");

            // =========================================================
            // CALCULATE COLUMNS
            // =========================================================

            // Branch
            // 4 columns per VPN type:
            // ISP / Line Speed / NID / Status
            // VPN Over Internet
            // Firewall Types
            // Notes

            int totalColumns =
                1 +
                (connectionTypes.Count * 4) +
                1 +
                1 +
                1;

            int lastColumn = totalColumns;

            // =========================================================
            // TITLE
            // =========================================================

            worksheet.Cells[1, 1].Value = "VPN - leased line";

            worksheet.Cells[1, 1, 1, lastColumn].Merge = true;

            var titleRange =
                worksheet.Cells[1, 1, 1, lastColumn];

            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;

            titleRange.Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            titleRange.Style.VerticalAlignment =
                ExcelVerticalAlignment.Center;

            titleRange.Style.Fill.PatternType =
                ExcelFillStyle.Solid;

            titleRange.Style.Fill.BackgroundColor
                .SetColor(Color.White);

            // =========================================================
            // HEADERS
            // =========================================================

            int headerRow = 2;
            int column = 1;

            worksheet.Cells[headerRow, column].Value = "Branch";
            column++;

            foreach (var type in connectionTypes)
            {
                worksheet.Cells[headerRow, column].Value =
                    $"{type.Name} ISP";
                column++;

                worksheet.Cells[headerRow, column].Value =
                    $"{type.Name} Line Speed";
                column++;

                worksheet.Cells[headerRow, column].Value =
                    $"{type.Name} NID";
                column++;

                worksheet.Cells[headerRow, column].Value =
                    $"{type.Name} Status";
                column++;
            }

            worksheet.Cells[headerRow, column].Value =
                "VPN Over Internet";

            column++;

            worksheet.Cells[headerRow, column].Value =
                "Firewall Types";

            column++;

            worksheet.Cells[headerRow, column].Value =
                "Notes";

            // =========================================================
            // HEADER STYLE
            // =========================================================

            using (var headerRange =
                worksheet.Cells[headerRow, 1, headerRow, lastColumn])
            {
                headerRange.Style.Font.Bold = true;

                headerRange.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                headerRange.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                headerRange.Style.Fill.PatternType =
                    ExcelFillStyle.Solid;

                headerRange.Style.Fill.BackgroundColor
                    .SetColor(Color.LightGray);

                headerRange.Style.Border.Top.Style =
                    ExcelBorderStyle.Thin;

                headerRange.Style.Border.Bottom.Style =
                    ExcelBorderStyle.Thin;

                headerRange.Style.Border.Left.Style =
                    ExcelBorderStyle.Thin;

                headerRange.Style.Border.Right.Style =
                    ExcelBorderStyle.Thin;

                headerRange.Style.WrapText = true;
            }

            // =========================================================
            // DATA
            // =========================================================

            int row = 3;

            // Track each branch's row range + wrapped text so we can
            // set row heights AFTER column widths are finalized below.
            // Merged cells don't auto-expand row height for wrapped
            // text in Excel/EPPlus, so this has to be done manually.
            var branchRowRanges = new List<(int StartRow, int EndRow, string FirewallText, string NotesText)>();

            foreach (var branch in data.OrderBy(x => x.BranchName))
            {
                // -----------------------------------------------------
                // Group this branch's connections by connection type
                // -----------------------------------------------------

                var connectionsByType = connectionTypes
                    .ToDictionary(
                        type => type.Name,
                        type => branch.Connections
                            .Where(x =>
                                x.ConnectionTypeName.Equals(
                                    type.Name,
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList(),
                        StringComparer.OrdinalIgnoreCase);

                // -----------------------------------------------------
                // Find how many rows this branch needs
                //
                // Example:
                // Main   = 5
                // Backup = 2
                // Mobile = 1
                //
                // Branch needs 5 rows.
                // -----------------------------------------------------

                int branchRowCount = connectionsByType.Values
                    .Select(list => list.Count)
                    .DefaultIfEmpty(1)
                    .Max();

                // Make sure a branch with no VPN connections still
                // gets one row.
                if (branchRowCount < 1)
                    branchRowCount = 1;

                int branchStartRow = row;
                int branchEndRow = row + branchRowCount - 1;

                // -----------------------------------------------------
                // BRANCH
                // -----------------------------------------------------

                worksheet.Cells[
                    branchStartRow,
                    1,
                    branchEndRow,
                    1
                ].Merge = true;

                var branchCell =
                    worksheet.Cells[branchStartRow, 1];

                branchCell.Value = branch.BranchName;

                branchCell.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                branchCell.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                branchCell.Style.WrapText = true;

                // -----------------------------------------------------
                // VPN TYPES
                // -----------------------------------------------------

                foreach (var type in connectionTypes)
                {
                    var connections =
                        connectionsByType[type.Name];

                    // Starting column for this VPN type
                    int typeStartColumn =
                        2 + (connectionTypes.IndexOf(type) * 4);

                    // -------------------------------------------------
                    // Put EACH connection on its OWN ROW
                    // -------------------------------------------------

                    for (int i = 0; i < connections.Count; i++)
                    {
                        var connection = connections[i];

                        int currentRow =
                            branchStartRow + i;

                        // ISP
                        worksheet.Cells[
                            currentRow,
                            typeStartColumn
                        ].Value =
                            connection.ServiceProviderName;

                        // Line Speed
                        worksheet.Cells[
                            currentRow,
                            typeStartColumn + 1
                        ].Value =
                            connection.LineSpeed;

                        // NID
                        worksheet.Cells[
                            currentRow,
                            typeStartColumn + 2
                        ].Value =
                            connection.NID;

                        // Status
                        var statusCell =
                            worksheet.Cells[
                                currentRow,
                                typeStartColumn + 3
                            ];

                        statusCell.Value =
                            connection.Status == true
                                ? "Online"
                                : connection.Status == false
                                    ? "Offline"
                                    : "Unknown";

                        statusCell.Style.HorizontalAlignment =
                            ExcelHorizontalAlignment.Center;

                        // Status color
                        if (connection.Status == true)
                        {
                            statusCell.Style.Fill.PatternType =
                                ExcelFillStyle.Solid;

                            statusCell.Style.Fill.BackgroundColor
                                .SetColor(Color.LimeGreen);
                        }
                        else if (connection.Status == false)
                        {
                            statusCell.Style.Fill.PatternType =
                                ExcelFillStyle.Solid;

                            statusCell.Style.Fill.BackgroundColor
                                .SetColor(Color.LightCoral);
                        }
                    }
                }

                // =====================================================
                // VPN OVER INTERNET
                // =====================================================

                int vpnOverInternetColumn =
                    lastColumn - 2;

                worksheet.Cells[
                    branchStartRow,
                    vpnOverInternetColumn,
                    branchEndRow,
                    vpnOverInternetColumn
                ].Merge = true;

                var vpnStatusCell =
                    worksheet.Cells[
                        branchStartRow,
                        vpnOverInternetColumn
                    ];

                vpnStatusCell.Value =
                    branch.VpnOverInternetStatus == true
                        ? "OK"
                        : branch.VpnOverInternetStatus == false
                            ? "NOT OK"
                            : "Unknown";

                vpnStatusCell.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                vpnStatusCell.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                vpnStatusCell.Style.WrapText = true;

                if (branch.VpnOverInternetStatus == true)
                {
                    vpnStatusCell.Style.Fill.PatternType =
                        ExcelFillStyle.Solid;

                    vpnStatusCell.Style.Fill.BackgroundColor
                        .SetColor(Color.LimeGreen);
                }
                else if (branch.VpnOverInternetStatus == false)
                {
                    vpnStatusCell.Style.Fill.PatternType =
                        ExcelFillStyle.Solid;

                    vpnStatusCell.Style.Fill.BackgroundColor
                        .SetColor(Color.LightCoral);
                }

                // =====================================================
                // FIREWALL TYPES (comma-separated)
                // =====================================================

                int firewallTypesColumn =
                    lastColumn - 1;

                worksheet.Cells[
                    branchStartRow,
                    firewallTypesColumn,
                    branchEndRow,
                    firewallTypesColumn
                ].Merge = true;

                var firewallTypesCell =
                    worksheet.Cells[
                        branchStartRow,
                        firewallTypesColumn
                    ];

                firewallTypesCell.Value =
                    fireWallTypesByBranch.TryGetValue(branch.BranchName, out var fireWallNames)
                    && !string.IsNullOrWhiteSpace(fireWallNames)
                        ? fireWallNames
                        : "N/A";

                firewallTypesCell.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Left;

                firewallTypesCell.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                firewallTypesCell.Style.WrapText = true;

                // =====================================================
                // NOTES
                // =====================================================

                int notesColumn =
                    lastColumn;

                worksheet.Cells[
                    branchStartRow,
                    notesColumn,
                    branchEndRow,
                    notesColumn
                ].Merge = true;

                var notesCell =
                    worksheet.Cells[
                        branchStartRow,
                        notesColumn
                    ];

                notesCell.Value = branch.Notes;

                notesCell.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Left;

                notesCell.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                notesCell.Style.WrapText = true;

                // =====================================================
                // MOVE TO NEXT BRANCH
                // =====================================================

                row = branchEndRow + 1;
            }

            // =========================================================
            // GENERAL STYLE
            // =========================================================

            if (row > 3)
            {
                using var dataRange =
                    worksheet.Cells[
                        3,
                        1,
                        row - 1,
                        lastColumn
                    ];

                dataRange.Style.Border.Top.Style =
                    ExcelBorderStyle.Thin;

                dataRange.Style.Border.Bottom.Style =
                    ExcelBorderStyle.Thin;

                dataRange.Style.Border.Left.Style =
                    ExcelBorderStyle.Thin;

                dataRange.Style.Border.Right.Style =
                    ExcelBorderStyle.Thin;

                dataRange.Style.VerticalAlignment =
                    ExcelVerticalAlignment.Center;

                dataRange.Style.WrapText = true;
            }

            // =========================================================
            // COLUMN SIZING
            // =========================================================

            worksheet.Cells[
                worksheet.Dimension.Address
            ].AutoFitColumns();

            for (int c = 1; c <= lastColumn; c++)
            {
                if (worksheet.Column(c).Width > 30)
                    worksheet.Column(c).Width = 30;
            }

            worksheet.Column(1).Width = 18;

            worksheet.Row(1).Height = 30;

            worksheet.View.FreezePanes(3, 2);

            // =========================================================
            // RETURN FILE
            // =========================================================

            var fileContents =
                package.GetAsByteArray();

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VPN_Connections_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        #endregion

        private static int GetColumnIndex(
    List<string> columns,
    string columnName)
        {
            return columns.IndexOf(columnName) + 1;
        }

        private static string JoinValues(
    IEnumerable<string?> values)
        {
            return string.Join(
                Environment.NewLine,
                values.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private static string JoinStatuses(
    IEnumerable<bool?> statuses)
        {
            return string.Join(
                Environment.NewLine,
                statuses.Select(s =>
                    s switch
                    {
                        true => "Online",
                        false => "Offline",
                        null => ""
                    })
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }



}