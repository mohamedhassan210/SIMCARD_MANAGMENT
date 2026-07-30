using System;
using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class DeviceSerialOperationViewModel
    {
        // ===== SEARCH/DISPLAY FIELDS =====
        /// <summary>
        /// User input: search by phone number or serial number
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        // ===== READ-ONLY SIM DETAILS (Populated after search) =====
        public int SimId { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Current Serial Number")]
        public string CurrentSerialNumber { get; set; }

        [Display(Name = "Network Type")]
        public string NetworkType { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Service Provider")]
        public string ServiceProviderName { get; set; }

        // ===== EDITABLE OPERATION FIELDS =====
        [Required(ErrorMessage = "New Serial Number is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Serial number must be between 1 and 100 characters")]
        [Display(Name = "New Serial Number")]
        public string NewSerialNumber { get; set; }

        [Display(Name = "Network Type Change")]
        public bool NetworkTypeChange { get; set; } = false;

        // ===== AUDIT/METADATA =====
        [ScaffoldColumn(false)]
        public DateTime OperationDate { get; set; }

        [ScaffoldColumn(false)]
        public int CreatedById { get; set; } = 1; // Mock value as per requirements
    }
}