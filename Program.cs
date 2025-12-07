using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webProje.Models; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		// Senkron çalýþtýrmak için GetAwaiter().GetResult() kullanýlýr.
		SeedRolesAndAdminUser(services).GetAwaiter().GetResult();
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Veritabaný tohumlama iþlemi sýrasýnda hata oluþtu.");
	}
}



if (!app.Environment.IsDevelopment())
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

app.Run();



static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
{

	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	
	string[] roleNames = { "Admin", "Uye" };

	foreach (var roleName in roleNames)
	{
		var roleExist = await roleManager.RoleExistsAsync(roleName);
		if (!roleExist)
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}


	const string adminEmail = "b231210376@sakarya.edu.tr";
	const string adminPassword = "sau"; 
	const string adminRole = "Admin";

	var adminUser = await userManager.FindByEmailAsync(adminEmail);

	if (adminUser == null)
	{
		var newAdmin = new ApplicationUser
		{
			UserName = adminEmail,
			Email = adminEmail,
			EmailConfirmed = true
		};

		var result = await userManager.CreateAsync(newAdmin, adminPassword);

		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(newAdmin, adminRole);
		}
	}
}