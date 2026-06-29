using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.QuoteRepo
{
    public class QuotaRepo : IQuotaRepo
    {
        private readonly AppDbContext _context;

        public QuotaRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Quota> GetAll()
        {
            return _context.Quotas.ToList();
        }

        public Quota? GetById(Guid id)
        {
            return _context.Quotas.Find(id);
        }

        public void Add(Quota quota)
        {
            _context.Quotas.Add(quota);
            _context.SaveChanges();
        }

        public void Update(Quota quota)
        {
            _context.Quotas.Update(quota);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var quota = GetById(id);

            if (quota != null)
            {
                _context.Quotas.Remove(quota);
                _context.SaveChanges();
            }
        }
    }
}