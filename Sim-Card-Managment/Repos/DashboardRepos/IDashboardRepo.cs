using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDashboardRepo
    {
        int GetTotalSimsCount();
        int GetTotalUsbsCount();
        IEnumerable<Employee> GetTopEmployees(int count);
        IEnumerable<Sim> GetTopSims(int count);
    }
}