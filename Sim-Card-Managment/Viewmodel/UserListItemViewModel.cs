using System;



namespace Sim_Card_Managment.Viewmodel

{

    public class UserListItemViewModel

    {

        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty; // Advanced: عشان نعرض اسم الجروب بدل الـ GUID المبهم

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; } // Advanced: عشان الـ Soft Delete

        public DateTime? LastLogin { get; set; } // Advanced: لتتبع آخر ظهور للموظف

        public DateTime CreatedAt { get; set; }

    }

}