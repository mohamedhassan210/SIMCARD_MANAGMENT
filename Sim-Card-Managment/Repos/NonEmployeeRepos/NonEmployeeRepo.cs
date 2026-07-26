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

        public NonEmployee? GetById(Guid id)
        {
            return _context.NonEmployees.Find(id);
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

        public void Delete(Guid id)
        {
            var ne = GetById(id);
            if (ne != null)
            {
                _context.NonEmployees.Remove(ne);
                _context.SaveChanges();
            }
        }
    }
}