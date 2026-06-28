using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDeviceStatusRepo
    {
        IEnumerable<DeviceStatus> GetAllDeviceStatuses();
        DeviceStatus? GetDeviceStatusbyId(Guid id);
        void AddDeviceStatus(DeviceStatus deviceStatus);
        void Update(DeviceStatus deviceStatus);
        void DeleteStatus(Guid id);
    }
}