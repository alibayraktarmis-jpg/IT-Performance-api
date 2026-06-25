namespace ITPerformansAPI.Models
{
    public class KriterAciklama
    {
        public int Id { get; set; }
        public int AltKriterId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}