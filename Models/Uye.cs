using System.ComponentModel.DataAnnotations;
namespace yeniWeb.Models
{
    public class Uye
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public UserDetails User { get; set; } = null!;

        [StringLength(100)]
        public string? AdSoyad { get; set; }

        [StringLength(50)]
        public string? Telefon { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
