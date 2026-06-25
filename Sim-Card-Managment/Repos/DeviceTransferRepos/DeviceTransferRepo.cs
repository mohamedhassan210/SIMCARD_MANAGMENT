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
            _context.SaveChanges();
        }

        public void DeleteTransfer(Guid id)
        {
            var transfer = GetDeviceTransferbyId(id);
            if (transfer != null)
            {
                _context.DeviceTransfers.Remove(transfer);
                _context.SaveChanges();
            }
        }

        public void Update(DeviceTransfer deviceTransfer)
        {
            _context.DeviceTransfers.Update(deviceTransfer);
            _context.SaveChanges();
        }
    }
}