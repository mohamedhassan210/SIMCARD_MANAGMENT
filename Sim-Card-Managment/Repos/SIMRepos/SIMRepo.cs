using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class SIMRepo : ISIMRepo
    {
        private readonly AppDbContext _context;

        public SIMRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Sim>> GetAvailableSimsAsync()
        {
            return await _context.Sims
                // بتعدل الـ Where دي بناءً على الـ Business Logic بتاعك (مثلاً الشريحة مش مربوطة بسيريال)
                .Where(s => !_context.Serials.Any(ser => ser.SimId == s.Id))
                .ToListAsync();
        }

        public IEnumerable<Sim> GetAll()
        {
            return _context.Sims.ToList();
        }

        public Sim? GetById(Guid id)
        {
            return _context.Sims.Find(id);
        }

        public void Add(Sim sim)
        {
            _context.Sims.Add(sim);
            _context.SaveChanges();
        }

        public void Update(Sim sim)
        {
            _context.Sims.Update(sim);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var sim = GetById(id);

            if (sim != null)
            {
                _context.Sims.Remove(sim);
                _context.SaveChanges();
            }
        }
    }
}