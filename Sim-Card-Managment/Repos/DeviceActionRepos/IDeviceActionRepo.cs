using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDeviceActionRepo
    {
        IEnumerable<DeviceAction> GetAllDeviceActions();
        void AddDeviceAction(DeviceAction deviceAction);
    }
}