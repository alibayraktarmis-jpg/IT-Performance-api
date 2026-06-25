namespace ITPerformansAPI.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public required string Ad { get; set; }

        public required string Soyad { get; set; }

        public required string Email { get; set; }

        public required string Sifre { get; set; }

        public required string Rol { get; set; }

        public required string Departman { get; set; }

        public bool AktifMi { get; set; } = true;

    }
}