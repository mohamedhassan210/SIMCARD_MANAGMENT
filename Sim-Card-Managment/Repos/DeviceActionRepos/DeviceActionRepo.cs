using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DeviceActionRepo : IDeviceActionRepo
    {
        private readonly AppDbContext _context;

        public DeviceActionRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DeviceAction> GetAllDeviceActions()
        {
            return _context.Actions.ToList();
        }

        public void AddDeviceAction(DeviceAction deviceAction)
        {
            _context.Actions.Add(deviceAction);
            _context.SaveChanges();
        }
    }
}