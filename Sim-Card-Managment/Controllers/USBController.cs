using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using System;
using System.Linq;
namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class USBController : Controller
    {
        private readonly IUSBRepo _usbRepo;
        private readonly AppDbContext _context;
        public USBController(IUSBRepo usbRepo, AppDbContext context)
        {
            _usbRepo = usbRepo;
            _context = context;
        }
        // GET: /USB
        public IActionResult Index()
        {
            var usbs = _usbRepo.GetAll();
            return View(usbs);
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var usb = _usbRepo.GetById(id);
            if (usb == null)
            {
                return NotFound();
            }
            return View(usb);
        }
        [HttpGet]
        public IActionResult Create()
        {
            PopulateServiceProvidersDropDownList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usb usb)
        {
            ModelState.Remove(nameof(Usb.ServiceProvider));
            if (ModelState.IsValid)
            {
                usb.RegisteredAt = DateTime.Now;
                usb.IsActive = true;
                _usbRepo.Add(usb);
                return RedirectToAction("Index", "SIM", new { type = "usb" });
            }
            PopulateServiceProvidersDropDownList(usb.ServiceProviderId);
            return View(usb);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var usb = _usbRepo.GetById(id);
            if (usb == null)
            {
                return NotFound();
            }
            PopulateServiceProvidersDropDownList(usb.ServiceProviderId);
            return View(usb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Usb usb)
        {
            ModelState.Remove(nameof(Usb.ServiceProvider));
            if (ModelState.IsValid)
            {
                _usbRepo.Update(usb);
                return RedirectToAction(nameof(Details), new { id = usb.Id });
            }
            PopulateServiceProvidersDropDownList(usb.ServiceProviderId);
            return View(usb);
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var usb = _usbRepo.GetById(id);
            if (usb == null)
            {
                return NotFound();
            }
            return View(usb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var usb = _usbRepo.GetById(id);
            if (usb == null)
            {
                return NotFound();
            }
            usb.IsActive = false;
            _usbRepo.Update(usb);
            return RedirectToAction("Index", "SIM", new { type = "all" });
        }
        private void PopulateServiceProvidersDropDownList(object? selectedProvider = null)
        {
            var providersQuery = _context.ServiceProviders
                .OrderBy(sp => sp.Name)
                .ToList();
            ViewBag.ServiceProviders = new SelectList(providersQuery, "Id", "Name", selectedProvider);
        }
    }
}