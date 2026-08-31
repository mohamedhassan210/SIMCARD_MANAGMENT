

    namespace Sim_Card_Managment.Viewmodel
    {
        public class HardwareLifecycleItemViewModel
        {
            public int Id { get; set; }
            public string CurrentHolderName { get; set; } = "Unassigned";
            public string AccountType { get; set; } = "Internal Employee";
            public string? PhoneNumber { get; set; }
            public string? SimSerialNumber { get; set; }
            public string? UsbSerialNumber { get; set; }
            public string PreviousHolderName { get; set; } = "None (First Owner)";
            public string? Notes { get; set; }
            public DateTime TransferDate { get; set; }

            // "sim" or "usb" — used for the device-type filter
            public string DeviceType { get; set; } = "";
        }
    }

