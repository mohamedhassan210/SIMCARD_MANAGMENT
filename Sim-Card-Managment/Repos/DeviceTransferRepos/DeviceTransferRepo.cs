using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DeviceTransferRepo : IDeviceTransferRepo
    {
        private readonly AppDbContext _context;

        public DeviceTransferRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DeviceTransfer> GetAllDeviceTransfers()
        {
            return _context.DeviceTransfers.ToList();
        }
        public Subscription? GetActiveSubscriptionBySimId(int simId)
        {
            if (simId <= 0) return null;

            return _context.Subscriptions
                .Include(s => s.Employee)
                .Include(s => s.NonEmployee) // Eager load NonEmployee
                .Include(s => s.Sim)
                .Where(s => s.SimId == simId && (s.EndDate == null || s.EndDate > DateTime.Now))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();
        }

        public Subscription? GetActiveSubscriptionByUsbId(int usbId)
        {
            if (usbId <= 0) return null;

            return _context.Subscriptions
                .Include(s => s.Employee)
                .Include(s => s.NonEmployee)
                .Include(s => s.Usb)
                .Where(s => s.UsbId == usbId && (s.EndDate == null || s.EndDate > DateTime.Now))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();
        }

        public DeviceTransfer? GetDeviceTransferbyId(int id)
        {
            return _context.DeviceTransfers.Find(id);
        }

        public void AddDeviceTransfer(DeviceTransfer deviceTransfer)
        {
            _context.DeviceTransfers.Add(deviceTransfer);
        }

        public Subscription? GetSubscriptionById(int id)
        {
            return _context.Subscriptions.Find(id);
        }

        public void AddSubscription(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
        }

        public void CompleteTransaction()
        {
            _context.SaveChanges();
        }
    }
}