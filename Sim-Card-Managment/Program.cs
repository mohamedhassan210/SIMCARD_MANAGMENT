using Microsoft.AspNetCore.Authentication.Cookies;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Repos.SubscriptionRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// 3. ÅÚÏÇÏ äÙÇã ÇáÜ Cookie Authentication æÇáãÓÇÑÇÊ ÇáãØáæÈÉ (Login & AccessDenied)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// ----------------------------------------

builder.Services.AddScoped<IUSBRepo, USBRepo>();
builder.Services.AddScoped<ISIMRepo, SIMRepo>();
builder.Services.AddScoped<IQuotaRepo, QuotaRepo>();
builder.Services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication(); 
// ----------------------------------------

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();