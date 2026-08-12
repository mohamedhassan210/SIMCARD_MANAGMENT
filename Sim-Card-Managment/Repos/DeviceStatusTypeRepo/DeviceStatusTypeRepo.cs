using Sim_Card_Managment.data;
using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Repos.DeviceStatusTypeRepo
{
    public class DeviceStatusTypeRepo : IDeviceStatusTypeRepo
    {
        private readonly AppDbContext _context;

        public DeviceStatusTypeRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DeviceStatusType> GetAll()
        {
            return _context.DeviceStatusesType.ToList();
        }

        public DeviceStatusType? GetById(int id)
        {
            return _context.DeviceStatusesType.Find(id);
        }

        public void Add(DeviceStatusType deviceStatusType)
        {
            _context.DeviceStatusesType.Add(deviceStatusType);
            _context.SaveChanges();
        }

        public void Update(DeviceStatusType deviceStatusType)
        {
            _context.DeviceStatusesType.Update(deviceStatusType);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _context.DeviceStatusesType.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}