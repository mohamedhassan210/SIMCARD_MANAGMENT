using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDeviceTransferRepo
    {
        IEnumerable<DeviceTransfer> GetAllDeviceTransfers();
        DeviceTransfer? GetDeviceTransferbyId(Guid id);
        void AddDeviceTransfer(DeviceTransfer deviceTransfer);
        void Update(DeviceTransfer deviceTransfer);
        void DeleteTransfer(Guid id);
    }
}