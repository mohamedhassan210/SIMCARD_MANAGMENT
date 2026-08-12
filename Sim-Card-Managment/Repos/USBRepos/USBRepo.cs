using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Sim_Card_Managment.Repos
{
    public class USBRepo : IUSBRepo
    {
        private readonly AppDbContext _context;
        public USBRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Usb>> GetAvailableUsbsAsync()
        {
            return await _context.Usbs
                .Where(u => !_context.Serials.Any(ser => ser.UsbId == u.Id))
                .ToListAsync();
        }
        public async Task<IEnumerable<Usb>> GetAvailableUsbsAsync(string query)
        {
            return await _context.Usbs
                .Include(u => u.ServiceProvider)
                .Where(u => u.IsActive &&
                            (string.IsNullOrEmpty(query) || u.SerialNumber.Contains(query) || (u.Model != null && u.Model.Contains(query))))
                .Take(6)
                .ToListAsync();
        }
        public IEnumerable<Usb> GetAll()
        {
            return _context.Usbs
                .Include(u => u.ServiceProvider)
                .Include(u => u.Subscriptions)
                .Include(u => u.DeviceStatuses)
                    .ThenInclude(ds => ds.StatusType)
                .ToList();
        }
        public Usb? GetById(int id)
        {
            return _context.Usbs
                .Include(u => u.ServiceProvider)
                .Include(u => u.Subscriptions!)
                    .ThenInclude(s => s.Employee)
                .Include(u => u.Subscriptions!)
                    .ThenInclude(s => s.NonEmployee)
                .Include(u => u.Subscriptions!)
                    .ThenInclude(s => s.Quota)
                .Include(u => u.DeviceStatuses)
                    .ThenInclude(ds => ds.StatusType)
                .FirstOrDefault(u => u.Id == id);
        }
        public async Task AddAsync(Usb usb)
        {
            await _context.Usbs.AddAsync(usb);
            await _context.SaveChangesAsync();
        }
        public async Task<Usb?> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.Usbs
                .FirstOrDefaultAsync(u => u.SerialNumber == serialNumber);
        }
        public void Add(Usb usb)
        {
            _context.Usbs.Add(usb);
            _context.SaveChanges();
        }
        public void Update(Usb usb)
        {
            _context.Usbs.Update(usb);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var usb = _context.Usbs.Find(id);
            if (usb != null)
            {
                _context.Usbs.Remove(usb);
                _context.SaveChanges();
            }
        }
        public async Task<IEnumerable<Usb>> GetAssignableUsbsAsync(string query)
        {
            return await _context.Usbs
                .Include(u => u.ServiceProvider)
                .Where(u => u.IsActive &&
                            !u.Subscriptions.Any(sub => sub.EndDate == null || sub.EndDate > DateTime.Now) &&
                            (string.IsNullOrEmpty(query) || u.SerialNumber.Contains(query) || (u.Model != null && u.Model.Contains(query))))
                .Take(6)
                .ToListAsync();
        }

    }
}