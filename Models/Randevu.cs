namespace webProje.Models
{
	public class Randevu
	{
		public int RandevuId { get; set; }

		public int UyeId { get; set; }
		public Uye Uye { get; set; }

		public int AntrenorId { get; set; }
		public Antrenor Antrenor { get; set; }

		public DateTime RandevuTarihi { get; set; }
		public string Saat { get; set; }

		public Hizmet Hizmet { get; set; }
	}

}
