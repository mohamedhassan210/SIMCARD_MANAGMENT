using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly AppDbContext _context;

        public DashboardRepo(AppDbContext context)
        {
            _context = context;
        }

        public int GetTotalSimsCount()
        {
            return _context.Sims.Count();
        }

        public int GetTotalUsbsCount()
        {
            return _context.Usbs.Count();
        }

        public IEnumerable<Employee> GetTopEmployees(int count)
        {
            return _context.Employees.Take(count).ToList();
        }

        public IEnumerable<Sim> GetTopSims(int count)
        {
            return _context.Sims.Take(count).ToList();
        }
    }
}