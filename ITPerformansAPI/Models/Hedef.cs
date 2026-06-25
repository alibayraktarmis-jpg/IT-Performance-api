namespace ITPerformansAPI.Models
{
    public class Hedef
    {
        public int Id { get; set; }
        public int CalisanId { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public DateTime BitisTarihi { get; set; }
        public bool TamamlandiMi { get; set; } = false;
    }
}