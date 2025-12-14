using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models.ViewModels
{
    public class AntrenorMusaitlikVM
    {
        public int? Id { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Lütfen bir gün seçiniz.")]
        [Range(0, 6, ErrorMessage = "Gün seçimi geçersiz.")]
        public int? Gun { get; set; }   // ✅ int? yaptık

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }
    }
}
