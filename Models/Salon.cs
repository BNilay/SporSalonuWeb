using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models
{
    public class Salon
    {
        public int SalonId { get; set; }

        [Required]
        [StringLength(100)]
        public string SalonAdi { get; set; }

        [Required]
        [StringLength(100)]
        public string CalismaSaatleri { get; set; }

        public ICollection<Hizmet>? Hizmetler { get; set; }
        public ICollection<Antrenor>? Antrenorler { get; set; }


    }
}
