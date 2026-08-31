using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.ViewModels.Subscription;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepo _subscriptionRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly IQuotaRepo _quotaRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly INonEmployeeRepo _nonEmployeeRepo;
        private readonly IDeviceActionRepo _actionRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly IDeviceStatusRepo _deviceStatusRepo;
        private readonly AppDbContext _context;

        public SubscriptionController(
            ISubscriptionRepo subscriptionRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            IQuotaRepo quotaRepo,
            IEmployeeRepo employeeRepo,
            INonEmployeeRepo nonEmployeeRepo,
            IDeviceActionRepo actionRepo,
            IAccountRepo accountRepo,
            IDeviceStatusRepo deviceStatusRepo,
            AppDbContext context)
        {
            _subscriptionRepo = subscriptionRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _quotaRepo = quotaRepo;
            _employeeRepo = employeeRepo;
            _nonEmployeeRepo = nonEmployeeRepo;
            _actionRepo = actionRepo;
            _accountRepo = accountRepo;
            _deviceStatusRepo = deviceStatusRepo;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var subscriptions = _subscriptionRepo.GetAll()
                .OrderByDescending(s => s.CreatedDate);
            var now = DateTime.Now;

            var model = subscriptions.Select(s => new SubscriptionIndexVM
            {
                Id = s.Id,
                SubscriberName = s.Employee?.Name ?? s.NonEmployee?.Name ?? "Unassigned",
                SubscriberType = s.EmpId.HasValue ? "Employee" : "Non-Employee",
                SubscriberIdentifier = s.Employee?.EmpCode ?? s.NonEmployee?.ContactInfo,

                HasSim = s.Sim != null,
                SimSerialNumber = s.Sim?.SerialNumber,
                HasUsb = s.Usb != null,
                UsbSerialNumber = s.Usb?.SerialNumber,

                QuotaName = s.Quota != null ? $"{s.Quota.BaseAmount} GB" : "N/A",
                Fees = s.Fees ?? 0,

                StartDate = s.StartDate,
                EndDate = s.EndDate,

                CreatedByUserName = s.CreatedByUser?.Username ?? "N/A"
            }).ToList();

            ViewBag.SubscriptionCount = model.Count(x => x.Status == "Active");
            ViewBag.ExpiringCount = model.Count(x =>
                x.Status == "Active" && x.EndDate.HasValue && x.EndDate.Value <= now.AddDays(30));

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new SubscriptionCreateViewModel
            {
                StartDate = DateTime.Today,
                ContractDurationYears = 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubscriptionCreateViewModel model)
        {
            bool hasSim = model.SelectedSimId.HasValue && model.SelectedSimId.Value > 0;
            bool hasUsb = model.SelectedUsbId.HasValue && model.SelectedUsbId.Value > 0;

            if (!hasSim && !hasUsb)
            {
                ModelState.AddModelError("", "Please select at least a SIM card or a USB device.");
            }

            if (!model.SelectedEmployeeId.HasValue || model.SelectedEmployeeId.Value == 0)
            {
                ModelState.AddModelError("SelectedEmployeeId", "A valid recipient must be selected.");
            }

            if (hasSim && (!model.SelectedQuotaId.HasValue || model.SelectedQuotaId.Value == 0))
            {
                ModelState.AddModelError("SelectedQuotaId", "A Quota must be selected when a SIM card is chosen.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int currentUserId = GetCurrentUserId();

            var createAction = _actionRepo.GetAllDeviceActions().FirstOrDefault(a => a.Name == "CreateSubscription")
                          ?? _actionRepo.GetAllDeviceActions().FirstOrDefault();
            var actionId = createAction != null ? createAction.Id : 0;

            decimal appliedFees = model.Fees;
            if (hasSim && model.Fees <= 0)
            {
                var quota = _quotaRepo.GetById(model.SelectedQuotaId!.Value);
                appliedFees = quota?.Fees ?? 0;
            }

            var subscription = new Subscription
            {
                EmpId = !model.IsNonEmployee ? model.SelectedEmployeeId : null,
                NonEmployeeId = model.IsNonEmployee ? model.SelectedEmployeeId : null,

                SimId = hasSim ? model.SelectedSimId : null,
                QuotaId = hasSim ? model.SelectedQuotaId : null,
                UsbId = hasUsb ? model.SelectedUsbId : null,

                ActionId = actionId,
                CreatedBy = currentUserId,
                StartDate = model.StartDate,
                EndDate = model.StartDate.AddYears(model.ContractDurationYears > 0 ? model.ContractDurationYears : 1),
                CreatedDate = DateTime.Now,
                Fees = appliedFees
            };

            _subscriptionRepo.Add(subscription);

            // Resolve the assignee's name once, since both device types (if both are set)
            // were just handed to the same subscriber
            string? assignedToName = !model.IsNonEmployee
                ? _employeeRepo.GetById(model.SelectedEmployeeId!.Value)?.Name
                : _nonEmployeeRepo.GetById(model.SelectedEmployeeId!.Value)?.Name;

            if (hasSim)
            {
                var sim = _simRepo.GetById(model.SelectedSimId!.Value);
                if (sim != null) { sim.Status = "Occupied"; _simRepo.Update(sim); }
                LogDeviceStatus(model.SelectedSimId, null, "Occupied", "Assigned via new subscription", currentUserId, assignedToName);
            }
            if (hasUsb)
            {
                var usb = _usbRepo.GetById(model.SelectedUsbId!.Value);
                if (usb != null) { usb.Status = "Occupied"; _usbRepo.Update(usb); }
                LogDeviceStatus(null, model.SelectedUsbId, "Occupied", "Assigned via new subscription", currentUserId, assignedToName);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var sub = _subscriptionRepo.GetById(id);
            if (sub == null) return NotFound();

            var vm = new SubscriptionDetailsVM
            {
                Id = sub.Id,
                SubscriberName = sub.Employee?.Name ?? sub.NonEmployee?.Name ?? "Unassigned",
                SubscriberType = sub.EmpId.HasValue ? "Employee" : "Non-Employee",
                EmpId = sub.EmpId,
                NonEmployeeId = sub.NonEmployeeId,

                SimId = sub.SimId,
                SimNumber = sub.Sim?.SerialNumber,
                SimPhoneNumber = sub.Sim?.PhoneNumber,
                SimNetworkType = sub.Sim?.NetworkType,
                SimProviderName = sub.Sim?.ServiceProvider?.Name,
                SimIsActive = sub.Sim?.IsActive ?? false,

                UsbId = sub.UsbId,
                UsbSerialNumber = sub.Usb?.SerialNumber,
                UsbModel = sub.Usb?.Model,
                UsbProviderName = sub.Usb?.ServiceProvider?.Name,
                UsbIsActive = sub.Usb?.IsActive ?? false,

                QuotaName = sub.Quota != null ? $"{sub.Quota.BaseAmount} GB" : "N/A",
                QuotaId = sub.QuotaId,
                Fees = sub.Fees ?? 0,

                StartDate = sub.StartDate,
                EndDate = sub.EndDate,

                CreatedDate = sub.CreatedDate,
                CreatedByUserName = sub.CreatedByUser?.Username ?? "N/A",

                Notes = sub.Notes
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sub = _subscriptionRepo.GetById(id);
            if (sub == null) return NotFound();

            var vm = new SubscriptionEditViewModel
            {
                Id = sub.Id,
                SubscriberName = sub.Employee?.Name ?? sub.NonEmployee?.Name ?? "Unassigned",
                SubscriberType = sub.EmpId.HasValue ? "Employee" : "Non-Employee",

                SelectedSimId = sub.SimId,
                CurrentSimSerial = sub.Sim?.SerialNumber,
                CurrentSimProviderId = sub.Sim?.ServiceProviderId,
                CurrentSimProviderName = sub.Sim?.ServiceProvider?.Name,
                CurrentSimProviderLogoPath = sub.Sim?.ServiceProvider?.LogoPath,
                CurrentSimNetworkType = sub.Sim?.NetworkType,
                CurrentSimPhoneNumber = sub.Sim?.PhoneNumber,

                SelectedUsbId = sub.UsbId,
                CurrentUsbSerial = sub.Usb?.SerialNumber,
                CurrentUsbModel = sub.Usb?.Model,
                CurrentUsbProviderName = sub.Usb?.ServiceProvider?.Name,
                CurrentUsbProviderLogoPath = sub.Usb?.ServiceProvider?.LogoPath,

                SelectedQuotaId = sub.QuotaId,
                CurrentQuotaDisplay = sub.Quota != null ? $"{sub.Quota.BaseAmount} GB" : null,
                CurrentQuotaBaseAmount = sub.Quota?.BaseAmount,
                CurrentQuotaExtraAmount = sub.Quota?.ExtraAmount,
                CurrentQuotaFee = sub.Quota?.Fees,

                Fees = sub.Fees ?? 0,
                OriginalFees = sub.Fees ?? 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SubscriptionEditViewModel model)
        {
            bool hasSim = model.SelectedSimId.HasValue && model.SelectedSimId.Value > 0;
            bool hasUsb = model.SelectedUsbId.HasValue && model.SelectedUsbId.Value > 0;

            if (!hasSim && !hasUsb)
            {
                ModelState.AddModelError("", "Please select at least a SIM card or a USB device.");
            }

            if (hasSim && (!model.SelectedQuotaId.HasValue || model.SelectedQuotaId.Value == 0))
            {
                ModelState.AddModelError("SelectedQuotaId", "A Quota must be selected when a SIM card is chosen.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sub = _subscriptionRepo.GetById(model.Id);
            if (sub == null) return NotFound();

            int? oldSimId = sub.SimId;
            int? oldUsbId = sub.UsbId;

            sub.SimId = hasSim ? model.SelectedSimId : null;
            sub.UsbId = hasUsb ? model.SelectedUsbId : null;
            sub.QuotaId = hasSim ? model.SelectedQuotaId : null;
            sub.Fees = model.Fees;

            _subscriptionRepo.Update(sub);

            int currentUserId = GetCurrentUserId();
            string? assignedToName = sub.Employee?.Name ?? sub.NonEmployee?.Name;
            ApplyDeviceStatusChangeOnEdit(oldSimId, sub.SimId, oldUsbId, sub.UsbId, currentUserId, assignedToName);
            return RedirectToAction(nameof(Details), new { id = sub.Id });
        }

        private void ApplyDeviceStatusChangeOnEdit(int? oldSimId, int? newSimId, int? oldUsbId, int? newUsbId, int reportedBy, string? assignedToName)
        {
            if (oldSimId != newSimId)
            {
                if (oldSimId.HasValue)
                {
                    var oldSim = _simRepo.GetById(oldSimId.Value);
                    if (oldSim != null) { oldSim.Status = "Unassigned"; _simRepo.Update(oldSim); }
                    LogDeviceStatus(oldSimId, null, "Unassigned", "Removed from subscription during edit", reportedBy);
                }
                if (newSimId.HasValue)
                {
                    var newSim = _simRepo.GetById(newSimId.Value);
                    if (newSim != null) { newSim.Status = "Occupied"; _simRepo.Update(newSim); }
                    LogDeviceStatus(newSimId, null, "Occupied", "Assigned to subscription during edit", reportedBy, assignedToName);
                }
            }

            if (oldUsbId != newUsbId)
            {
                if (oldUsbId.HasValue)
                {
                    var oldUsb = _usbRepo.GetById(oldUsbId.Value);
                    if (oldUsb != null) { oldUsb.Status = "Unassigned"; _usbRepo.Update(oldUsb); }
                    LogDeviceStatus(null, oldUsbId, "Unassigned", "Removed from subscription during edit", reportedBy);
                }
                if (newUsbId.HasValue)
                {
                    var newUsb = _usbRepo.GetById(newUsbId.Value);
                    if (newUsb != null) { newUsb.Status = "Occupied"; _usbRepo.Update(newUsb); }
                    LogDeviceStatus(null, newUsbId, "Occupied", "Assigned to subscription during edit", reportedBy, assignedToName);
                }
            }
        }

        /// <summary>
        /// Writes a DeviceStatus history record for a SIM or USB device — mirrors the
        /// pattern DeviceTransferController already uses for transfer-driven status changes,
        /// so subscription-driven status changes (create/edit) show up in the same log.
        /// </summary>
        private void LogDeviceStatus(int? simId, int? usbId, string statusName, string notes, int reportedBy, string? assignedToName = null)
        {
            if (!simId.HasValue && !usbId.HasValue) return;

            var statusType = _context.DeviceStatusesType.FirstOrDefault(t => t.Name == statusName);
            if (statusType == null) return;

            var deviceStatus = new DeviceStatus
            {
                SimId = simId,
                UsbId = usbId,
                StatusTypeId = statusType.Id,
                StatusDate = DateTime.Now,
                Notes = notes,
                ReportedBy = reportedBy,
                AssignedToName = assignedToName
            };

            // AddDeviceStatus already calls SaveChanges() internally
            _deviceStatusRepo.AddDeviceStatus(deviceStatus);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            var defaultUser = _accountRepo.GetAllUsersAsync(null, null, true).Result.FirstOrDefault();
            return defaultUser?.Id ?? 1;
        }

        [HttpGet]
        public async Task<IActionResult> SearchRecipients(string query, bool isNonEmployee)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            if (isNonEmployee)
            {
                var nonEmployees = await _nonEmployeeRepo.SearchNonEmployeesAsync(query);

                var result = nonEmployees.Select(n => new {
                    id = n.Id,
                    name = n.Name,
                    details = $"Non-Employee | Contact: {n.ContactInfo ?? "N/A"}"
                });

                return Json(result);
            }
            else
            {
                var employees = await _employeeRepo.SearchActiveEmployeesAsync(query);

                var result = employees.Select(e => new {
                    id = e.Id,
                    name = e.Name,
                    details = $"National ID: {e.NationalID}"
                });

                return Json(result);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchSims(string query)
        {
            var sims = await _simRepo.GetAssignableSimsAsync(query);

            var result = sims.Select(s => new {
                id = s.Id,
                phoneNumber = s.PhoneNumber,
                serialNumber = s.SerialNumber,
                networkType = s.NetworkType,
                providerName = s.ServiceProvider?.Name ?? "Unknown",
                providerId = s.ServiceProviderId,
                providerLogoPath = s.ServiceProvider?.LogoPath
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsbs(string query)
        {
            var usbs = await _usbRepo.GetAssignableUsbsAsync(query);

            var result = usbs.Select(u => new {
                id = u.Id,
                model = u.Model,
                serialNumber = u.SerialNumber,
                providerName = u.ServiceProvider?.Name ?? "Unknown",
                providerLogoPath = u.ServiceProvider?.LogoPath
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotasByProvider(int providerId)
        {
            var quotas = await _quotaRepo.GetQuotasByProviderIdAsync(providerId);

            var result = quotas.Select(q => new {
                id = q.Id,
                baseAmount = q.BaseAmount,
                extraAmount = q.ExtraAmount,
                fees = q.Fees
            });

            return Json(result);
        }
    }
}