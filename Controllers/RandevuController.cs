using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Data;
using yeniWeb.Models;
using yeniWeb.Models.ViewModels;

namespace yeniWeb.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserDetails> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<UserDetails> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task PopulateDropDowns(int? selectedUyeId = null, int? selectedAntrenorId = null, int? selectedHizmetId = null)
        {
            var uyeler = await _context.Uyeler
                .OrderBy(u => u.AdSoyad)
                .ToListAsync();
            ViewBag.Uyeler = new SelectList(uyeler, "Id", "AdSoyad", selectedUyeId);

            var antrenorler = await _context.Antrenorler
                .OrderBy(a => a.AdSoyad)
                .ToListAsync();
            ViewBag.Antrenorler = new SelectList(antrenorler, "Id", "AdSoyad", selectedAntrenorId);

            var hizmetler = await _context.Hizmetler
                .OrderBy(h => h.HizmetAdi)
                .ToListAsync();
            ViewBag.Hizmetler = new SelectList(hizmetler, "HizmetId", "HizmetAdi", selectedHizmetId);
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var all = await _context.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .OrderByDescending(r => r.RandevuTarihi)
                    .ToListAsync();

                return View(all);
            }

            var userId = _userManager.GetUserId(User);
            var uye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UserId == userId);
            if (uye == null) return Forbid();

            var list = await _context.Randevular
                .Where(r => r.UyeId == uye.Id)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            return View(list);
        }

       
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
                return Forbid(); 

            var userId = _userManager.GetUserId(User);
            var uye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UserId == userId);
            if (uye == null) return Forbid();

            await PopulateDropDowns(selectedUyeId: null, selectedAntrenorId: null, selectedHizmetId: null);

            var vm = new RandevuFormVM
            {
                Tarih = DateTime.Today,
                BaslangicSaati = new TimeSpan(10, 0, 0),
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuFormVM vm)
        {
            if (User.IsInRole("Admin"))
                return Forbid(); 

            var userId = _userManager.GetUserId(User);
            var uye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UserId == userId);
            if (uye == null) return Forbid();

            await PopulateDropDowns(vm.UyeId, vm.AntrenorId, vm.HizmetId);

            if (!ModelState.IsValid)
                return View(vm);

            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(h => h.HizmetId == vm.HizmetId);
            if (hizmet == null)
            {
                ModelState.AddModelError("HizmetId", "Seçilen hizmet bulunamadı.");
                return View(vm);
            }

            int sureDakika = hizmet.HizmetSuresi;
            decimal ucret = hizmet.Fiyat;

            var baslangic = vm.Tarih.Date.Add(vm.BaslangicSaati);
            var bitis = baslangic.AddMinutes(sureDakika);
            var gun = baslangic.DayOfWeek;

           
            bool uygunMusaitlikVar = await _context.AntrenorMusaitlikler.AnyAsync(m =>
                m.AntrenorId == vm.AntrenorId &&
                m.Gun == gun &&
                m.BaslangicSaati <= baslangic.TimeOfDay &&
                m.BitisSaati >= bitis.TimeOfDay
            );

            if (!uygunMusaitlikVar)
            {
                ModelState.AddModelError("", "Seçilen antrenör bu gün/saat aralığında müsait değil.");
                return View(vm);
            }

            bool cakisma = await _context.Randevular.AnyAsync(r =>
                r.AntrenorId == vm.AntrenorId &&
                r.Durum != RandevuDurumu.Iptal &&
                r.RandevuTarihi < bitis &&
                r.RandevuTarihi.AddMinutes(r.SureDakika) > baslangic
            );

            if (cakisma)
            {
                ModelState.AddModelError("", "Bu saat aralığında antrenörün başka bir randevusu var.");
                return View(vm);
            }

            var randevu = new Randevu
            {
                UyeId = uye.Id,
                AntrenorId = vm.AntrenorId,
                HizmetId = vm.HizmetId,
                RandevuTarihi = baslangic,
                SureDakika = sureDakika,
                Ucret = ucret,
                Durum = RandevuDurumu.Beklemede,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        
        // -----------------------------
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.Randevular
                .Include(x => x.Uye)
                .Include(x => x.Antrenor)
                .Include(x => x.Hizmet)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var uye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UserId == userId);
                if (uye == null) return Forbid();

                if (r.UyeId != uye.Id) return Forbid();
            }

            return View(r);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var r = await _context.Randevular.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var uye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UserId == userId);
                if (uye == null) return Forbid();

                if (r.UyeId != uye.Id) return Forbid();
            }

            _context.Randevular.Remove(r);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == id);
            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminIptal(int id)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == id);
            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Iptal;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
