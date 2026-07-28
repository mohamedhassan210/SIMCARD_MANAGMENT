using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDeviceTransferRepo
    {
        IEnumerable<DeviceTransfer> GetAllDeviceTransfers();
        DeviceTransfer? GetDeviceTransferbyId(int id);
        void AddDeviceTransfer(DeviceTransfer deviceTransfer);
        Subscription? GetSubscriptionById(int id);
        void AddSubscription(Subscription subscription);
        void CompleteTransaction();
    }
}