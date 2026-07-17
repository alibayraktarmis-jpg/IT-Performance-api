using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ITPerformansAPI.Tests.TestOrtami;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class KullaniciGirisTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public KullaniciGirisTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        [Fact]
        public async Task DogruSifreyleGiris_TokenDoner()
        {
            var client = _fabrika.CreateClient();
            var yanit = await client.PostAsJsonAsync("/api/Kullanicilar/login",
                new { email = "emp1@test.local", sifre = VeritabaniFixture.OrtakSifre });
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
            var json = JsonDocument.Parse(await yanit.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("token").GetString()));
            Assert.Equal("Employee", json.RootElement.GetProperty("rol").GetString());
        }

        [Fact]
        public async Task YanlisSifreyleGiris_401()
        {
            var client = _fabrika.CreateClient();
            var yanit = await client.PostAsJsonAsync("/api/Kullanicilar/login",
                new { email = "emp1@test.local", sifre = "tamamen-yanlis" });
            Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        }

        [Fact]
        public async Task PasifKullaniciGiremez_401()
        {
            var client = _fabrika.CreateClient();
            var yanit = await client.PostAsJsonAsync("/api/Kullanicilar/login",
                new { email = "pasif@test.local", sifre = VeritabaniFixture.OrtakSifre });
            Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        }

        [Fact]
        public async Task AyniEmailIleIkinciKullanici_400VeAciklayiciMesaj()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/Kullanicilar", new
            {
                ad = "Kopya",
                soyad = "Kullanici",
                email = "emp1@test.local",
                sifre = "Sifre123!",
                rol = "Employee",
                departman = "Yazılımcılar",
                evaluatorId = (int?)null
            });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
            var govde = await yanit.Content.ReadAsStringAsync();
            Assert.Contains("zaten kullaniliyor", govde);
        }

        [Fact]
        public async Task SifreDegistir_YanlisMevcutSifre_400()
        {
            var emp2 = _fabrika.Emp2Client();
            var yanit = await emp2.PutAsJsonAsync("/api/Kullanicilar/sifre-degistir",
                new { mevcutSifre = "yanlis-sifre", yeniSifre = "Yeni12345!" });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task SifreDegistir_DogruMevcutSifre_YeniSifreyleGirilir()
        {
            var emp2 = _fabrika.Emp2Client();
            var degistir = await emp2.PutAsJsonAsync("/api/Kullanicilar/sifre-degistir",
                new { mevcutSifre = VeritabaniFixture.OrtakSifre, yeniSifre = "YepyeniSifre1!" });
            Assert.Equal(HttpStatusCode.OK, degistir.StatusCode);

            var client = _fabrika.CreateClient();
            var giris = await client.PostAsJsonAsync("/api/Kullanicilar/login",
                new { email = "emp2@test.local", sifre = "YepyeniSifre1!" });
            Assert.Equal(HttpStatusCode.OK, giris.StatusCode);
        }

        [Fact]
        public async Task KisaSifre_400()
        {
            var emp2 = _fabrika.Emp2Client();
            var yanit = await emp2.PutAsJsonAsync("/api/Kullanicilar/sifre-degistir",
                new { mevcutSifre = VeritabaniFixture.OrtakSifre, yeniSifre = "123" });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }
    }
}
