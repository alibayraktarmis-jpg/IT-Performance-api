namespace ITPerformansAPI.Models
{
    public class UpdateDetaylarDto
    {
        public string Yorum { get; set; } = "";
        public double ToplamSkor { get; set; }
        public List<DetayItem> Detaylar { get; set; } = new();
    }

    public class DetayItem
    {
        public int AltKriterId { get; set; }
        public int Puan { get; set; }
    }
}
