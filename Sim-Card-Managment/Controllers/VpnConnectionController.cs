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
        public async Task<IActionResult> ExportToExcel(string? searchTerm, string status = "all")
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var data = await _vpnRepo.GetForExcelAsync();
            var fireWallTypesByBranch = await _branchRepo.GetFireWallTypeNamesByBranchNameAsync();

            status = (status ?? "all").ToLower();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                data = data
                    .Where(b =>
                        b.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Connections.Any(v =>
                            v.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            v.NID?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                            v.ConnectionTypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToList();
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vpn");

            string[] headers = {
        "Branch", "Connection Type", "ISP", "NID", "Line Speed",
        "Status", "VPN Over Internet", "Firewall Types"
    };
            int lastColumn = headers.Length;

            // Title
            worksheet.Cells[1, 1, 1, lastColumn].Merge = true;
            worksheet.Cells[1, 1].Value = "VPN - Leased Line";
            var titleRange = worksheet.Cells[1, 1, 1, lastColumn];
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Header
            int headerRow = 2;
            for (int i = 0; i < headers.Length; i++)
                worksheet.Cells[headerRow, i + 1].Value = headers[i];

            using (var headerRange = worksheet.Cells[headerRow, 1, headerRow, lastColumn])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Data — one row per connection
            int row = 3;
            foreach (var branch in data.OrderBy(x => x.BranchName))
            {
                var connections = branch.Connections.AsEnumerable();
                if (status == "up") connections = connections.Where(c => c.Status == true);
                else if (status == "down") connections = connections.Where(c => c.Status == false);
                var filteredConnections = connections.ToList();

                if (!filteredConnections.Any())
                    continue; // branch has no matching connections under this filter — skip it

                var firewallText = fireWallTypesByBranch.TryGetValue(branch.BranchName, out var fw) && !string.IsNullOrWhiteSpace(fw)
                    ? fw : "N/A";
                var vpnOverInternetText = branch.VpnOverInternetStatus == true ? "OK"
                    : branch.VpnOverInternetStatus == false ? "NOT OK" : "Unknown";

                foreach (var c in filteredConnections)
                {
                    worksheet.Cells[row, 1].Value = branch.BranchName;
                    worksheet.Cells[row, 2].Value = c.ConnectionTypeName;
                    worksheet.Cells[row, 3].Value = c.ServiceProviderName;
                    worksheet.Cells[row, 4].Value = string.IsNullOrWhiteSpace(c.NID) ? "N/A" : c.NID;
                    worksheet.Cells[row, 5].Value = string.IsNullOrWhiteSpace(c.LineSpeed) ? "N/A" : c.LineSpeed;

                    var statusCell = worksheet.Cells[row, 6];
                    statusCell.Value = c.Status == true ? "Online" : c.Status == false ? "Offline" : "Unknown";
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    if (c.Status == true)
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                    else if (c.Status == false)
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    worksheet.Cells[row, 7].Value = vpnOverInternetText;
                    worksheet.Cells[row, 8].Value = firewallText;
                    row++;
                }
            }

            if (row > 3)
            {
                using var dataRange = worksheet.Cells[3, 1, row - 1, lastColumn];
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                dataRange.Style.WrapText = true;
            }

            worksheet.Column(1).Width = 18;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            for (int c = 1; c <= lastColumn; c++)
            {
                if (worksheet.Column(c).Width > 30)
                    worksheet.Column(c).Width = 30;
            }
            worksheet.Row(1).Height = 30;
            worksheet.View.FreezePanes(3, 1);

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + (status == "up" ? "Online" : "Offline") : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VPN_Connections{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
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