using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using webProje.Models;
using System.Security.Claims;
namespace webProje.Controllers
{
	[Authorize]
	public class RandevuController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public RandevuController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		
		public async Task<IActionResult> Index()
		{
			ViewBag.Hizmetler = await _context.Hizmetler.ToListAsync();
			ViewBag.Antrenorler = await _context.Antrenorler.ToListAsync();
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Randevu randevu, int hizmetId)
		{
			var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
			if (hizmet == null)
			{
				ModelState.AddModelError("", "Seçilen hizmet bulunamadı.");
				return View("Index", randevu);
			}

			
		}
	
	}
}
