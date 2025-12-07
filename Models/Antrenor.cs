using System.ComponentModel.DataAnnotations;

namespace webProje.Models
{
	public class Antrenor
	{
		[Key]
		public int AntrenorId { get; set; }
		public string AntrenorAd { get; set; }
		public string AntrenorSoyad { get; set; }
		public string UzmanlikAlani { get; set; }

		public string CalismaSaatleri { get; set; }
	}
}
