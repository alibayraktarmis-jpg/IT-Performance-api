using System.Net;
using System.Net.Mail;

namespace ITPerformansAPI.Helpers
{
    public static class EmailGonderici
    {
        public static void SifreSifirlamaMailiGonder(IConfiguration configuration, string aliciEmail, string link)
        {
            var host = configuration["Smtp:Host"]!;
            var port = int.Parse(configuration["Smtp:Port"]!);
            var kullanici = configuration["Smtp:User"]!;
            var sifre = configuration["Smtp:Password"]!;
            var gonderenAdi = configuration["Smtp:GonderenAdi"] ?? "IT Performans Sistemi";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(kullanici, sifre),
                EnableSsl = true,
            };

            var mesaj = new MailMessage
            {
                From = new MailAddress(kullanici, gonderenAdi),
                Subject = "Şifre Sıfırlama Talebi",
                Body = $"Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın (15 dakika geçerlidir):\n\n{link}\n\nBu talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz.",
                IsBodyHtml = false,
            };
            mesaj.To.Add(aliciEmail);

            client.Send(mesaj);
        }
    }
}
