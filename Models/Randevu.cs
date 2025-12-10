using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models
{
    public enum RandevuDurumu
    {
        Beklemede = 0,
        Onaylandi = 1,
        Iptal = 2
    }

    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int UyeId { get; set; }
        public Uye Uye { get; set; } = null!;

        [Required]
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; } = null!;

        [Required]
        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; } = null!;

        [Required]
        public DateTime RandevuTarihi { get; set; }  

        [Range(1,5)]
        public int SureDakika { get; set; }  // saat

        [Column(TypeName = "decimal(10,2)")]
        public decimal Ucret { get; set; }  

        [Required]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}
