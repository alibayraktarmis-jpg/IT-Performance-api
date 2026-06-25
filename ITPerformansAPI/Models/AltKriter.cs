namespace ITPerformansAPI.Models
{
    public class AltKriter
    {
        public int Id { get; set; }
        public int AnaBaslikId { get; set; }
        public string KriterAdi { get; set; } = string.Empty;
        public bool AktifMi { get; set; } = true;
    }
}