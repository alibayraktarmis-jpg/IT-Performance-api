using System.Net;
using System.Net.Http.Json;
using ITPerformansAPI.Tests.TestOrtami;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class RateLimitTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public RateLimitTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        [Fact]
        public async Task Login_BesDenemedenSonra_429()
        {
            var client = _fabrika.CreateClient();
            var kodlar = new List<HttpStatusCode>();

            for (int i = 0; i < 6; i++)
            {
                var yanit = await client.PostAsJsonAsync("/api/Kullanicilar/login",
                    new { email = "saldirgan@test.local", sifre = "deneme" });
                kodlar.Add(yanit.StatusCode);
            }

            Assert.All(kodlar.Take(5), k => Assert.NotEqual(HttpStatusCode.TooManyRequests, k));
            Assert.Equal(HttpStatusCode.TooManyRequests, kodlar[5]);
        }
    }
}
