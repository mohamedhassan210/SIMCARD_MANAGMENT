using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface ISerialRepo
    {
        Task<IEnumerable<Serial>> GetAllAsync(string? serialNumber = null, int? documentId = null);
        Task<Serial?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string serialNumber); 
        Task AddAsync(Serial serial);
        Task AddRangeAsync(IEnumerable<Serial> serials);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
