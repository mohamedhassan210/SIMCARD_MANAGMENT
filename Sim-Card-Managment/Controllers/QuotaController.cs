using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Viewmodel;
using System.Drawing;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class QuotaController : Controller
    {
        private readonly IQuotaRepo _quotaRepo;
        private readonly IServiceProviderRepository _serviceProviderRepo;

        public QuotaController(IQuotaRepo quotaRepo, IServiceProviderRepository serviceProviderRepo)
        {
            _quotaRepo = quotaRepo;
            _serviceProviderRepo = serviceProviderRepo;
        }

        // GET: Quota
        public IActionResult Index()
        {
            var quotas = _quotaRepo.GetAll().Select(q => new QuotaViewModel
            {
                Id = q.Id,
                BaseAmount = q.BaseAmount,
                ExtraAmount = q.ExtraAmount,
                Fees = q.Fees,
                ServiceProviderId = q.ServiceProviderId,
                IsActive = q.IsActive,
                ServiceProviderName = q.ServiceProvider?.Name
            });

            return View(quotas);
        }

        // GET: Quota/ExportQuotasExcel?status=active|inactive|all — mirrors Index's Status Filter dropdown.
        [HttpGet]
        public IActionResult ExportQuotasExcel(string status = "all")
        {
            status = (status ?? "all").ToLower();

            var quotas = _quotaRepo.GetAll().Select(q => new QuotaViewModel
            {
                Id = q.Id,
                BaseAmount = q.BaseAmount,
                ExtraAmount = q.ExtraAmount,
                Fees = q.Fees,
                ServiceProviderId = q.ServiceProviderId,
                IsActive = q.IsActive,
                ServiceProviderName = q.ServiceProvider?.Name
            }).ToList();

            var filtered = status switch
            {
                "active" => quotas.Where(q => q.IsActive).ToList(),
                "inactive" => quotas.Where(q => !q.IsActive).ToList(),
                _ => quotas
            };

            ExcelPackage.License.SetNonCommercialPersonal("MyName");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Quotas");

            worksheet.Cells[1, 1].Value = "Service Provider";
            worksheet.Cells[1, 2].Value = "Base Amount (GB)";
            worksheet.Cells[1, 3].Value = "Extra Amount (GB)";
            worksheet.Cells[1, 4].Value = "Fees";
            worksheet.Cells[1, 5].Value = "Status";

            using (var headerRange = worksheet.Cells[1, 1, 1, 5])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                headerRange.Style.Font.Color.SetColor(Color.White);
            }

            int row = 2;
            foreach (var q in filtered)
            {
                worksheet.Cells[row, 1].Value = q.ServiceProviderName ?? "N/A";
                worksheet.Cells[row, 2].Value = q.BaseAmount;
                worksheet.Cells[row, 3].Value = q.ExtraAmount;
                worksheet.Cells[row, 4].Value = q.Fees;
                worksheet.Cells[row, 5].Value = q.IsActive ? "Active" : "Inactive";
                row++;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            var fileContents = package.GetAsByteArray();
            var suffix = status != "all" ? "_" + status : "";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Quotas{suffix}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: Quota/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new QuotaViewModel
            {
                ServiceProviders = await GetProvidersSelectListAsync()
            };
            return View(vm);
        }

        // POST: Quota/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuotaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var quota = new Quota
                {
                    BaseAmount = vm.BaseAmount,
                    ExtraAmount = vm.ExtraAmount,
                    Fees = vm.Fees,
                    ServiceProviderId = vm.ServiceProviderId,
                    IsActive = vm.IsActive
                };

                _quotaRepo.Add(quota);
                TempData["Success"] = "Quota added successfully";
                return RedirectToAction(nameof(Index));
            }

            vm.ServiceProviders = await GetProvidersSelectListAsync(vm.ServiceProviderId);
            return View(vm);
        }

        // GET: Quota/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quota = _quotaRepo.GetById(id);
            if (quota == null)
                return NotFound();

            var vm = new QuotaViewModel
            {
                Id = quota.Id,
                BaseAmount = quota.BaseAmount,
                ExtraAmount = quota.ExtraAmount,
                Fees = quota.Fees,
                ServiceProviderId = quota.ServiceProviderId,
                IsActive = quota.IsActive,
                ServiceProviders = await GetProvidersSelectListAsync(quota.ServiceProviderId)
            };

            return View(vm);
        }

        // POST: Quota/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuotaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var quota = _quotaRepo.GetById(vm.Id);
                if (quota == null)
                    return NotFound();

                quota.BaseAmount = vm.BaseAmount;
                quota.ExtraAmount = vm.ExtraAmount;
                quota.Fees = vm.Fees;
                quota.ServiceProviderId = vm.ServiceProviderId;
                quota.IsActive = vm.IsActive;

                _quotaRepo.Update(quota);
                TempData["Success"] = "Quota updated successfully";
                return RedirectToAction(nameof(Index));
            }

            vm.ServiceProviders = await GetProvidersSelectListAsync(vm.ServiceProviderId);
            return View(vm);
        }

        // GET: Quota/Details/{id}
        public IActionResult Details(int id)
        {
            var quota = _quotaRepo.GetById(id);
            if (quota == null)
                return NotFound();

            var vm = new QuotaViewModel
            {
                Id = quota.Id,
                BaseAmount = quota.BaseAmount,
                ExtraAmount = quota.ExtraAmount,
                Fees = quota.Fees,
                ServiceProviderId = quota.ServiceProviderId,
                IsActive = quota.IsActive,
                ServiceProviderName = quota.ServiceProvider?.Name
            };

            return View(vm);
        }

        private async Task<SelectList> GetProvidersSelectListAsync(int? selectedId = null)
        {
            var providers = await _serviceProviderRepo.GetAllAsync();
            return new SelectList(providers, "Id", "DisplayName", selectedId);
        }
    }
}