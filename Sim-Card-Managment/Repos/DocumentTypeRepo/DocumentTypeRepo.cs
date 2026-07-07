using Sim_Card_Managment.data;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DocumentTypeRepo : IDocumentTypeRepo
    {
        private readonly AppDbContext _context;

        public DocumentTypeRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DocumentType>> GetAllAsync()
        {
            return await _context.DocumentTypes.OrderBy(dt => dt.DisplayName).ToListAsync();
        }

        public async Task<DocumentType?> GetByIdAsync(Guid id)
        {
            return await _context.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == id);
        }

        public async Task AddAsync(DocumentType documentType)
        {
            await _context.DocumentTypes.AddAsync(documentType);
        }

        public async Task UpdateAsync(DocumentType documentType)
        {
            _context.DocumentTypes.Update(documentType);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType != null)
            {
                _context.DocumentTypes.Remove(documentType);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) >= 0;
        }
    }
}
