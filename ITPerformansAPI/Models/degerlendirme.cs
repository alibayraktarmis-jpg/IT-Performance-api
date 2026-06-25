namespace ITPerformansAPI.Models
{
    public class Degerlendirme
    {
        public int Id { get; set; }
        public int DegerlendiriciId { get; set; }

        public int CalisanId { get; set; }

        public int KriterId { get; set; }
        public int Puan { get; set; }
        public required string Yorum { get; set; }
    }
}