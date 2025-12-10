namespace yeniWeb.Models
{
    public class AntrenorHizmet
    {
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; } = null!;

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; } = null!;
    }
}
