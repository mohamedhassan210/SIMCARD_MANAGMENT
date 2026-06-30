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

        public int GetActiveSimsCount()
        {
            return _context.Subscriptions.Count(s => s.EndDate == null);
        }

        public int GetActiveUsbsCount()
        {
            return _context.Subscriptions.Count(s => s.EndDate == null && s.UsbId != null);
        }

        public int GetDeviceStatusCount(string statusType, bool isSim)
        {
            if (isSim)
            {
                return _context.DeviceStatuses.Count(d => d.StatusType == statusType && d.SimId != null);
            }
            else
            {
                return _context.DeviceStatuses.Count(d => d.StatusType == statusType && d.UsbId != null);
            }
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