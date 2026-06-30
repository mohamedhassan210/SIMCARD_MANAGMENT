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
            ViewBag.TotalSims = _repo.GetTotalSimsCount();
            ViewBag.TotalUsbs = _repo.GetTotalUsbsCount();

            ViewBag.RecentEmployees = _repo.GetTopEmployees(4);
            ViewBag.RecentSims = _repo.GetTopSims(4);

            return View();
        }
    }
}