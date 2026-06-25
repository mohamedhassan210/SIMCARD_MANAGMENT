using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDeviceActionRepo
    {
        IEnumerable<DeviceAction> GetAllDeviceActions();
        DeviceAction? GetDeviceActionbyId(Guid id);
        void AddDeviceAction(DeviceAction deviceAction);
        void Update(DeviceAction deviceAction);
        void DeleteAction(Guid id);
    }
}
