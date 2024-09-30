using Microsoft.EntityFrameworkCore;
using StoreManagementWebsite.Models;
using SignalRChat.Hubs;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("StoreManagementPlatformContext");
builder.Services.AddDbContext<StoreManagementPlatformContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");

app.Run();
