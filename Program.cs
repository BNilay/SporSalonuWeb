using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Data;
using yeniWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// DATABASE BAĞLANTISI
// ------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ------------------------------------------------------
// IDENTITY + ROLE SİSTEMİ
// ------------------------------------------------------
builder.Services.AddDefaultIdentity<UserDetails>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()                  // <-- Roller aktif
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ------------------------------------------------------

async Task CreateRolesAndAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDetails>>();

    
    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // --- Admin kullanıcı bilgileri ---
    string adminEmail = "b231210376@sakarya.edu.tr";
    string adminPassword = "Sau123!";  // Şifre kurallara uygun olmalı

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new UserDetails
        {
            UserName = adminEmail,
            Email = adminEmail,
            UserAd = "Site",
            UserSoyad = "Yöneticisi",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (!createResult.Succeeded)
        {
            throw new Exception("Admin kullanıcı oluşturulamadı: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }

   
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}


await CreateRolesAndAdminAsync(app);


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
