using Sim_Card_Managment.Models;
using Sim_Card_Managment.Viewmodel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sim_Card_Managment.Repos.Account
{
    public interface IAccountRepo
    {
        Task<UserOtp> CreateAndSaveNewOtpAsync(string email, string otpCode);
        Task<UserOtp?> GetValidOtpByEmailAsync(string email);
        Task<User?> GetUserByEmailAsync(string email);
        bool Register(RegisterViewModel model);
        Task<LoginResult> Login(LoginViewmodel model);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
        Task Logout();
        Task<UserProfileViewModel?> GetProfileByIdAsync(int id);

        // الميثود الجديدة لجلب المجموعات من قاعدة البيانات
        Task<List<Group>> GetAllGroupsAsync();

        // 🔥 الـ Methods المتطورة لإدارة المستخدمين 🔥
        Task<List<UserListItemViewModel>> GetAllUsersAsync(string? search, int? groupId, bool? isActive);
        Task<EditUserViewModel?> GetUserForEditAsync(int id);
        Task<bool> UpdateUserAsync(EditUserViewModel model);
        Task<bool> ToggleUserActiveAsync(int id); // تجميد/تفعيل الحساب
        Task<bool> SoftDeleteUserAsync(int id);   // الحذف الذكي (إخفاء وليس مسح نهائي)
        Task<bool> ChangeUserGroupAsync(int userId, int newGroupId);
        Task<ChangePasswordResult> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> ActivateUserAsync(int id);
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public bool IsFirstLogin { get; set; }
        public string? ErrorMessage { get; set; }
    }
}