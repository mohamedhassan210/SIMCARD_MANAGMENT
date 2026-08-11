using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;
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
        private readonly INonEmployeeRepo _nonEmployeeRepo; // Added Non-Employee Repo
        private readonly IDeviceActionRepo _actionRepo;
        private readonly IAccountRepo _accountRepo;

        public SubscriptionController(
            ISubscriptionRepo subscriptionRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            IQuotaRepo quotaRepo,
            IEmployeeRepo employeeRepo,
            INonEmployeeRepo nonEmployeeRepo, // Inject INonEmployeeRepo
            IDeviceActionRepo actionRepo,
            IAccountRepo accountRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _quotaRepo = quotaRepo;
            _employeeRepo = employeeRepo;
            _nonEmployeeRepo = nonEmployeeRepo;
            _actionRepo = actionRepo;
            _accountRepo = accountRepo;
        }

        // GET: Subscription/Index
        [HttpGet]
        public IActionResult Index()
        {
            var subscriptions = _subscriptionRepo.GetAll();
            ViewBag.SubscriptionCount = subscriptions.Count();
            return View(subscriptions);
        }

        // GET: Subscription/Create
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
            // 1. Validation: Enforce SIM selection
            if (!model.SelectedSimId.HasValue || model.SelectedSimId.Value == 0)
            {
                ModelState.AddModelError("SelectedSimId", "A SIM card is required to create a subscription.");
            }

            // 2. Validation: Enforce Recipient selection
            if (!model.SelectedEmployeeId.HasValue || model.SelectedEmployeeId.Value == 0)
            {
                ModelState.AddModelError("SelectedEmployeeId", "A valid recipient must be selected.");
            }

            // 3. Validation: Enforce Quota selection
            if (!model.SelectedQuotaId.HasValue || model.SelectedQuotaId.Value == 0)
            {
                ModelState.AddModelError("SelectedQuotaId", "A Quota must be selected.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 4. Authentication Check
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                var defaultUser = _accountRepo.GetAllUsersAsync(null, null, true).Result.FirstOrDefault();
                if (defaultUser != null)
                {
                    currentUserId = defaultUser.Id;
                }
                else
                {
                    ModelState.AddModelError("", "User authentication error. Please log in again.");
                    return View(model);
                }
            }

            // 5. Fetch Action
            var createAction = _actionRepo.GetAllDeviceActions().FirstOrDefault(a => a.Name == "CreateSubscription")
                          ?? _actionRepo.GetAllDeviceActions().FirstOrDefault();
            var actionId = createAction != null ? createAction.Id : 0;

            // 6. Create Subscription Entity
            var subscription = new Subscription
            {
                EmpId = !model.IsNonEmployee ? model.SelectedEmployeeId : null,
                NonEmployeeId = model.IsNonEmployee ? model.SelectedEmployeeId : null,

                SimId = model.SelectedSimId.Value,
                QuotaId = model.SelectedQuotaId.Value, // <-- Set QuotaId here
                UsbId = model.SelectedUsbId,

                ActionId = actionId,
                CreatedBy = currentUserId,
                StartDate = model.StartDate,
                EndDate = model.StartDate.AddYears(model.ContractDurationYears > 0 ? model.ContractDurationYears : 1),
                CreatedDate = DateTime.Now,
                Fees = model.Fees
            };

            // 7. Save Entity
            _subscriptionRepo.Add(subscription);

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Search Recipients (Handles both Employee & Non-Employee queries)
        [HttpGet]
        public async Task<IActionResult> SearchRecipients(string query, bool isNonEmployee)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            if (isNonEmployee)
            {
                // Query Non-Employee Repository
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
                // Query Employee Repository
                var employees = await _employeeRepo.SearchActiveEmployeesAsync(query);

                var result = employees.Select(e => new {
                    id = e.Id,
                    name = e.Name,
                    details = $"National ID: {e.NationalID}"
                });

                return Json(result);
            }
        }

        // AJAX: Search SIMs
        [HttpGet]
        public async Task<IActionResult> SearchSims(string query)
        {
            var sims = await _simRepo.GetAvailableSimsAsync(query);

            var result = sims.Select(s => new {
                id = s.Id,
                phoneNumber = s.PhoneNumber,
                serialNumber = s.SerialNumber,
                networkType = s.NetworkType,
                providerName = s.ServiceProvider?.Name ?? "Unknown",
                providerId = s.ServiceProviderId
            });

            return Json(result);
        }

        // AJAX: Search USBs
        [HttpGet]
        public async Task<IActionResult> SearchUsbs(string query)
        {
            var usbs = await _usbRepo.GetAvailableUsbsAsync(query);

            var result = usbs.Select(u => new {
                id = u.Id,
                model = u.Model,
                serialNumber = u.SerialNumber,
                providerName = u.ServiceProvider?.Name ?? "Unknown"
            });

            return Json(result);
        }

        // AJAX: Get Quotas by Provider
        [HttpGet]
        public async Task<IActionResult> GetQuotasByProvider(int providerId)
        {
            var quotas = await _quotaRepo.GetQuotasByProviderIdAsync(providerId);

            var result = quotas.Select(q => new {
                id = q.Id,
                baseAmount = q.BaseAmount,
                extraAmount = q.ExtraAmount
            });

            return Json(result);
        }
    }
}