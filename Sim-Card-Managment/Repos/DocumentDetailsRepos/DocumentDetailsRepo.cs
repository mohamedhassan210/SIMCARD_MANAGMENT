using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Models;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using System.Threading.Tasks;
namespace Sim_Card_Management.Repos.DocumentDetailsRepos
{
    public class DocumentDetailsRepo : IDocumentDetailsRepo
    {
        private readonly AppDbContext _context;

        public DocumentDetailsRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DocumentDetails documentDetails)
        {
            await _context.DocumentDetails.AddAsync(documentDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<DocumentDetails?> GetByIdAsync(int id)
        {
            return await _context.DocumentDetails
                .FirstOrDefaultAsync(dd => dd.Id == id);
        }
    }
}
