using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Data;
using yeniWeb.Models;

namespace yeniWeb.Controllers
{
    [ApiController]
    [Route("api/randevular")]

    public class RandevuApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRandevular() 
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.RandevuTarihi)
                .Select(r => new
                {
                    r.Uye.AdSoyad,
                    r.Id,
                    r.RandevuTarihi,
                    r.SureDakika,
                    r.Ucret,
                    Durum = r.Durum.ToString(),
                    Uye = r.Uye.AdSoyad,
                    Antrenor = r.Antrenor.AdSoyad,
                    Hizmet = r.Hizmet.HizmetAdi
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}
