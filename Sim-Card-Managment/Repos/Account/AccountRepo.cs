using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Repos.Account
{
    public class AccountRepo : IAccountRepo
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountRepo(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserOtp> CreateAndSaveNewOtpAsync(string email, string otpCode)
        {
            var newOtp = new UserOtp
            {
                Email = email,
                OtpCode = otpCode,
                ExpireDate = DateTime.Now.AddMinutes(15), // كود صالح لمدة 15 دقيقة
                IsUsed = false
            };

            _context.UserOtps.Add(newOtp);
            await _context.SaveChangesAsync();
            return newOtp;
        }

        public async Task<UserOtp?> GetValidOtpByEmailAsync(string email)
        {
            return await _context.UserOtps
                .Where(o => o.Email == email && o.ExpireDate > DateTime.Now && !o.IsUsed)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public bool Register(RegisterViewModel model)
        {
            try
            {
                var user = new User
                {
                    //Id = int.Newint(),
                    Username = model.Username,
                    // تشفير كلمة المرور قبل حفظها في قاعدة البيانات
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash),
                    Email = model.Email,
                    GroupId = model.GroupId,
                    IsActive = true, // الحساب ينزل نشط تلقائياً
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResult> Login(LoginViewmodel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Invalid Username, Password, or Inactive Account." };
            }
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim("GroupId", user.GroupId.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Username.ToLower() == "manager" ? "Manager" : "Employee")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            user.LastLogin = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (model.Password == "123456")
            {
                return new LoginResult { IsSuccess = true, IsFirstLogin = true };
            }

            return new LoginResult { IsSuccess = true, IsFirstLogin = false };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserProfileViewModel?> GetProfileByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Group)   // ← needed for GroupName
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            return new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.Username,
                Email = user.Email ?? "No Email",
                Role = user.Username.ToLower() == "manager" ? "Manager" : "Employee",
                GroupName = user.Group?.Name ?? "N/A",   // ← add
                IsActive = user.IsActive                // ← add
            };
        }

        public async Task Logout()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // جلب جميع المجموعات لربطها بالـ Dropdown في صفحة الـ View الجديد
        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups.ToListAsync();
        }

        // --- 🔥 ميثودز الـ User Management والـ Advanced 🔥 ---

        public async Task<List<UserListItemViewModel>> GetAllUsersAsync(string? search, int? groupId, bool? isActive)
        {
            // جلب المستخدمين (هنا نلغي شرط u.IsActive == false ليعرض كل المستخدمين الحاليين)
            var query = _context.Users.AsQueryable();

            // الفلترة بالبحث عن الاسم أو الإيميل
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));
            }

            // الفلترة بناءً على المجموعة
            if (groupId.HasValue)
            {
                query = query.Where(u => u.GroupId == groupId.Value);
            }

            // الفلترة بناءً على حالة الحساب (نشط / متجمد)
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            return await query.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email ?? "No Email",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                GroupName = u.Group != null ? u.Group.Name : "N/A",
                Role = u.Username.ToLower() == "manager" ? "Manager" : "Employee"
            }).ToListAsync();
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? "",
                GroupId = user.GroupId,
                Role = user.Username.ToLower() == "manager" ? "Manager" : "Employee"
            };
        }

        public async Task<bool> UpdateUserAsync(EditUserViewModel model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null) return false;

            user.Email = model.Email;
            user.GroupId = model.GroupId;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserActiveAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // عكس حالة الحساب الحالية للتجميد أو التفعيل السريع
            user.IsActive = !user.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // الحذف الذكي: نجعل الحساب غير نشط أو نقوم بعمل حقل خاص بالـ IsDeleted لو متوفر بالموديل
            user.IsActive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ChangeUserGroupAsync(int userId, int newGroupId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.GroupId = newGroupId;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}