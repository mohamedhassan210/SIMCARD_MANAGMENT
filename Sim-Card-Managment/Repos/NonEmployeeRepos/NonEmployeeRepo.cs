using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Repos.NonEmployeeRepos
{
    public class NonEmployeeRepo : INonEmployeeRepo
    {
        private readonly AppDbContext _context;

        public NonEmployeeRepo(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<string> GetDistinctTypes()
        {
            return _context.NonEmployees
                .Where(n => !string.IsNullOrEmpty(n.Type))
                .Select(n => n.Type!)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }
        // Add this implementation:
        public async Task<IEnumerable<NonEmployee>> SearchNonEmployeesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<NonEmployee>();

            var lowerQuery = query.ToLower();

            return await _context.NonEmployees
                .AsNoTracking()
                .Where(ne =>
                    (ne.Name != null && ne.Name.ToLower().Contains(lowerQuery)) ||
                    (ne.ContactInfo != null && ne.ContactInfo.ToLower().Contains(lowerQuery)) ||
                    (ne.Type != null && ne.Type.ToLower().Contains(lowerQuery)))
                .Take(10) // Limit results for fast auto-complete UI performance
                .ToListAsync();
        }

        public async Task<List<PersonListItemViewModel>> GetPeopleListAsync()
        {
            var now = DateTime.Now;

            return await _context.NonEmployees
                .AsNoTracking()
                .Include(ne => ne.Subscriptions)
                .Select(ne => new PersonListItemViewModel
                {
                    Id = ne.Id,
                    Name = ne.Name,
                    ExtraInfo = ne.Type,
                    PersonType = "Non-Employee",
                    Identifier = ne.ContactInfo,
                    IsActive = ne.IsActive,

                    ActiveSimOnlyCount = ne.Subscriptions.Count(s => s.SimId != null && (s.EndDate == null || s.EndDate >= now)),
                    ActiveUsbCount = ne.Subscriptions.Count(s => s.UsbId != null && (s.EndDate == null || s.EndDate >= now)),

                    StartDate = ne.CreatedAt
                })
                .ToListAsync();
        }

        public IEnumerable<NonEmployee> GetAll()
        {
            return _context.NonEmployees.ToList();
        }

        public NonEmployee? GetById(int id)
        {
            return _context.NonEmployees
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Sim)
                        .ThenInclude(s=>s.ServiceProvider)
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Usb)
                .Include(e => e.Subscriptions!)
                    .ThenInclude(s => s.Quota)
                .FirstOrDefault(e => e.Id == id);
        }

        public void Add(NonEmployee nonEmployee)
        {
            _context.NonEmployees.Add(nonEmployee);
            _context.SaveChanges();
        }

        public void Update(NonEmployee nonEmployee)
        {
            _context.NonEmployees.Update(nonEmployee);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var nonEmployee = _context.NonEmployees.Find(id);
            if (nonEmployee != null)
            {
                nonEmployee.IsActive = false;
                _context.NonEmployees.Update(nonEmployee);
                _context.SaveChanges();
            }
        }
    }
}