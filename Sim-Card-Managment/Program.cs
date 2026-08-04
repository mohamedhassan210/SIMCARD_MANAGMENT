using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Management.Repos.DeviceSerialOperationsRepos;
using Sim_Card_Management.Repos.DocumentDetailsRepos;
using Sim_Card_Management.Repos.ItemTypeRepos;
using Sim_Card_Managment.Services;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Repositories;
using Sim_Card_Managment.Settings;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers & Views
builder.Services.AddControllersWithViews();

// 2. Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 3. Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Clean session timeout
        options.SlidingExpiration = true;
    });

// 4. Email Services Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// 5. Database Context & Audit Interceptor
var connectionString = builder.Configuration.GetConnectionString("conn");

builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

// 6. Register Repositories & Services
builder.Services.AddScoped<IUSBRepo, USBRepo>();
builder.Services.AddScoped<ISIMRepo, SIMRepo>();
builder.Services.AddScoped<IQuotaRepo, QuotaRepo>();
builder.Services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();

// Scoped Permission Discovery Service
builder.Services.AddScoped<PermissionDiscoveryService>();

builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<IDeviceActionRepo, DeviceActionRepo>();
builder.Services.AddScoped<IDeviceStatusRepo, DeviceStatusRepo>();
builder.Services.AddScoped<IDeviceTransferRepo, DeviceTransferRepo>();
builder.Services.AddScoped<INonEmployeeRepo, NonEmployeeRepo>();
builder.Services.AddScoped<IGroupRepo, GroupRepo>();
builder.Services.AddScoped<IPermissionRepo, PermissionRepo>();
builder.Services.AddScoped<IServiceProviderRepository, ServiceProviderRepository>();
builder.Services.AddScoped<IDocumentRepo, DocumentRepo>();
builder.Services.AddScoped<IDocumentTypeRepo, DocumentTypeRepo>();
builder.Services.AddScoped<ISerialRepo, SerialRepo>();
builder.Services.AddScoped<IDocumentDetailsRepo, DocumentDetailsRepo>();
builder.Services.AddScoped<IItemTypeRepo, ItemTypeRepo>();
builder.Services.AddScoped<IDeviceSerialOperationsRepo, DeviceSerialOperationsRepo>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();

// 7. Enable Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// BUILD APP (Only ONCE)
var app = builder.Build();

// 8. Seed Permissions on App Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var discovery = scope.ServiceProvider.GetRequiredService<PermissionDiscoveryService>();

    await discovery.SeedPermissionsAsync(db);
}

// 9. Configure HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();