using Sim_Card_Managment.Models;

namespace Sim_Card_Managment.Repos
{
    public interface IDashboardRepo
    {
        int GetActiveSimsCount();
        int GetActiveUsbsCount();
        int GetDeviceStatusCount(string statusType, bool isSim);
        IEnumerable<Employee> GetTopEmployees(int count);
        IEnumerable<Sim> GetTopSims(int count);
    }
}