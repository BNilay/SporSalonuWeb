using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using yeniWeb.Data;
using yeniWeb.Models;
using yeniWeb.Models.ViewModels;

namespace yeniWeb.Controllers
{
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateSalonDropDown(object? selected = null)
        {
            var salonlar = _context.Salonlar.OrderBy(s => s.SalonAdi).ToList();
            ViewBag.SalonId = new SelectList(salonlar, "SalonId", "SalonAdi", selected);
        }

        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .ToListAsync();

            return View(antrenorler);
        }
        public async Task<IActionResult> Details(int id) 
        {
            var antrenor = await _context.Antrenorler
                .Include(a=> a.Salon)
                .Include(a=> a.AntrenorHizmetler!)
                .ThenInclude(ah => ah.Hizmet)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();
            return View(antrenor);

        }

        public async Task<IActionResult> Create() 
        {
            PopulateSalonDropDown();

            var vm = new AntrenorFormVM
            {
                Hizmetler = await _context.Hizmetler
                .OrderBy(h=> h.HizmetAdi)
                .Select(h => new HizmetItemVM
                {
                    HizmetId = h.HizmetId,
                    HizmetAdi = h.HizmetAdi,
                    Selected = false
                }
                ).ToListAsync()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntrenorFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateSalonDropDown(vm.SalonId);

                // Checkbox listesi yeniden doldurulmalı
                var all = await _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
                vm.Hizmetler = all.Select(h => new HizmetItemVM
                {
                    HizmetId = h.HizmetId,
                    HizmetAdi = h.HizmetAdi,
                    Selected = vm.SelectedHizmetIds.Contains(h.HizmetId)
                }).ToList();

                return View(vm);
            }

            var antrenor = new Antrenor
            {
                AdSoyad = vm.AdSoyad,
                UzmanlikAlanlari = vm.UzmanlikAlanlari,
                SalonId = vm.SalonId
            };

            _context.Antrenorler.Add(antrenor);
            await _context.SaveChangesAsync();

            // Many-to-many kayıtları
            if (vm.SelectedHizmetIds.Any())
            {
                foreach (var hizmetId in vm.SelectedHizmetIds.Distinct())
                {
                    _context.AntrenorHizmetler.Add(new AntrenorHizmet
                    {
                        AntrenorId = antrenor.Id,
                        HizmetId = hizmetId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Antrenor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            PopulateSalonDropDown(antrenor.SalonId);

            var selected = antrenor.AntrenorHizmetler?
                .Select(x => x.HizmetId)
                .ToHashSet() ?? new HashSet<int>();

            var hizmetler = await _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();

            var vm = new AntrenorFormVM
            {
                Id = antrenor.Id,
                AdSoyad = antrenor.AdSoyad,
                UzmanlikAlanlari = antrenor.UzmanlikAlanlari,
                SalonId = antrenor.SalonId,
                SelectedHizmetIds = selected.ToList(),
                Hizmetler = hizmetler.Select(h => new HizmetItemVM
                {
                    HizmetId = h.HizmetId,
                    HizmetAdi = h.HizmetAdi,
                    Selected = selected.Contains(h.HizmetId)
                }).ToList()
            };

            return View(vm);
        }

        // POST: /Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AntrenorFormVM vm)
        {
            if (vm.Id == null || id != vm.Id.Value) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateSalonDropDown(vm.SalonId);

                var all = await _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
                vm.Hizmetler = all.Select(h => new HizmetItemVM
                {
                    HizmetId = h.HizmetId,
                    HizmetAdi = h.HizmetAdi,
                    Selected = vm.SelectedHizmetIds.Contains(h.HizmetId)
                }).ToList();

                return View(vm);
            }

            antrenor.AdSoyad = vm.AdSoyad;
            antrenor.UzmanlikAlanlari = vm.UzmanlikAlanlari;
            antrenor.SalonId = vm.SalonId;

            // Hizmet eşlemesini güncelle
            var old = antrenor.AntrenorHizmetler ?? new List<AntrenorHizmet>();
            _context.AntrenorHizmetler.RemoveRange(old);

            if (vm.SelectedHizmetIds.Any())
            {
                foreach (var hizmetId in vm.SelectedHizmetIds.Distinct())
                {
                    _context.AntrenorHizmetler.Add(new AntrenorHizmet
                    {
                        AntrenorId = antrenor.Id,
                        HizmetId = hizmetId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Antrenor/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // POST: /Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            // önce ilişkiyi sil
            if (antrenor.AntrenorHizmetler != null && antrenor.AntrenorHizmetler.Any())
            {
                _context.AntrenorHizmetler.RemoveRange(antrenor.AntrenorHizmetler);
            }

            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
