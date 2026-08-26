using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Adjust these to match the exact namespace of your interfaces:
using Sim_Card_Managment.Repos;

namespace Sim_Card_Management.Controllers
{
    [RequirePermission]
    public class ReportController : Controller
    {
        private readonly ISubscriptionRepo _subscriptionRepo;

        public ReportController(ISubscriptionRepo subscriptionRepo)
        {
            _subscriptionRepo = subscriptionRepo;

            ExcelPackage.License.SetNonCommercialPersonal("Joo");
        }

        public async Task<IActionResult> Index()
        {
            // Fetch subscriptions with related details (Sim, Usb, Employee, Quota)
            var allSubscriptions = await _subscriptionRepo.GetAllSubscriptionsWithDetailsAsync();

            // Take 2 subscriptions to display in the section preview
            var previewSubscriptions = allSubscriptions.Take(2).ToList();

            return View(previewSubscriptions);
        }

        #region --- Subscriptions Excel Report ---

        // Download Excel Report — status: "Active" | "Expired" | null/"ALL" for everything,
        // plus an optional date range on StartDate.
        // Mirrors Subscription/Index.cshtml's Status filter dropdown.
        public async Task<IActionResult> ExportSubscriptionsExcel(string? status, DateTime? from, DateTime? to)
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes(status, from, to);

            var fileNameParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
                fileNameParts.Add(status);
            if (from.HasValue || to.HasValue)
            {
                var fromPart = from.HasValue ? from.Value.ToString("yyyyMMdd") : "Start";
                var toPart = to.HasValue ? to.Value.ToString("yyyyMMdd") : "Now";
                fileNameParts.Add($"{fromPart}-{toPart}");
            }

            var suffix = fileNameParts.Any() ? "_" + string.Join("_", fileNameParts) : "";

            string fileName = $"Subscriptions_Report{suffix}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // View Online (Inline Content-Disposition)
        public async Task<IActionResult> ViewSubscriptionsOnline(string? status, DateTime? from, DateTime? to)
        {
            byte[] fileContents = await GenerateSubscriptionsExcelBytes(status, from, to);
            Response.Headers.Add("Content-Disposition", "inline; filename=Subscriptions_Report.xlsx");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private async Task<byte[]> GenerateSubscriptionsExcelBytes(string? status, DateTime? from, DateTime? to)
        {
            var subscriptions = await _subscriptionRepo.GetAllSubscriptionsWithDetailsAsync(); // Ensure your repo loads Sim, Usb, Employee, Quota

            var query = subscriptions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                bool wantActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(sub =>
                {
                    bool isActive = sub.EndDate == null || sub.EndDate > DateTime.Now;
                    return isActive == wantActive;
                });
            }

            if (from.HasValue)
            {
                query = query.Where(sub => sub.StartDate.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                // Inclusive of the whole "to" day.
                query = query.Where(sub => sub.StartDate.Date <= to.Value.Date);
            }

            var filteredSubscriptions = query.ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Subscriptions");

            // Define Headers — separate SIM/USB columns since a subscription can have
            // both at once, and a real Fees column (was missing entirely before).
            string[] headers = { "Subscriber", "SIM Number", "USB Serial", "Quota (GB)", "Monthly Fees", "Status", "Start Date" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(229, 80, 38)); // Brand Orange
                cell.Style.Font.Color.SetColor(Color.White);
            }

            // Fill Data
            int row = 2;
            foreach (var sub in filteredSubscriptions)
            {
                string subscriberName = sub.Employee?.Name ?? sub.NonEmployee?.Name ?? "Unassigned";
                string simNumber = sub.Sim?.PhoneNumber ?? "N/A";
                string usbSerial = sub.Usb?.SerialNumber ?? "N/A";

                decimal? quotaGb = sub.Quota != null
                    ? sub.Quota.BaseAmount + sub.Quota.ExtraAmount
                    : (decimal?)null;

                decimal fees = sub.Fees ?? 0;
                bool isActive = sub.EndDate == null || sub.EndDate > DateTime.Now;

                worksheet.Cells[row, 1].Value = subscriberName;
                worksheet.Cells[row, 2].Value = simNumber;
                worksheet.Cells[row, 3].Value = usbSerial;
                worksheet.Cells[row, 4].Value = quotaGb.HasValue ? (object)quotaGb.Value : "N/A";
                worksheet.Cells[row, 5].Value = fees;
                worksheet.Cells[row, 6].Value = isActive ? "Active" : "Expired";
                worksheet.Cells[row, 7].Value = sub.StartDate.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        #endregion
    }
}