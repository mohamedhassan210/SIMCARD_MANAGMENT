using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;


namespace Sim_Card_Managment.Controllers
{
    public class USBController : Controller
    {
        private readonly IUSBRepo _usbRepo;

        public USBController(IUSBRepo usbRepo)
        {
            _usbRepo = usbRepo;
        }

        public IActionResult Index()
        {
            return View(_usbRepo.GetAll());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Usb usb)
        {
            if (ModelState.IsValid)
            {
                _usbRepo.Add(usb);
                return RedirectToAction(nameof(Index));
            }

            return View(usb);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var usb = _usbRepo.GetById(id);

            if (usb == null)
                return NotFound();

            return View(usb);
        }

        [HttpPost]
        public IActionResult Edit(Usb usb)
        {
            if (ModelState.IsValid)
            {
                _usbRepo.Update(usb);
                return RedirectToAction(nameof(Index));
            }

            return View(usb);
        }

        public IActionResult Delete(Guid id)
        {
            _usbRepo.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}