namespace ITPerformansAPI.Models
{
    public class DegerlendirmeDetay
    {
        public int Id { get; set; }
        public int DegerlendirmeId { get; set; }
        public int AltKriterId { get; set; }
        public int Puan { get; set; }
        public string? KriterAdi { get; set; }
        public string? AnaBaslikAdi { get; set; }
    }
}