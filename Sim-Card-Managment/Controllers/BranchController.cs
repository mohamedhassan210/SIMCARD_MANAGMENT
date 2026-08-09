using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Repos.BranchRepos;
using Sim_Card_Managment.Repos.InternetLineRepos;
using Sim_Card_Managment.Repos.VpnConnectionRepos;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class BranchController : Controller
    {
        private readonly IBranchRepo _branchRepo;
        private readonly IInternetLineRepo _internetLineRepo;   
        private readonly IVpnConnectionRepo _vpnConnectionRepo; 

        public BranchController(
            IBranchRepo branchRepo,
            IInternetLineRepo internetLineRepo,       
            IVpnConnectionRepo vpnConnectionRepo)     
        {
            _branchRepo = branchRepo;
            _internetLineRepo = internetLineRepo;     
            _vpnConnectionRepo = vpnConnectionRepo;  
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
        public IActionResult Create()
        {
            return View(new BranchCreateViewModel());
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
                return View(model);

            await _branchRepo.AddAsync(model);

            TempData["Success"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _branchRepo.GetForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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
        #region Reports

        [HttpGet]
        public async Task<IActionResult> NetworkReport(string? searchTerm)
        {
            var branches = await _branchRepo.GetAllAsync();
            var allInternetLines = await _internetLineRepo.GetAllAsync();
            var allVpnConnections = await _vpnConnectionRepo.GetAllAsync();

            var model = new NetworkReportViewModel();

            foreach (var branch in branches)
            {
                var internetLines = allInternetLines
                    .Where(il => il.BranchName == branch.Name).ToList();

                var vpnConnections = allVpnConnections
                    .Where(v => v.BranchName == branch.Name).ToList();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    internetLines = internetLines.Where(il =>
                        il.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (il.ServiceProviderName != null && il.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (il.PhoneNumber != null && il.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                    vpnConnections = vpnConnections.Where(v =>
                        v.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (v.ServiceProviderName != null && v.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (v.NID != null && v.NID.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                model.Branches.Add(new BranchNetworkItem
                {
                    BranchName = branch.Name,
                    IsActive = branch.IsActive,
                    VpnOverInternetStatus = branch.VpnOverInternetStatus,
                    InternetLines = internetLines,
                    VpnConnections = vpnConnections
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportInternetLinesToExcel(string? searchTerm)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var allLines = await _internetLineRepo.GetAllAsync();
            var lines = allLines.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                lines = lines.Where(il =>
                    il.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (il.ServiceProviderName != null && il.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (il.PhoneNumber != null && il.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            var data = lines.OrderBy(il => il.BranchName).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Internet Lines");

                worksheet.Cells[1, 1].Value = "Branch";
                worksheet.Cells[1, 2].Value = "ISP";
                worksheet.Cells[1, 3].Value = "Payment Type";
                worksheet.Cells[1, 4].Value = "Service Type";
                worksheet.Cells[1, 5].Value = "Phone Number";
                worksheet.Cells[1, 6].Value = "Bandwidth";
                worksheet.Cells[1, 7].Value = "Status";

                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.SteelBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                int row = 2;
                foreach (var line in data)
                {
                    worksheet.Cells[row, 1].Value = line.BranchName;
                    worksheet.Cells[row, 2].Value = line.ServiceProviderName;
                    worksheet.Cells[row, 3].Value = line.PaymentTypeName;
                    worksheet.Cells[row, 4].Value = line.ServiceTypeName;
                    worksheet.Cells[row, 5].Value = line.PhoneNumber ?? "N/A";
                    worksheet.Cells[row, 6].Value = line.Bandwidth ?? "N/A";
                    worksheet.Cells[row, 7].Value = line.Status ? "UP" : "DOWN";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var fileContents = package.GetAsByteArray();
                return File(fileContents,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Internet_Lines_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportVpnConnectionsToExcel(string? searchTerm)
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            var allVpns = await _vpnConnectionRepo.GetAllAsync();
            var vpns = allVpns.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                vpns = vpns.Where(v =>
                    v.BranchName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (v.ServiceProviderName != null && v.ServiceProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (v.NID != null && v.NID.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            var data = vpns.OrderBy(v => v.BranchName).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("VPN Connections");

                worksheet.Cells[1, 1].Value = "Branch";
                worksheet.Cells[1, 2].Value = "Connection Type";
                worksheet.Cells[1, 3].Value = "ISP";
                worksheet.Cells[1, 4].Value = "NID";
                worksheet.Cells[1, 5].Value = "Line Speed";
                worksheet.Cells[1, 6].Value = "Status";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkSlateGray);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                int row = 2;
                foreach (var vpn in data)
                {
                    worksheet.Cells[row, 1].Value = vpn.BranchName;
                    worksheet.Cells[row, 2].Value = vpn.ConnectionTypeName;
                    worksheet.Cells[row, 3].Value = vpn.ServiceProviderName;
                    worksheet.Cells[row, 4].Value = vpn.NID ?? "N/A";
                    worksheet.Cells[row, 5].Value = vpn.LineSpeed ?? "N/A";
                    worksheet.Cells[row, 6].Value = vpn.Status == true ? "Online"
                                                  : vpn.Status == false ? "Offline"
                                                  : "Unknown";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var fileContents = package.GetAsByteArray();
                return File(fileContents,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"VPN_Connections_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        #endregion
    }
}