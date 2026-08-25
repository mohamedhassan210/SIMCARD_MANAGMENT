using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.FireWallTypeRepos;
using Sim_Card_Managment.Viewmodel;
using System.Security.Claims;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class BranchController : Controller
    {
        private readonly IBranchRepo _branchRepo;
        private readonly IFireWallTypeRepo _fireWallTypeRepo;

        public BranchController(IBranchRepo branchRepo, IFireWallTypeRepo fireWallTypeRepo)
        {
            _branchRepo = branchRepo;
            _fireWallTypeRepo = fireWallTypeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branches = await _branchRepo.GetAllAsync();
            return View(branches);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var branch = await _branchRepo.GetByIdWithDetailsAsync(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new BranchCreateViewModel
            {
                FireWallTypes = await GetFireWallTypeSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchCreateViewModel model)
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
            {
                model.CreatedById = currentUserId;
            }
            else
            {
                model.CreatedById = 1; // Default fallback ID if unauthenticated in dev
            }

            if (!ModelState.IsValid)
            {
                model.FireWallTypes = await GetFireWallTypeSelectListAsync();
                return View(model);
            }

            await _branchRepo.AddAsync(model);
            TempData["Success"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _branchRepo.GetForEditAsync(id);
            if (model == null) return NotFound();

            model.FireWallTypes = await GetFireWallTypeSelectListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.FireWallTypes = await GetFireWallTypeSelectListAsync();
                return View(model);
            }

            await _branchRepo.UpdateAsync(model);
            TempData["Success"] = "Branch updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _branchRepo.SoftDeleteAsync(id);
            TempData["Success"] = "Branch deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            await _branchRepo.ActivateAsync(id);
            TempData["Success"] = "Branch activated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IEnumerable<SelectListItem>> GetFireWallTypeSelectListAsync()
        {
            var types = await _fireWallTypeRepo.GetAllAsync();
            return types.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            });
        }
        // GET: /Branch/ExportBranchesExcel — mirrors the Index page's Status dropdown only
        [HttpGet]
        public async Task<IActionResult> ExportBranchesExcel(string status = "all")
        {
            status = (status ?? "all").ToLower();
            bool? isActive = status switch
            {
                "active" => true,
                "inactive" => false,
                _ => null
            };

            var branches = await _branchRepo.GetAllAsync(isActive);

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Branches");

            worksheet.Cells[1, 1].Value = "Branch Name";
            worksheet.Cells[1, 2].Value = "Site Code";
            worksheet.Cells[1, 3].Value = "Branch Code";
            worksheet.Cells[1, 4].Value = "VPN Over Internet";
            worksheet.Cells[1, 5].Value = "Is Active";
            worksheet.Cells[1, 6].Value = "ISP Connections";
            worksheet.Cells[1, 7].Value = "VPN Lines";
            worksheet.Cells[1, 8].Value = "Created At";
            worksheet.Cells[1, 9].Value = "Created By";

            using (var headerRange = worksheet.Cells[1, 1, 1, 9])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var b in branches)
            {
                worksheet.Cells[row, 1].Value = b.Name;
                worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(b.SiteCode) ? "N/A" : b.SiteCode;
                worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(b.BranchCode) ? "N/A" : b.BranchCode;
                worksheet.Cells[row, 4].Value = b.VpnOverInternetStatus == true ? "Enabled"
                    : b.VpnOverInternetStatus == false ? "Disabled" : "Not Configured";
                worksheet.Cells[row, 5].Value = b.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 6].Value = b.InternetLineCount;
                worksheet.Cells[row, 7].Value = b.VpnConnectionCount;
                worksheet.Cells[row, 8].Value = b.CreatedAt.ToString("MMM dd, yyyy");
                worksheet.Cells[row, 9].Value = b.CreatedByUsername;
                row++;
            }

            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + (isActive == true ? "Active" : "Inactive") : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Branches{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: /Branch/ExportBranchIspExcel — ISP Connections for one branch, filtered by Up/Down
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ExportBranchIspExcel(int id, string status = "all")
        {
            var branch = await _branchRepo.GetByIdWithDetailsAsync(id);
            if (branch == null) return NotFound();

            status = (status ?? "all").ToLower();
            var lines = branch.InternetLines.AsEnumerable();
            if (status == "up") lines = lines.Where(l => l.Status);
            else if (status == "down") lines = lines.Where(l => !l.Status);
            var filtered = lines.ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("ISP Connections");

            string[] headers = {
        "ISP", "Payment", "Service Type", "SIM SN",
        "Phone Number", "Renewal Date", "Quota", "Bandwidth", "Status", "Notes"
    };
            int lastColumn = headers.Length;

            // Title
            worksheet.Cells[1, 1, 1, lastColumn].Merge = true;
            worksheet.Cells[1, 1].Value = $"ISP Connections - {branch.Name}";
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

            // Data — one row per line
            int row = 3;
            foreach (var l in filtered)
            {
                worksheet.Cells[row, 1].Value = l.ServiceProviderName;
                worksheet.Cells[row, 2].Value = l.PaymentTypeName;
                worksheet.Cells[row, 3].Value = l.ServiceTypeName;
                worksheet.Cells[row, 4].Value = string.IsNullOrWhiteSpace(l.SimSerialNumber) ? "N/A" : l.SimSerialNumber;
                worksheet.Cells[row, 5].Value = string.IsNullOrWhiteSpace(l.PhoneNumber) ? "N/A" : l.PhoneNumber;
                worksheet.Cells[row, 6].Value = l.NextRenewalDate.HasValue
                    ? l.NextRenewalDate.Value.ToString("dd MMM yyyy")
                    : "N/A";
                worksheet.Cells[row, 7].Value = l.QuotaGB.HasValue ? $"{l.QuotaGB} GB" : "N/A";
                worksheet.Cells[row, 8].Value = string.IsNullOrWhiteSpace(l.Bandwidth) ? "N/A" : l.Bandwidth;
                worksheet.Cells[row, 9].Value = l.Status ? "UP" : "DOWN";
                worksheet.Cells[row, 10].Value = string.IsNullOrWhiteSpace(l.Notes) ? "" : l.Notes;
                row++;
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

                const int statusColumn = 9;
                for (int r = 3; r < row; r++)
                {
                    var statusCell = worksheet.Cells[r, statusColumn];
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var statusText = statusCell.Value?.ToString() ?? "";
                    if (statusText == "UP")
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                    else if (statusText == "DOWN")
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            for (int c = 1; c <= lastColumn; c++)
            {
                if (worksheet.Column(c).Width > 30)
                    worksheet.Column(c).Width = 30;
            }
            worksheet.Row(1).Height = 30;
            worksheet.View.FreezePanes(3, 1);

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + (status == "up" ? "Up" : "Down") : "";
            var safeBranchName = string.Join("_", branch.Name.Split(System.IO.Path.GetInvalidFileNameChars()));

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{safeBranchName}_ISP{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: /Branch/ExportBranchVpnExcel — VPN Lines for one branch, filtered by Up/Down
        [HttpGet]
        public async Task<IActionResult> ExportBranchVpnExcel(int id, string status = "all")
        {
            var branch = await _branchRepo.GetByIdWithDetailsAsync(id);
            if (branch == null) return NotFound();

            status = (status ?? "all").ToLower();
            var vpns = branch.VpnConnections.AsEnumerable();
            if (status == "up") vpns = vpns.Where(v => v.Status == true);
            else if (status == "down") vpns = vpns.Where(v => v.Status == false);
            var filtered = vpns.ToList();

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("VPN Lines");

            worksheet.Cells[1, 1].Value = "Connection Type";
            worksheet.Cells[1, 2].Value = "Service Provider";
            worksheet.Cells[1, 3].Value = "NID";
            worksheet.Cells[1, 4].Value = "Line Speed";
            worksheet.Cells[1, 5].Value = "Status";

            using (var headerRange = worksheet.Cells[1, 1, 1, 5])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var v in filtered)
            {
                worksheet.Cells[row, 1].Value = v.ConnectionTypeName;
                worksheet.Cells[row, 2].Value = v.ServiceProviderName;
                worksheet.Cells[row, 3].Value = v.NID ?? "N/A";
                worksheet.Cells[row, 4].Value = v.LineSpeed ?? "N/A";
                worksheet.Cells[row, 5].Value = v.Status == true ? "Up" : v.Status == false ? "Down" : "Unknown";
                row++;
            }

            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + (status == "up" ? "Up" : "Down") : "";
            var safeBranchName = string.Join("_", branch.Name.Split(System.IO.Path.GetInvalidFileNameChars()));

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{safeBranchName}_VPN{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }
    }
}