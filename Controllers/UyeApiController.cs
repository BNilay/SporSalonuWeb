using Microsoft.AspNetCore.Mvc;
using yeniWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace yeniWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/uyeler")]
    public class UyeApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UyeApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUyeler()
        {

           var uyeler = await _context.Uyeler
                .OrderBy(u => u.AdSoyad)
                .Select(u => new 
                {
                    u.Id,
                    u.AdSoyad,
                })
                .ToListAsync();
            return Ok(uyeler);
                
        }
    }
}
