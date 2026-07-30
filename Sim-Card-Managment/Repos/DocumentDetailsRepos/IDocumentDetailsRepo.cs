using Sim_Card_Management.Models;

namespace Sim_Card_Management.Repos.DocumentDetailsRepos
{
    public interface IDocumentDetailsRepo
    {
        Task AddAsync(DocumentDetails documentDetails);
        Task<DocumentDetails?> GetByIdAsync(int id);
    }
}
