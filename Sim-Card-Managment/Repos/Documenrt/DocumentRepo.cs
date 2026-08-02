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

        public async Task<IEnumerable<Document>> GetAllAsync(string? searchTerm = null, int? documentTypeId = null)
        {
            var query = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentDetails)
                    .ThenInclude(dd => dd.ItemType)
                .Include(d => d.DocumentDetails)
                    .ThenInclude(dd => dd.Serials)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchTerm) || (d.Notes != null && d.Notes.Contains(searchTerm)));
            }

            if (documentTypeId.HasValue)
            {
                query = query.Where(d => d.DocumenttypeId == documentTypeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.CreatedBy)
                .Include(d => d.DocumentDetails)
                .ThenInclude(d=>d.Serials)
                .Include(d=>d.ServiceProvider)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
        }
        
        


            public async Task AddAsync(Document document)
            {
                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
            }

            

            public async Task<IEnumerable<Document>> GetAllAsync()
            {
                return await _context.Documents
                    .AsNoTracking()
                    .ToListAsync();
            }
        


        public async Task<IEnumerable<Document>> GetFilteredDocumentsAsync(string searchTerm, int? documentTypeId)
        {
            var query = _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.CreatedBy)
                .Include(d => d.DocumentDetails)
                .ThenInclude(d => d.Serials)
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
