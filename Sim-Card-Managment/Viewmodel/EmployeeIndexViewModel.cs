using System;

namespace Sim_Card_Managment.ViewModels
{
    public class EmployeeIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NationalID { get; set; } = string.Empty;
        public int ActiveSimOnlyCount { get; set; }
        public int ActiveUsbCount { get; set; }
    }
}