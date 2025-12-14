using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Data;
using yeniWeb.Models;
using yeniWeb.Models.ViewModels;

namespace yeniWeb.Controllers
{
    public class AntrenorMusaitlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorMusaitlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateGunlerDropDown(object? selected = null)
        {
            var gunler = new List<SelectListItem>
            {
                new("Pazar", "0"),
                new("Pazartesi", "1"),
                new("Salı", "2"),
                new("Çarşamba", "3"),
                new("Perşembe", "4"),
                new("Cuma", "5"),
                new("Cumartesi", "6"),
            };

            ViewBag.Gunler = new SelectList(gunler, "Value", "Text", selected);
        }

        // /AntrenorMusaitlik?antrenorId=1
        public async Task<IActionResult> Index(int antrenorId)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == antrenorId);

            if (antrenor == null) return NotFound();

            ViewBag.Antrenor = antrenor;

            var list = await _context.AntrenorMusaitlikler
                .Where(m => m.AntrenorId == antrenorId)
                .OrderBy(m => (int)m.Gun)
                .ThenBy(m => m.BaslangicSaati)
                .ToListAsync();

            return View(list);
        }

        // GET: /AntrenorMusaitlik/Create?antrenorId=1
        public IActionResult Create(int antrenorId)
        {
            PopulateGunlerDropDown();

            var vm = new AntrenorMusaitlikVM
            {
                AntrenorId = antrenorId,
                Gun = null, // kullanıcı seçecek
                BaslangicSaati = new TimeSpan(9, 0, 0),
                BitisSaati = new TimeSpan(18, 0, 0),
            };

            return View(vm);
        }


        // POST: /AntrenorMusaitlik/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntrenorMusaitlikVM vm)
        {
            if (vm.BitisSaati <= vm.BaslangicSaati)
                ModelState.AddModelError("", "Bitiş saati başlangıç saatinden büyük olmalıdır.");

            if (vm.Gun == null)
                ModelState.AddModelError("Gun", "Lütfen bir gün seçiniz.");

            if (vm.Gun != null)
            {
                bool overlap = await _context.AntrenorMusaitlikler.AnyAsync(m =>
                    m.AntrenorId == vm.AntrenorId &&
                    m.Gun == (DayOfWeek)vm.Gun.Value &&
                    m.BaslangicSaati < vm.BitisSaati &&
                    m.BitisSaati > vm.BaslangicSaati
                );

                if (overlap)
                    ModelState.AddModelError("", "Bu gün/saat aralığı mevcut bir müsaitlikle çakışıyor.");
            }

            if (!ModelState.IsValid)
            {
                PopulateGunlerDropDown(vm.Gun);
                return View(vm);
            }

            var entity = new AntrenorMusaitlik
            {
                AntrenorId = vm.AntrenorId,
                Gun = (DayOfWeek)vm.Gun!.Value, // ✅ int -> DayOfWeek
                BaslangicSaati = vm.BaslangicSaati,
                BitisSaati = vm.BitisSaati
            };

            _context.AntrenorMusaitlikler.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { antrenorId = vm.AntrenorId });
        }

        // GET: /AntrenorMusaitlik/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (m == null) return NotFound();

            PopulateGunlerDropDown((int)m.Gun);

            var vm = new AntrenorMusaitlikVM
            {
                Id = m.Id,
                AntrenorId = m.AntrenorId,
                Gun = (int)m.Gun, // ✅ DayOfWeek -> int
                BaslangicSaati = m.BaslangicSaati,
                BitisSaati = m.BitisSaati
            };

            return View(vm);
        }

        // POST: /AntrenorMusaitlik/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AntrenorMusaitlikVM vm)
        {
            if (vm.Id == null || id != vm.Id.Value) return NotFound();

            if (vm.BitisSaati <= vm.BaslangicSaati)
                ModelState.AddModelError("", "Bitiş saati başlangıç saatinden büyük olmalıdır.");

            if (vm.Gun == null)
                ModelState.AddModelError("Gun", "Lütfen bir gün seçiniz.");

            if (vm.Gun != null)
            {
                bool overlap = await _context.AntrenorMusaitlikler.AnyAsync(m =>
                    m.Id != id &&
                    m.AntrenorId == vm.AntrenorId &&
                    m.Gun == (DayOfWeek)vm.Gun.Value &&
                    m.BaslangicSaati < vm.BitisSaati &&
                    m.BitisSaati > vm.BaslangicSaati
                );

                if (overlap)
                    ModelState.AddModelError("", "Bu gün/saat aralığı mevcut bir müsaitlikle çakışıyor.");
            }

            if (!ModelState.IsValid)
            {
                PopulateGunlerDropDown(vm.Gun);
                return View(vm);
            }

            var entity = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Gun = (DayOfWeek)vm.Gun!.Value; // ✅ int -> DayOfWeek
            entity.BaslangicSaati = vm.BaslangicSaati;
            entity.BitisSaati = vm.BitisSaati;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { antrenorId = vm.AntrenorId });
        }

        // GET: /AntrenorMusaitlik/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.AntrenorMusaitlikler
                .Include(x => x.Antrenor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null) return NotFound();
            return View(m);
        }

        // POST: /AntrenorMusaitlik/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var m = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (m == null) return NotFound();

            int antrenorId = m.AntrenorId;

            _context.AntrenorMusaitlikler.Remove(m);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { antrenorId });
        }


    }
}
