using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ITPerformansAPI.Tests.TestOrtami;

namespace ITPerformansAPI.Tests.Testler
{
    [Collection("Api")]
    public class SkorTestleri : IClassFixture<ApiFactory>
    {
        readonly ApiFactory _fabrika;

        public SkorTestleri(ApiFactory fabrika) => _fabrika = fabrika;

        static async Task<int> DegerlendirmeOlustur(HttpClient client, int calisanId, string donem)
        {
            var yanit = await client.PostAsJsonAsync("/api/Degerlendirmeler", new
            {
                degerlendiricId = VeritabaniFixture.AdminId,
                calisanId,
                tarih = DateTime.Today,
                donem,
                yorum = "test"
            });
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
            var json = JsonDocument.Parse(await yanit.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("id").GetInt32();
        }

        static async Task<double> DetaylariKaydet(HttpClient client, int degerlendirmeId, object[] detaylar)
        {
            var yanit = await client.PutAsJsonAsync($"/api/Degerlendirmeler/{degerlendirmeId}/detaylar",
                new { yorum = "test", detaylar });
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
            var json = JsonDocument.Parse(await yanit.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("toplamSkor").GetDouble();
        }

        [Fact]
        public async Task TamPuanlama_Skor100Doner()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-TAM");
            var skor = await DetaylariKaydet(admin, id, new object[]
            {
                new { altKriterId = 1, puan = 5 }, new { altKriterId = 2, puan = 5 },
                new { altKriterId = 3, puan = 5 }, new { altKriterId = 4, puan = 5 },
                new { altKriterId = 5, puan = 5 }, new { altKriterId = 6, puan = 5 }
            });
            Assert.Equal(100, skor);
        }

        [Fact]
        public async Task KismiPuanlama_SadecePuanlananKriterlerinOrtalamasiAlinir()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-KISMI");
            var skor = await DetaylariKaydet(admin, id, new object[]
            {
                new { altKriterId = 1, puan = 5 },
                new { altKriterId = 3, puan = 4 },
                new { altKriterId = 4, puan = 2 }
            });
            Assert.Equal(58, skor);
        }

        [Fact]
        public async Task OrtaPuanlama_Skor60Doner()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-ORTA");
            var skor = await DetaylariKaydet(admin, id, new object[]
            {
                new { altKriterId = 1, puan = 3 }, new { altKriterId = 2, puan = 3 },
                new { altKriterId = 3, puan = 3 }, new { altKriterId = 4, puan = 3 },
                new { altKriterId = 5, puan = 3 }, new { altKriterId = 6, puan = 3 }
            });
            Assert.Equal(60, skor);
        }

        [Fact]
        public async Task GecersizPuan_400Doner()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-GECERSIZ");
            var yanit = await admin.PutAsJsonAsync($"/api/Degerlendirmeler/{id}/detaylar",
                new { yorum = "test", detaylar = new object[] { new { altKriterId = 1, puan = 6 } } });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }

        [Fact]
        public async Task GecersizKriterIleGuncelleme_EskiDetaylarKorunur()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-ROLLBACK");
            await DetaylariKaydet(admin, id, new object[]
            {
                new { altKriterId = 1, puan = 5 },
                new { altKriterId = 3, puan = 4 },
                new { altKriterId = 4, puan = 2 }
            });

            var bozukYanit = await admin.PutAsJsonAsync($"/api/Degerlendirmeler/{id}/detaylar",
                new { yorum = "bozuk", detaylar = new object[]
                {
                    new { altKriterId = 1, puan = 3 },
                    new { altKriterId = 999999, puan = 4 }
                }});
            Assert.NotEqual(HttpStatusCode.OK, bozukYanit.StatusCode);

            var detayYanit = await admin.GetStringAsync($"/api/DegerlendirmeDetaylar/degerlendirme/{id}");
            var detaylar = JsonDocument.Parse(detayYanit).RootElement.EnumerateArray()
                .ToDictionary(d => d.GetProperty("altKriterId").GetInt32(), d => d.GetProperty("puan").GetInt32());

            Assert.Equal(3, detaylar.Count);
            Assert.Equal(5, detaylar[1]);
            Assert.Equal(4, detaylar[3]);
            Assert.Equal(2, detaylar[4]);
        }

        [Fact]
        public async Task DetaylarNull_400Doner()
        {
            var admin = _fabrika.AdminClient();
            var id = await DegerlendirmeOlustur(admin, VeritabaniFixture.Emp1Id, "SKOR-NULL");
            var yanit = await admin.PutAsJsonAsync($"/api/Degerlendirmeler/{id}/detaylar",
                new { yorum = "test", detaylar = (object?)null });
            Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
        }
    }
}
