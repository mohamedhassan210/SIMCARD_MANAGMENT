using Microsoft.EntityFrameworkCore;
using Sim_Card_Managment.data;
using Sim_Card_Managment.Repos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("conn1")));

builder.Services.AddScoped<IUSBRepo, USBRepo>();
builder.Services.AddScoped<ISIMRepo, SIMRepo>();
builder.Services.AddScoped<IQuotaRepo, QuotaRepo>();
builder.Services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<INonEmployeeRepo, NonEmployeeRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
