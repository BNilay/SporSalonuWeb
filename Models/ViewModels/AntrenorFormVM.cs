using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models.ViewModels
{
    public class AntrenorFormVM
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AdSoyad { get; set; } = null!;

        [StringLength(250)]
        public string? UzmanlikAlanlari { get; set; }

        [Required]
        public int SalonId { get; set; }

        public List<int> SelectedHizmetIds { get; set; } = new();
        public List<HizmetItemVM> Hizmetler { get; set; } = new();
    }

    public class HizmetItemVM
    {
        public int HizmetId { get; set; }
        public string HizmetAdi { get; set; } = null!;
        public bool Selected { get; set; }
    }
}
