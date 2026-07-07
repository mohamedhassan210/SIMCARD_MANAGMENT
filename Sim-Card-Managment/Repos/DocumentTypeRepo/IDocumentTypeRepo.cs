using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface IDocumentTypeRepo
    {
        Task<IEnumerable<DocumentType>> GetAllAsync();
        Task<DocumentType?> GetByIdAsync(Guid id);
        Task AddAsync(DocumentType documentType);
        Task UpdateAsync(DocumentType documentType);
        Task DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
