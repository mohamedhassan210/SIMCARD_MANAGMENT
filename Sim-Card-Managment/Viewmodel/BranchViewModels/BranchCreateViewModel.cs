using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class BranchCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool? VpnOverInternetStatus { get; set; }
        public int CreatedById { get; set; }
    }
}