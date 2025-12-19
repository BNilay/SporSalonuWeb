using Microsoft.AspNetCore.Mvc;
using yeniWeb.Data;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace yeniWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SalonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalonController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
           var salonlar = await _context.Salonlar.ToListAsync();
            return View(salonlar);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salon = await _context.Salonlar
                 .FirstOrDefaultAsync(s => s.SalonId == id);

            if (salon == null)
            {
                return NotFound();
            }

            return View(salon);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Create(Salon salon) 
        {
            if (!ModelState.IsValid)
            {
                return View(salon);
            }

            _context.Salonlar.Add(salon);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id) 
        {
            if(id == null) 
            {
                return NotFound();
            }

            var salon = await _context.Salonlar.FindAsync(id);
            if(salon == null)
                return NotFound();
            return View(salon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Salon salon)
        {
            if (id != salon.SalonId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(salon);
            }

            try
            {
                _context.Update(salon);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = _context.Salonlar.Any(e => e.SalonId == salon.SalonId);
                if (!exists)
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id) 
        {
            if(id == null) 
            {
                return NotFound();
            }

            var salon = await _context.Salonlar
                .FirstOrDefaultAsync(s => s.SalonId == id);

            if(salon == null) 
            {
                return NotFound();
            }

            return View(salon);
        }
        [HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var antrenorVarMi = await _context.Antrenorler
        .AnyAsync(a => a.SalonId == id);

    if (antrenorVarMi)
    {
        TempData["Error"] = "Bu salona bağlı antrenörler var. Önce antrenörleri silmelisiniz.";
        return RedirectToAction(nameof(Delete), new { id });
    }

    var hizmetVarMi = await _context.Hizmetler
        .AnyAsync(h => h.SalonId == id);

    if (hizmetVarMi)
    {
        TempData["Error"] = "Bu salona bağlı hizmetler var. Önce hizmetleri silmelisiniz.";
        return RedirectToAction(nameof(Delete), new { id });
    }

    var randevuVarMi = await _context.Randevular
        .AnyAsync(r => r.Antrenor.Salon.SalonId == id);

    if (randevuVarMi)
    {
        TempData["Error"] = "Bu salona bağlı randevular var. Önce randevuları silmelisiniz.";
        return RedirectToAction(nameof(Delete), new { id });
    }

    var salon = await _context.Salonlar.FindAsync(id);
    if (salon == null) return NotFound();

    _context.Salonlar.Remove(salon);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}




    }
}
