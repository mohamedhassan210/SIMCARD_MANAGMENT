using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repos.Account;
using Sim_Card_Managment.Repos.EmployeeRepos;
using Sim_Card_Managment.Repos.GroupRepos;
using Sim_Card_Managment.Repos.NonEmployeeRepos;
using Sim_Card_Managment.Repos.QuoteRepo;
using Sim_Card_Managment.Repositories;
using Sim_Card_Managment.Services;

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
// Ensure the path string matches "ConnectionStrings:conn"
var connectionString = builder.Configuration.GetConnectionString("conn");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUSBRepo, USBRepo>();
builder.Services.AddScoped<ISIMRepo, SIMRepo>();
builder.Services.AddScoped<IQuotaRepo, QuotaRepo>();
builder.Services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();
builder.Services.AddScoped<IDashboardRepo,DashboardRepo>();
builder.Services.AddSingleton<PermissionDiscoveryService>();
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
builder.Services.AddScoped<ISerialRepo,SerialRepo>();
builder.Services.AddSession();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var discovery = app.Services.GetRequiredService<PermissionDiscoveryService>();
    await discovery.SeedPermissionsAsync(db);
}

// Configure the HTTP request pipeline.
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
// ----------------------------------------

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=home}/{id?}");

app.Run();