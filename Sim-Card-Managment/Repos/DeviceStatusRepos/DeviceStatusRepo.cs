using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DeviceStatusRepo : IDeviceStatusRepo
    {
        private readonly AppDbContext _context;

        public DeviceStatusRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DeviceStatus> GetAllDeviceStatuses()
        {
            return _context.DeviceStatuses
                .Include(d => d.Sim)
                .Include(d => d.Usb)
                .Include(d => d.ReportedByUser)
                .ToList();
        }

        public DeviceStatus? GetDeviceStatusbyId(Guid id)
        {
            return _context.DeviceStatuses
                .Include(d => d.Sim)
                .Include(d => d.Usb)
                .Include(d => d.ReportedByUser)
                .Include(d => d.ReplacedBySim)
                .Include(d => d.ReplacedByUsb)
                .FirstOrDefault(d => d.Id == id);
        }

        public void AddDeviceStatus(DeviceStatus deviceStatus)
        {
            _context.DeviceStatuses.Add(deviceStatus);
            _context.SaveChanges();
        }

        public void DeleteStatus(Guid id)
        {
            var status = GetDeviceStatusbyId(id);
            if (status != null)
            {
                _context.DeviceStatuses.Remove(status);
                _context.SaveChanges();
            }
        }

        public void Update(DeviceStatus deviceStatus)
        {
            _context.DeviceStatuses.Update(deviceStatus);
            _context.SaveChanges();
        }
    }
}