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
        // Ensure your fetch method includes .Include(d => d.DocumentType).Include(d => d.CreatedBy).Include(d => d.Serials)
        Task<IEnumerable<Document>> GetFilteredDocumentsAsync(string searchTerm, Guid? documentTypeId);
    }
}
