using Sim_Card_Management.Models;

namespace Sim_Card_Management.Repos.DeviceSerialOperationsRepos
{
    public interface IDeviceSerialOperationsRepo
    {
        Task AddAsync(DeviceSerialOperation operation);
        Task<DeviceSerialOperation?> GetByIdAsync(int id);
        Task<IEnumerable<DeviceSerialOperation>> GetAllAsync();
    }
}
