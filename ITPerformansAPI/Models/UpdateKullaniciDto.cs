namespace ITPerformansAPI.Models
{
    public class UpdateKullaniciDto
    {
        public required string Ad { get; set; }
        public required string Soyad { get; set; }
        public required string Email { get; set; }
        public required string Rol { get; set; }
        public required string Departman { get; set; }
        public int? EvaluatorId { get; set; }
    }
}
