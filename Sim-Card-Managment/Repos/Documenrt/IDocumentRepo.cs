using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface IDocumentRepo
    {
        Task<IEnumerable<Document>> GetAllAsync(string? searchTerm = null, Guid? documentTypeId = null);
        Task<Document?> GetByIdAsync(Guid id);
        Task AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
