using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models.ViewModels
{
    public class PlanRequestVm
    {
        [Range(10, 90)]
        public int Age { get; set; }

        [Required]
        public string Sex { get; set; } = "";

        [Range(120, 220)]
        public int HeightCm { get; set; }

        [Range(30, 250)]
        public int WeightKg { get; set; }

        [Required]
        public string Goal { get; set; } = "";

        [Range(1, 7)]
        public int DaysPerWeek { get; set; } = 3;

        public string? Equipment { get; set; }
        public string? Notes { get; set; }

        public IFormFile? Photo { get; set; }


    }
}
