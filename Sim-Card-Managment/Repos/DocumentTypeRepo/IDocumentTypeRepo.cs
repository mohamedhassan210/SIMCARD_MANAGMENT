using Sim_Card_Managment.Models;
namespace Sim_Card_Managment.Repos
{
    public interface IDocumentTypeRepo
    {
        Task<IEnumerable<DocumentType>> GetAllAsync();
        Task<DocumentType?> GetByIdAsync(int id);
        Task AddAsync(DocumentType documentType);
        Task UpdateAsync(DocumentType documentType);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
