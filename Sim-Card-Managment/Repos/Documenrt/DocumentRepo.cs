using Sim_Card_Managment.data;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class DocumentRepo : IDocumentRepo
    {
        private readonly AppDbContext _context; 

        public DocumentRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllAsync(string? searchTerm = null, Guid? documentTypeId = null)
        {
            var query = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.CreatedBy)
                .AsQueryable();

            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumenttypeId == documentTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchTerm) || d.Notes.Contains(searchTerm));
            }

            return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            return await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.CreatedBy)
                .Include(d => d.Serials)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
        }



        public async Task<IEnumerable<Document>> GetFilteredDocumentsAsync(string searchTerm, Guid? documentTypeId)
        {
            var query = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.CreatedBy)
                .Include(d => d.Serials)
                .AsQueryable();

            // Filter by text search (Document number or Notes text match)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchTerm) || d.Notes.Contains(searchTerm));
            }

            // Filter by Document Type dropdown selection
            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumenttypeId == documentTypeId.Value);
            }

            return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }


        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) >= 0;
        }
    }
}
