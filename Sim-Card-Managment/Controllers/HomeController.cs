using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;

namespace Sim_Card_Managment.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardRepo _repo;

        public HomeController(IDashboardRepo repo)
        {
            _repo = repo;
        }

        public IActionResult home()
        {
            ViewBag.ActiveSims = _repo.GetActiveSimsCount();

            
            ViewBag.ActiveUsbs = _repo.GetActiveUsbsCount();

            ViewBag.RecentEmployees = _repo.GetTopEmployees(4);
            ViewBag.RecentSims = _repo.GetTopSims(4);

            return View();
        }
        [HttpGet]
        public IActionResult GetWeeklyActivityData()
        {
            var chartData = _repo.GetWeeklyActivityData(); // Calling the repository

            return Json(new
            {
                simData = chartData.SimCounts,
                usbData = chartData.UsbCounts
            });
        }
    }
}