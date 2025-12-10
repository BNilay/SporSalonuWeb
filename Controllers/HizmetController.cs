using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using yeniWeb.Data;
using yeniWeb.Models;

namespace yeniWeb.Controllers

{
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateSalonDropDownList(object? selectedSalon = null) 
        {
            var salonlar = _context.Salonlar
                .OrderBy(s => s.SalonAdi)
                .ToList();
            ViewBag.SalonId = new SelectList(salonlar, "SalonId", "SalonAdi", selectedSalon);
        }

            public async Task<IActionResult> Index()
            {
                var hizmetler = await _context.Hizmetler
                    .Include(h => h.Salon)
                    .ToListAsync();
                return View(hizmetler);
            }

        public async Task<IActionResult> Details(int id)
        {
            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(h => h.HizmetId == id);

            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }
        public IActionResult Create()
        {
            PopulateSalonDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                // HATALARI TOPLA
                var errors = ModelState
                    .SelectMany(kvp => kvp.Value.Errors
                        .Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
                    .ToList();

                System.Diagnostics.Debug.WriteLine("MODELSTATE ERRORS:");
                foreach (var err in errors)
                {
                    System.Diagnostics.Debug.WriteLine(err);
                }

                PopulateSalonDropDownList(hizmet.SalonId);
                return View(hizmet);
            }

            _context.Hizmetler.Add(hizmet);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id) 
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if(hizmet == null)
                return NotFound();

            PopulateSalonDropDownList(hizmet.SalonId);
            return View(hizmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Edit(int id ,Hizmet hizmet)
        {
            if(id != hizmet.HizmetId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateSalonDropDownList(hizmet.SalonId);
                return View(hizmet);
            }
            try
            {
                _context.Update(hizmet);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) 
            {
                bool exists = _context.Hizmetler.Any(h => h.HizmetId == hizmet.HizmetId);
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
               
            }
            return RedirectToAction("Index");

        }

        public async Task<IActionResult>Delete(int id) 
        {
            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(h => h.HizmetId == id);

            if(hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound();
            }

            _context.Hizmetler.Remove(hizmet);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }
    }
}

