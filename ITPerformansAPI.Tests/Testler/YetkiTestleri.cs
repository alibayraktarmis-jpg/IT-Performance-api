using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ITPerformansAPI.Tests.TestOrtami;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class YetkiTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public YetkiTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        [Fact]
        public async Task AnonimIstek_401Doner()
        {
            var client = _fabrika.CreateClient();
            var yanit = await client.GetAsync("/api/Degerlendirmeler");
            Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
        }

        [Fact]
        public async Task Employee_KendiVerisineErisebilir()
        {
            var emp1 = _fabrika.Emp1Client();
            var yanit = await emp1.GetAsync($"/api/Degerlendirmeler/calisan/{VeritabaniFixture.Emp1Id}");
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        }

        [Fact]
        public async Task Employee_BaskasininVerisine_403()
        {
            var emp1 = _fabrika.Emp1Client();
            var yanit = await emp1.GetAsync($"/api/Degerlendirmeler/calisan/{VeritabaniFixture.Emp2Id}");
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Employee_DegerlendirmeOlusturamaz_403()
        {
            var emp1 = _fabrika.Emp1Client();
            var yanit = await emp1.PostAsJsonAsync("/api/Degerlendirmeler", new
            {
                degerlendiricId = VeritabaniFixture.Emp1Id,
                calisanId = VeritabaniFixture.Emp1Id,
                tarih = DateTime.Today,
                donem = "YETKI-EMP",
                yorum = ""
            });
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Evaluator_KendiEkibindekiCalisaniDegerlendirebilir()
        {
            var evA = _fabrika.EvaluatorAClient();
            var yanit = await evA.PostAsJsonAsync("/api/Degerlendirmeler", new
            {
                degerlendiricId = VeritabaniFixture.EvaluatorAId,
                calisanId = VeritabaniFixture.Emp1Id,
                tarih = DateTime.Today,
                donem = "YETKI-EKIP",
                yorum = ""
            });
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        }

        [Fact]
        public async Task Evaluator_EkipDisindakiCalisana_403()
        {
            var evA = _fabrika.EvaluatorAClient();
            var yanit = await evA.PostAsJsonAsync("/api/Degerlendirmeler", new
            {
                degerlendiricId = VeritabaniFixture.EvaluatorAId,
                calisanId = VeritabaniFixture.Emp2Id,
                tarih = DateTime.Today,
                donem = "YETKI-DISARI",
                yorum = ""
            });
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Evaluator_KriterEkleyemez_403()
        {
            var evA = _fabrika.EvaluatorAClient();
            var yanit = await evA.PostAsJsonAsync("/api/AnaBasliklar",
                new { baslik = "Yetkisiz", agirlikYuzdesi = 5, aktifMi = false });
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Employee_DepartmanOzetine_403()
        {
            var emp1 = _fabrika.Emp1Client();
            var yanit = await emp1.GetAsync("/api/Degerlendirmeler/departman-ozet");
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Admin_DepartmanOzetine_200()
        {
            var admin = _fabrika.AdminClient();
            var yanit = await admin.GetAsync("/api/Degerlendirmeler/departman-ozet");
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        }

        [Fact]
        public async Task Evaluator_KullaniciSilemez_403()
        {
            var evA = _fabrika.EvaluatorAClient();
            var yanit = await evA.DeleteAsync($"/api/Kullanicilar/{VeritabaniFixture.Emp1Id}");
            Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
        }

        [Fact]
        public async Task Employee_KullaniciListesinde_SadeceKendiniGorur()
        {
            var emp1 = _fabrika.Emp1Client();
            var json = JsonDocument.Parse(await emp1.GetStringAsync("/api/Kullanicilar"));
            var idler = json.RootElement.EnumerateArray().Select(k => k.GetProperty("id").GetInt32()).ToList();
            Assert.Single(idler);
            Assert.Equal(VeritabaniFixture.Emp1Id, idler[0]);
        }

        [Fact]
        public async Task Evaluator_KullaniciListesinde_SadeceEkibiniGorur()
        {
            var evA = _fabrika.EvaluatorAClient();
            var json = JsonDocument.Parse(await evA.GetStringAsync("/api/Kullanicilar"));
            var idler = json.RootElement.EnumerateArray().Select(k => k.GetProperty("id").GetInt32()).OrderBy(x => x).ToList();
            Assert.Equal(new[] { VeritabaniFixture.Emp1Id, VeritabaniFixture.PasifId }, idler);
        }
    }
}
