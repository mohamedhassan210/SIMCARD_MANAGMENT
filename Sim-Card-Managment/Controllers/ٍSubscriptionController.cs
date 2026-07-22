using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Viewmodel;
using Sim_Card_Managment.ViewModels;
using System.Security.Claims;

namespace Sim_Card_Managment.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRepo _subscriptionRepo;
        private readonly ISIMRepo _simRepo;
        private readonly IUSBRepo _usbRepo;
        private readonly IQuotaRepo _quotaRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IDeviceActionRepo _actionRepo;
        private readonly IAccountRepo _accountRepo;

        public SubscriptionController(
            ISubscriptionRepo subscriptionRepo,
            ISIMRepo simRepo,
            IUSBRepo usbRepo,
            IQuotaRepo quotaRepo,
            IEmployeeRepo employeeRepo,
            IDeviceActionRepo actionRepo,
            IAccountRepo accountRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _simRepo = simRepo;
            _usbRepo = usbRepo;
            _quotaRepo = quotaRepo;
            _employeeRepo = employeeRepo;
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
            // 1. Get the current user ID from the authentication claims cookie
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                // Fallback: If for any reason the claim is missing, fetch a default user ID from DB
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

            // 2. Fetch Action from Repo
            var createAction = _actionRepo.GetAllDeviceActions().FirstOrDefault(a => a.Name == "CreateSubscription")
                              ?? _actionRepo.GetAllDeviceActions().FirstOrDefault();

            var actionId = createAction != null ? createAction.Id : Guid.NewGuid();

            // 3. Create Subscription Entity
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                EmpId = model.SelectedEmployeeId,

                SimId = model.DeviceType == "SIM" ? model.SelectedSimId.GetValueOrDefault() : Guid.Empty,
                QuotaId = model.DeviceType == "SIM" ? model.SelectedQuotaId.GetValueOrDefault() : Guid.Empty,
                UsbId = model.DeviceType == "USB" ? model.SelectedUsbId : null,

                ActionId = actionId,

                // Set the authenticated user ID as CreatedBy
                CreatedBy = currentUserId,

                StartDate = model.StartDate,
                EndDate = model.StartDate.AddYears(model.ContractDurationYears > 0 ? model.ContractDurationYears : 1),
                CreatedDate = DateTime.Now
            };

            // 4. Save & Redirect
            _subscriptionRepo.Add(subscription);

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Search Employees
        [HttpGet]
        public async Task<IActionResult> SearchEmployees(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var employees = await _employeeRepo.SearchActiveEmployeesAsync(query);

            var result = employees.Select(e => new {
                id = e.Id,
                name = e.Name,
                details = $"National ID: {e.NationalID}"
            });

            return Json(result);
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
        public async Task<IActionResult> GetQuotasByProvider(Guid providerId)
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