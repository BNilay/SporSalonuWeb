using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models
{
    public class Hizmet
    {
       
        public int HizmetId { get; set; }

        [Required]
        [StringLength(100)]
        public string HizmetAdi { get; set; }

        [Required]
        [Range(1,10)]
        public decimal Fiyat { get; set; }

        [Required]
        [Range(1,5)]
        public int HizmetSuresi { get; set; } //saat

        public int SalonId { get; set; }
        public Salon Salon { get; set; }

        public ICollection<AntrenorHizmet>? AntrenorHizmetler { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
