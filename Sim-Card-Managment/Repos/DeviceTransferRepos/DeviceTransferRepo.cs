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

        public DeviceTransfer? GetDeviceTransferbyId(Guid id)
        {
            return _context.DeviceTransfers.Find(id);
        }

        public void AddDeviceTransfer(DeviceTransfer deviceTransfer)
        {
            _context.DeviceTransfers.Add(deviceTransfer);
        }

        public Subscription? GetSubscriptionById(Guid id)
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