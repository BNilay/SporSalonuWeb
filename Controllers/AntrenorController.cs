using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using webProje.Models;
using Microsoft.EntityFrameworkCore;
namespace webProje.Controllers
{
	[Authorize(Roles ="Admin")]
	public class AntrenorController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AntrenorController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Listele()
		{
			var antrenorler = await _context.Antrenorler.ToListAsync();
			return View(antrenorler);
		}

		public IActionResult Create()
		{
			return View();
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Antrenor antrenor)
		{
			// Model doğrulaması (sunucu taraflı)
			if (ModelState.IsValid)
			{
				_context.Add(antrenor);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Listele));
			}
			return View(antrenor);
		}

		public	async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var antrenor = await _context.Antrenorler.FindAsync(id);
			if (antrenor == null)
			{
				return NotFound();
			}
			return View(antrenor);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Antrenor antrenor)
		{
			if (id != antrenor.AntrenorId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(antrenor);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Antrenorler.Any(e=> e.AntrenorId == antrenor.AntrenorId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Listele));
			}
			return View(antrenor);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var antrenor = await _context.Antrenorler
				.FirstOrDefaultAsync(m => m.AntrenorId == id);
			if (antrenor == null)
			{
				return NotFound();
			}

			return View(antrenor);
		}
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var antrenor = await _context.Antrenorler.FindAsync(id);
			
			if(antrenor != null) 
			{
				_context.Antrenorler.Remove(antrenor);
			}
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Listele));
		}
	}
}
