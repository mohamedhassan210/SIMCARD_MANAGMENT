using Sim_Card_Management.Models;

namespace Sim_Card_Managment.Repos.DeviceStatusTypeRepo
{
    public interface IDeviceStatusTypeRepo
    {
        IEnumerable<DeviceStatusType> GetAll();
        DeviceStatusType? GetById(int id);
        void Add(DeviceStatusType deviceStatusType);
        void Update(DeviceStatusType deviceStatusType);
        void Delete(int id);
    }
}