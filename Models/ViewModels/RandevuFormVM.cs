using System.ComponentModel.DataAnnotations;

namespace yeniWeb.Models.ViewModels
{
    public class RandevuFormVM
    {
        public int? Id { get; set; }

        [Required]
        public int UyeId { get; set; }

        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }


      //  public decimal Ucret { get; set; }
    }
}
