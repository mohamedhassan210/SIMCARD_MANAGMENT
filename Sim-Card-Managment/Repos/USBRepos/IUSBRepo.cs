using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IUSBRepo
    {
        Task<IEnumerable<Usb>> GetAvailableUsbsAsync(string query);
        Task<IEnumerable<Usb>> GetAvailableUsbsAsync();
        IEnumerable<Usb> GetAll();
        Usb? GetById(int id);
        void Add(Usb usb);
        void Update(Usb usb);
        void Delete(int id);
    }
}