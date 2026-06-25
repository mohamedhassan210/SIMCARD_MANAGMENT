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
        public DeviceAction? GetDeviceActionbyId(Guid id)
        {
            return _context.Actions.Find(id);
        }
        public void AddDeviceAction(DeviceAction deviceAction)
        {
            _context.Actions.Add(deviceAction);
            _context.SaveChanges();
        }
        public void DeleteAction(Guid id)
        {
            var action = GetDeviceActionbyId(id);
            if(action != null)
            {
                _context.Actions.Remove(action);
                _context.SaveChanges();
            }
        }

        public void Update(DeviceAction deviceAction)
        {
            _context.Actions.Update(deviceAction);
            _context.SaveChanges();
        }
    }
}
