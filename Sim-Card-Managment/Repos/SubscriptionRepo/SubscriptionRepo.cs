using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public class SubscriptionRepo : ISubscriptionRepo
    {
        private readonly AppDbContext _context;

        public SubscriptionRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Subscription> GetAll()
        {
            return _context.Subscriptions
                .Include(s => s.Employee)
                .Include(s => s.NonEmployee)
                .Include(s => s.Sim)
                .Include(s => s.Usb)
                .Include(s => s.Quota)
                .Include(s => s.Action)
                .Include(s => s.CreatedByUser)
                .ToList();
        }

        public Subscription? GetById(Guid id)
        {
            return _context.Subscriptions
                .Include(s => s.Employee)
                .Include(s => s.NonEmployee)
                .Include(s => s.Sim)
                .Include(s => s.Usb)
                .Include(s => s.Quota)
                .Include(s => s.Action)
                .Include(s => s.CreatedByUser)
                .FirstOrDefault(s => s.Id == id);
        }

        public void Add(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();
        }

        public void Update(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var subscription = GetById(id);

            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }
        }
    }
}