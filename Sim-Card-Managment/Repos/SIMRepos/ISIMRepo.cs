using Microsoft.VisualStudio.CodeCoverage;
using Sim_Card_Managment.Models;


namespace Sim_Card_Managment.Repos
{
    public interface ISIMRepo
    {
        Task<IEnumerable<Sim>> GetAvailableSimsAsync();
        Task<IEnumerable<Sim>> GetAvailableSimsAsync(string query);
        Task AddAsync(Sim sim);
        Task UpdateAsync(Sim sim);
        Task<Sim?> GetByIdAsync(int id);
        Task<Sim?> GetBySerialNumberAsync(string serialNumber);
        Task<IEnumerable<Sim>> SearchAsync(string searchTerm);
        IEnumerable<Sim> GetAll();
        Sim? GetById(int id);
        void Add(Sim sim);
        void Update(Sim sim);
        void Delete(int id);
        Task<IEnumerable<Sim>> GetAssignableSimsAsync(string query);
        Task<List<Sim>> GetAssignableSimsForInternetLineAsync(string? query, int? excludeLineId = null);
    }
}