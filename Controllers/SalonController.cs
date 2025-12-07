using Microsoft.AspNetCore.Mvc;

namespace webProje.Controllers
{
	public class SalonController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
