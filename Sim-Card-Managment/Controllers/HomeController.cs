using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            ViewBag.ActiveSims = _repo.GetActiveSimsCount();
            ViewBag.LostSims = _repo.GetDeviceStatusCount("Lost", true);
            ViewBag.ReplacedSims = _repo.GetDeviceStatusCount("Replaced", true);
            ViewBag.ReturnedSims = _repo.GetDeviceStatusCount("Returned", true);

            
            ViewBag.ActiveUsbs = _repo.GetActiveUsbsCount();
            ViewBag.LostUsbs = _repo.GetDeviceStatusCount("Lost", false);
            ViewBag.ReplacedUsbs = _repo.GetDeviceStatusCount("Replaced", false);
            ViewBag.ReturnedUsbs = _repo.GetDeviceStatusCount("Returned", false);

            
            ViewBag.RecentEmployees = _repo.GetTopEmployees(4);
            ViewBag.RecentSims = _repo.GetTopSims(4);

            return View();
        }
    }
}