using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace yeniWeb.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AdSoyad { get; set; } = null!;


      
        [StringLength(250)]
        public string? UzmanlikAlanlari { get; set; }

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        public ICollection<AntrenorHizmet>? AntrenorHizmetler { get; set; }

  
        public ICollection<AntrenorMusaitlik>? Musaitlikler { get; set; }


        public ICollection<Randevu>? Randevular { get; set; }
    }
}
