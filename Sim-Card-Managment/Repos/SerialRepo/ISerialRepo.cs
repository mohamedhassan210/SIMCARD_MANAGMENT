using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface ISerialRepo
    {
        Task<IEnumerable<Serial>> GetAllAsync(string? serialNumber = null, Guid? documentId = null);
        Task<Serial?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(string serialNumber); 
        Task AddAsync(Serial serial);
        Task AddRangeAsync(IEnumerable<Serial> serials);
        Task DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
