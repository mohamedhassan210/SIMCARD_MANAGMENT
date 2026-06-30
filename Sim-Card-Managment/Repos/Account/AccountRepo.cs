using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
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
        public UserOtp? GetValidOtpByEmail(string email)
        {
            return _context.UserOtps
                .FirstOrDefault(o => o.Email == email && o.ExpireDate > DateTime.Now);
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public bool Register(RegisterViewModel model)
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

        public async Task<bool> Login(LoginViewmodel model)
        {
          
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.PasswordHash == model.Password);

            if (user == null)
             return false; 
            

           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString()) 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

           
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe, 
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

           
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return true;
        }

        public async Task Logout() => await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}