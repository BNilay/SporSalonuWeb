using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using yeniWeb.Models.ViewModels;
using yeniWeb.Services;

namespace yeniWeb.Controllers
{
   

    public class AiController : Controller
    {
        private readonly GeminiPlanService _geminiPlanService;

        public AiController(GeminiPlanService geminiPlanService)
        {
            _geminiPlanService = geminiPlanService;
        }

        [HttpGet]
        public IActionResult Plan()
        {
            return View(new PlanRequestVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Plan(PlanRequestVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var resultJson = await _geminiPlanService.GeneratePlanAsync(model, ct);
                ViewBag.ResultJson = resultJson;
                return View("PlanResult");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("PlanResult");
            }
        }
    }
}
