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
            List<Employee> employees  = new List<Employee>();
            Employee emp = new Employee();
            emp.Name = "Ahmed";
            emp.Position = "3la Beta3y";
            emp.NationalID = "123456";
            emp.IsActive = true;
            
            employees.Add(emp);
            Employee emp2 = new Employee();
            emp2.Name = "Ahmedd";
            emp2.Position = "3la Beta3yy";
            emp2.NationalID = "1234565";
            emp2.IsActive = true;
            employees.Add(emp2);
            ViewBag.ActiveSims = _repo.GetActiveSimsCount();
            ViewBag.LostSims = _repo.GetDeviceStatusCount("Lost", true);
            ViewBag.ReplacedSims = _repo.GetDeviceStatusCount("Replaced", true);
            ViewBag.ReturnedSims = _repo.GetDeviceStatusCount("Returned", true);

            
            ViewBag.ActiveUsbs = _repo.GetActiveUsbsCount();
            ViewBag.LostUsbs = _repo.GetDeviceStatusCount("Lost", false);
            ViewBag.ReplacedUsbs = _repo.GetDeviceStatusCount("Replaced", false);
            ViewBag.ReturnedUsbs = _repo.GetDeviceStatusCount("Returned", false);

            ViewBag.RecentEmployees = employees;
            //ViewBag.RecentEmployeess = _repo.GetTopEmployees(4);
            ViewBag.RecentSims = _repo.GetTopSims(4);

            return View();
        }
    }
}