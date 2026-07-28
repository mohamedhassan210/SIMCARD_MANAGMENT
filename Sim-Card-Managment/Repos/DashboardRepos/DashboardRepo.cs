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

        public int GetActiveSimsCount() => _context.Sims.Count();

        public int GetActiveUsbsCount() => _context.Usbs.Count();

        public int GetDeviceStatusCount(string statusType, bool isSim)
        {
            if (isSim) return _context.DeviceStatuses.Count(d => d.StatusType.Name == statusType && d.SimId != null);
            else return _context.DeviceStatuses.Count(d => d.StatusType.Name == statusType && d.UsbId != null);
        }

        public IEnumerable<Employee> GetTopEmployees(int count) => _context.Employees.Take(count).ToList();

        public IEnumerable<Sim> GetTopSims(int count) => _context.Sims.Take(count).ToList();

        public (int[] SimCounts, int[] UsbCounts) GetWeeklyActivityData()
        {
            var daysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            var filterDate = DateTime.Today.AddDays(-7);

            // 1. Fetch filtered data from DB into memory first (.ToList())
            var rawSimData = _context.Sims
                .Where(s => s.RegisteredAt >= filterDate)
                .Select(s => s.RegisteredAt)
                .ToList();

            var rawUsbData = _context.Usbs
                .Where(u => u.RegisteredAt >= filterDate)
                .Select(u => u.RegisteredAt)
                .ToList();

            // 2. Perform grouping in-memory (LINQ to Objects)
            var simDataGroup = rawSimData
                .GroupBy(date => date.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToList();

            var usbDataGroup = rawUsbData
                .GroupBy(date => date.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToList();

            // 3. Map into the day arrays for the chart
            var simCounts = daysOfWeek.Select(d => simDataGroup.FirstOrDefault(s => s.Day == d)?.Count ?? 0).ToArray();
            var usbCounts = daysOfWeek.Select(d => usbDataGroup.FirstOrDefault(u => u.Day == d)?.Count ?? 0).ToArray();

            return (simCounts, usbCounts);
        }
    }
}