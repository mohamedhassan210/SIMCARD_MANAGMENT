namespace Sim_Card_Managment.Viewmodel
{
    // Editing an existing status log entry — same shape as Create, plus the Id
    // of the record being changed.
    public class DeviceStatusEditViewModel : DeviceStatusCreateViewModel
    {
        public int Id { get; set; }
    }
}