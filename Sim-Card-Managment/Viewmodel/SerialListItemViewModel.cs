using System.ComponentModel.DataAnnotations;

namespace Sim_Card_Managment.Viewmodel
{
    public class SerialListItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "رقم السيريال")]
        public string SerialNumber { get; set; }

        [Display(Name = "رقم المستند")]
        public string DocumentNumber { get; set; }

        [Display(Name = "نوع المستند")]
        public string DocumentTypeName { get; set; }

        [Display(Name = "شريحة SIM المرتبطة")]
        public string? SimPhoneNumber { get; set; }

        [Display(Name = "جهاز USB المرتبط")]
        public string? UsbModelOrSerial { get; set; }

        [Display(Name = "بواسطة")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
    }
}
