namespace ITPerformansAPI.Models
{
    public class AnaBaslik
    {
        public int Id { get; set; }
        public required string Baslik { get; set; }
        public int AgirlikYuzdesi { get; set; }
        public bool AktifMi { get; set; }
    }
}