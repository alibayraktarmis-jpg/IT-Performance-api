using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using ITPerformansAPI.Tests.TestOrtami;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class HedefVeKuralTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public HedefVeKuralTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        [Fact]
        public async Task GecmisTarihliHedef_400()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/Hedefler", new
            {
                calisanId = VeritabaniFixture.Emp1Id,
                aciklama = "gecmis tarih",
                bitisTarihi = DateTime.Today.AddDays(-1)
            });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task BosAciklamaliHedef_400()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/Hedefler", new
            {
                calisanId = VeritabaniFixture.Emp1Id,
                aciklama = "   ",
                bitisTarihi = DateTime.Today.AddDays(7)
            });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task GecerliHedef_200()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/Hedefler", new
            {
                calisanId = VeritabaniFixture.Emp1Id,
                aciklama = "Gecerli test hedefi",
                bitisTarihi = DateTime.Today.AddDays(7)
            });
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        }

        [Fact]
        public async Task AdminRolundekiKullaniciyaHedefAtanamaz_400()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/Hedefler", new
            {
                calisanId = VeritabaniFixture.AdminId,
                aciklama = "Admin'e hedef",
                bitisTarihi = DateTime.Today.AddDays(7)
            });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task Evaluator_EkipDisinaHedefAtayamaz_403()
        {
            var evA = _fabrika.EvaluatorAClient();
            var yanit = await evA.PostAsJsonAsync("/api/Hedefler", new
            {
                calisanId = VeritabaniFixture.Emp2Id,
                aciklama = "Ekip disi hedef",
                bitisTarihi = DateTime.Today.AddDays(7)
            });
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task AyniCalisanAyniDonem_IkinciDegerlendirme_409()
        {
            var admin = _fabrika.AdminClient();
            var govde = new
            {
                degerlendiricId = VeritabaniFixture.AdminId,
                calisanId = VeritabaniFixture.Emp1Id,
                tarih = DateTime.Today,
                donem = "KURAL-CAKISMA",
                yorum = ""
            };
            var ilk = await admin.PostAsJsonAsync("/api/Degerlendirmeler", govde);
            Assert.Equal(HttpStatusCode.OK, ilk.StatusCode);

            var ikinci = await admin.PostAsJsonAsync("/api/Degerlendirmeler", govde);
            Assert.Equal(HttpStatusCode.Conflict, ikinci.StatusCode);
        }

        [Fact]
        public async Task KullaniciSilinince_DegerlendirmeleriDeCascadeSilinir()
        {
            var admin = _fabrika.AdminClient();
            var olustur = await admin.PostAsJsonAsync("/api/Kullanicilar", new
            {
                ad = "Cascade",
                soyad = "Test",
                email = "cascade@test.local",
                sifre = "Cascade123!",
                rol = "Employee",
                departman = "Yazılımcılar",
                evaluatorId = (int?)null
            });
            Assert.Equal(HttpStatusCode.OK, olustur.StatusCode);

            var json = JsonDocument.Parse(await admin.GetStringAsync("/api/Kullanicilar"));
            var id = json.RootElement.EnumerateArray()
                .First(k => k.GetProperty("email").GetString() == "cascade@test.local")
                .GetProperty("id").GetInt32();

            var deg = await admin.PostAsJsonAsync("/api/Degerlendirmeler", new
            {
                degerlendiricId = VeritabaniFixture.AdminId,
                calisanId = id,
                tarih = DateTime.Today,
                donem = "KURAL-CASCADE",
                yorum = ""
            });
            Assert.Equal(HttpStatusCode.OK, deg.StatusCode);

            var sil = await admin.DeleteAsync($"/api/Kullanicilar/{id}");
            Assert.Equal(HttpStatusCode.OK, sil.StatusCode);

            using var baglanti = new SqlConnection(VeritabaniFixture.BaglantiDizesi);
            var kalan = await baglanti.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Degerlendirmeler WHERE CalisanId = @Id", new { Id = id });
            Assert.Equal(0, kalan);
        }

        [Fact]
        public async Task AktifAgirlikToplami100uAsamaz_400()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/AnaBasliklar",
                new { baslik = "Fazla Agirlik", agirlikYuzdesi = 10, aktifMi = true });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task NegatifAgirlik_400()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.PostAsJsonAsync("/api/AnaBasliklar",
                new { baslik = "Negatif", agirlikYuzdesi = -5, aktifMi = false });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }
    }
}
