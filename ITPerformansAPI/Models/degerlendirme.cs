namespace ITPerformansAPI.Models
{
    public class Degerlendirme
    {
        public int Id { get; set; }
        public int DegerlendiricId { get; set; }
        public int CalisanId { get; set; }
        public DateTime Tarih { get; set; }
        public string Donem { get; set; } = "";
        public string Yorum { get; set; } = "";
        public float ToplamSkor { get; set; }
    }
}