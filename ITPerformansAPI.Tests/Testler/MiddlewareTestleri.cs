using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ITPerformansAPI.Tests.TestOrtami;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class MiddlewareTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public MiddlewareTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        async Task<int> KullaniciOlustur(HttpClient admin, string email)
        {
            var olustur = await admin.PostAsJsonAsync("/api/Kullanicilar", new
            {
                ad = "Gecici",
                soyad = "Kullanici",
                email,
                sifre = "Gecici123!",
                rol = "Employee",
                departman = "Yazılımcılar",
                evaluatorId = (int?)null
            });
            Assert.Equal(HttpStatusCode.OK, olustur.StatusCode);

            var json = JsonDocument.Parse(await admin.GetStringAsync("/api/Kullanicilar"));
            return json.RootElement.EnumerateArray()
                .First(k => k.GetProperty("email").GetString() == email)
                .GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task PasifeAlinanKullanicinin_EskiTokeni_401Olur()
        {
            var admin = _fabrika.AdminClient();
            var id = await KullaniciOlustur(admin, "middleware-pasif@test.local");

            var kullaniciClient = _fabrika.YetkiliClient(id, "middleware-pasif@test.local", "Employee");
            var oncesi = await kullaniciClient.GetAsync("/api/Hedefler");
            Assert.Equal(HttpStatusCode.OK, oncesi.StatusCode);

            var pasifYap = await admin.PatchAsync($"/api/Kullanicilar/{id}/aktif", JsonContent.Create(false));
            Assert.Equal(HttpStatusCode.OK, pasifYap.StatusCode);

            var sonrasi = await kullaniciClient.GetAsync("/api/Hedefler");
            Assert.Equal(HttpStatusCode.Unauthorized, sonrasi.StatusCode);
        }

        [Fact]
        public async Task RoluDegisenKullanicinin_EskiTokeni_401Olur()
        {
            var admin = _fabrika.AdminClient();
            var id = await KullaniciOlustur(admin, "middleware-rol@test.local");

            var kullaniciClient = _fabrika.YetkiliClient(id, "middleware-rol@test.local", "Employee");
            var oncesi = await kullaniciClient.GetAsync("/api/Hedefler");
            Assert.Equal(HttpStatusCode.OK, oncesi.StatusCode);

            var rolDegistir = await admin.PutAsJsonAsync($"/api/Kullanicilar/{id}", new
            {
                ad = "Gecici",
                soyad = "Kullanici",
                email = "middleware-rol@test.local",
                rol = "Evaluator",
                departman = "Yazılımcılar",
                evaluatorId = (int?)null
            });
            Assert.Equal(HttpStatusCode.OK, rolDegistir.StatusCode);

            var sonrasi = await kullaniciClient.GetAsync("/api/Hedefler");
            Assert.Equal(HttpStatusCode.Unauthorized, sonrasi.StatusCode);
        }
    }
}
