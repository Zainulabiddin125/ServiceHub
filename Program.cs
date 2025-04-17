using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ServiceHubContextConnection") ?? throw new InvalidOperationException("Connection string 'ServiceHubContextConnection' not found.");

builder.Services.AddDbContext<ServiceHubContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>().AddEntityFrameworkStores<ServiceHubContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login"; 
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(
    options => {
        options.Password.RequireUppercase = false;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// Add area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AttendanceMachine}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) =>
{
    // Allow users to access Login and Register pages
    if (!context.User.Identity.IsAuthenticated &&
        !context.Request.Path.StartsWithSegments("/Identity/Account/Login") &&
        !context.Request.Path.StartsWithSegments("/Identity/Account/Register") &&
        !context.Request.Path.StartsWithSegments("/Identity/Account/Logout"))
    {
        //return;
        // Check if the request is for an API endpoint
        if (!context.Request.Path.StartsWithSegments("/HR/TransferEmployee_API"))
        {
            context.Response.Redirect("/Identity/Account/Login");
            return;
        }
    }
    await next();
});

app.MapRazorPages();

app.Run();
