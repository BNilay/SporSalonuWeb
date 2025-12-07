using System.ComponentModel.DataAnnotations;

namespace webProje.Models
{
	public class Uye
	{
		[Key]
		public int UyeId { get; set; }

		public string UyeAd { get; set; }

		public string UyeSoyad { get; set; }
	}
}
