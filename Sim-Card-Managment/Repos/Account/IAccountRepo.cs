using System.Threading.Tasks;
using Sim_Card_Managment.Viewmodel;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Repos.Account
{
    public interface IAccountRepo
    {
        UserOtp? GetValidOtpByEmail(string email);
        User? GetUserByEmail(string email);
        bool Register(RegisterViewModel model);
        Task<LoginResult> Login(LoginViewmodel model);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
        Task Logout();
        Task<UserProfileViewModel?> GetProfileByIdAsync(Guid id);

        // 🔥 تم إضافة الـ Methods المتطورة لإدارة المستخدمين هنا 🔥
        Task<List<UserListItemViewModel>> GetAllUsersAsync(string? search, Guid? groupId, bool? isActive);
        Task<EditUserViewModel?> GetUserForEditAsync(Guid id);
        Task<bool> UpdateUserAsync(EditUserViewModel model);
        Task<bool> ToggleUserActiveAsync(Guid id); // تجميد/تفعيل الحساب
        Task<bool> SoftDeleteUserAsync(Guid id);   // الحذف الذكي (إخفاء وليس مسح نهائي)
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public bool IsFirstLogin { get; set; }
        public string? ErrorMessage { get; set; }
    }
}