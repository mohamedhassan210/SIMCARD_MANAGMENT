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



        public bool Register(RegisterViewModel model)

        {

            try

            {

                var user = new User

                {

                    Username = model.Username,

                    PasswordHash = model.PasswordHash,

                    Email = model.Email,

                    GroupId = model.GroupId,

                    IsActive = true,

                    CreatedAt = DateTime.Now,

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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);



            if (user == null || user.IsDeleted) // 🔐 تم إضافة شرط الحذف الذكي هنا لمنع دخول الممسوحين

            {

                return new LoginResult { IsSuccess = false, ErrorMessage = "Invalid Username or Password." };

            }



            var claims = new List<Claim>

            {

                new Claim(ClaimTypes.Name, user.Username),

                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(ClaimTypes.Email, user.Email ?? ""),

                new Claim("GroupId", user.GroupId.ToString()),

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



            if (user.PasswordHash == "123456")

            {

                return new LoginResult { IsSuccess = true, IsFirstLogin = true };

            }



            return new LoginResult { IsSuccess = true, IsFirstLogin = false };

        }



        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)

        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null) return false;



            user.PasswordHash = model.NewPassword;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;

        }



        public async Task<UserProfileViewModel?> GetProfileByIdAsync(Guid id)

        {

            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted) return null;



            return new UserProfileViewModel

            {

                Id = user.Id,

                FullName = user.Username,

                Email = user.Email ?? "No Email",

                Role = user.Username.ToLower() == "manager" ? "Manager" : "Employee"

            };

        }



        public async Task Logout()

        {

            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        }



        // --- 🔥 بداية ميثودز الـ User Management الجديدة والـ Advanced 🔥 ---



        public async Task<List<UserListItemViewModel>> GetAllUsersAsync(string? search, Guid? groupId, bool? isActive)

        {

            // جلب المستخدمين الذين لم يتم حذفهم حذفاً ذكياً

            var query = _context.Users.Where(u => u.IsDeleted == false).AsQueryable();



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

                Role = u.Username.ToLower() == "manager" ? "Manager" : "Employee"

            }).ToListAsync();

        }



        public async Task<EditUserViewModel?> GetUserForEditAsync(Guid id)

        {

            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted) return null;



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

            if (user == null || user.IsDeleted) return false;



            user.Email = model.Email;

            user.GroupId = model.GroupId;



            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;

        }



        public async Task<bool> ToggleUserActiveAsync(Guid id)

        {

            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted) return false;



            // عكس حالة الحساب الحالية للتجميد أو التفعيل السريع

            user.IsActive = !user.IsActive;



            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;

        }



        public async Task<bool> SoftDeleteUserAsync(Guid id)

        {

            var user = await _context.Users.FindAsync(id);

            if (user == null) return false;



            // تحويل حالة الحذف الذكي إلى true ليختفي من العرض مع الحفاظ على سجلاته

            user.IsDeleted = true;



            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;

        }

    }

}

