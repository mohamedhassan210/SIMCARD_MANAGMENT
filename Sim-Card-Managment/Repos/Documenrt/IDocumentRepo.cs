using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface IDocumentRepo
    {
        Task<IEnumerable<Document>> GetAllAsync(string? searchTerm = null, int? documentTypeId = null);
        Task<IEnumerable<Document>> GetAllAsync();
        Task<Document?> GetByIdAsync(int id);
        Task AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
        // Ensure your fetch method includes .Include(d => d.DocumentType).Include(d => d.CreatedBy).Include(d => d.Serials)
        Task<IEnumerable<Document>> GetFilteredDocumentsAsync(string searchTerm, int? documentTypeId);
    }
}
