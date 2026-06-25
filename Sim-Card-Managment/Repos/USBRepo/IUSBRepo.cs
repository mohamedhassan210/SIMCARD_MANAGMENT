using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos.USBRepo.USBRepo
{
    public interface IUSBRepo
    {
        IEnumerable<Usb> GetAll();
        Usb? GetById(Guid id);
        void Add(Usb usb);
        void Update(Usb usb);
        void Delete(Guid id);
    }
}