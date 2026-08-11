using Microsoft.AspNetCore.Mvc;
using Sim_Card_Management.Models;
using Sim_Card_Management.Repos.DeviceSerialOperationsRepos;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class DeviceSerialOperationsController : Controller
    {
        private readonly IDeviceSerialOperationsRepo _deviceSerialOperationsRepo;
        private readonly ISIMRepo _simRepo;

        public DeviceSerialOperationsController(
            IDeviceSerialOperationsRepo deviceSerialOperationsRepo,
            ISIMRepo simRepo)
        {
            _deviceSerialOperationsRepo = deviceSerialOperationsRepo;
            _simRepo = simRepo;
        }

        /// <summary>
        /// GET: /DeviceSerialOperations/Create
        /// Displays the Device Serial Operation form with empty ViewModel
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new DeviceSerialOperationViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// GET: /api/device-serial-operation/search-sim?searchTerm=...
        /// Async API endpoint for SIM search by phone number or serial number
        /// Returns: JSON list of matching SIMs or empty list
        /// </summary>
        [HttpGet]
        [Route("api/device-serial-operation/search-sim")]
        public async Task<IActionResult> SearchSim(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(new List<object>());
            }

            var sims = await _simRepo.SearchAsync(searchTerm);

            var result = sims.Select(s => new
            {
                id = s.Id,
                phone = s.PhoneNumber,
                serial = s.SerialNumber,
                networkType = s.NetworkType,
                IsActive = s.IsActive ? "Active" : "Inactive",
                Status = s.Status,
                provider = s.ServiceProvider?.Name ?? "Unknown"
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// POST: /DeviceSerialOperations/Create
        /// Saves the Device Serial Operation: updates SIM serial and creates audit log
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceSerialOperationViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            try
            {
                // 1. Fetch SIM using ISimRepo
                var sim = await _simRepo.GetByIdAsync(model.SimId);

                if (sim == null)
                {
                    ModelState.AddModelError("SimId", "SIM card not found.");
                    return View(model);
                }

                // 2. Capture old serial for audit log
                var oldSerialNumber = sim.SerialNumber;

                // 3. Update the SIM record with new serial number
                sim.SerialNumber = model.NewSerialNumber.Trim();
                if(model.NetworkTypeChange == true)
                {
                    if(sim.NetworkType == "5G")
                    {
                        sim.NetworkType = "4G";
                    }
                    else
                    {
                        sim.NetworkType = "5G";
                    }
                }
                await _simRepo.UpdateAsync(sim);

                // 4. Create audit log entry
                var deviceSerialOperation = new DeviceSerialOperation
                {
                    SimId = model.SimId,
                    OldSerialNumber = oldSerialNumber,
                    NewSerialNumber = model.NewSerialNumber.Trim(),
                    NetworkTypeChange = model.NetworkTypeChange,
                    OperationDate = DateTime.UtcNow,
                    CreatedById = model.CreatedById
                };

                // 5. Add audit log entry using IDeviceSerialOperationsRepo
                await _deviceSerialOperationsRepo.AddAsync(deviceSerialOperation);

                TempData["SuccessMessage"] = $"SIM serial successfully updated. Old: {oldSerialNumber} → New: {model.NewSerialNumber}";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while saving the operation: {ex.Message}");
                return View(model);
            }
        }
    }
}