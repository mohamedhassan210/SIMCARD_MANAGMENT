using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.USBRepo
{
    public class USBRepo : IUSBRepo
    {
        private readonly AppDbContext _context;

        public USBRepo(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Usb> GetAll()
        {
            return _context.Usbs.ToList();
        }

        public Usb? GetById(Guid id)
        {
            return _context.Usbs.Find(id);
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

        public void Delete(Guid id)
        {
            var usb = GetById(id);

            if (usb != null)
            {
                _context.Usbs.Remove(usb);
                _context.SaveChanges();
            }
        }
    }
}