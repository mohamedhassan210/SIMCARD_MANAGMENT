namespace Sim_Card_Managment.ViewModels
{
    public class NonEmployeeIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public int ActiveSimOnlyCount { get; set; }
        public int ActiveUsbCount { get; set; }
    }
}