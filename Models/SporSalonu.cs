using System.ComponentModel.DataAnnotations;

namespace webProje.Models
{
	public class SporSalonu
	{
		[Key]
		public int SalonId { get; set; }
		public string SalonAdi { get; set; }

		public string Hizmetler { get; set; }

		public string CalismaSaatleri { get; set; }

	}
}
