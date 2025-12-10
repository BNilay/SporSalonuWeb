using System.ComponentModel.DataAnnotations;
namespace yeniWeb.Models
{
    public class AntrenorMusaitlik
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; } = null!;

        [Required]
        public DayOfWeek Gun { get; set; }  

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }
    }
}
