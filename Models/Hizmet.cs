using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace yeniWeb.Models
{
    public class Hizmet
    {
        public int HizmetId { get; set; }

        [Required]
        [StringLength(100)]
        public string HizmetAdi { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "1", "100000", ErrorMessage = "Fiyat 1 ile 100000 arasında olmalıdır.")]
        public decimal Fiyat { get; set; }

        [Required]
        [Range(1, 600, ErrorMessage = "Hizmet süresi 1 ile 600 arasında olmalıdır.")]
        public int HizmetSuresi { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir salon seçiniz.")]
        public int SalonId { get; set; }

        public Salon? Salon { get; set; }

        public ICollection<AntrenorHizmet>? AntrenorHizmetler { get; set; }
        public ICollection<Randevu>? Randevular { get; set; }
    }
}
