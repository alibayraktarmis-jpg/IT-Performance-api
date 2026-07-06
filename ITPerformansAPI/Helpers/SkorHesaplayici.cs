using Dapper;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Helpers
{
    public static class SkorHesaplayici
    {
        // Toplam skor her zaman kayitli DegerlendirmeDetaylar satirlarindan
        // sunucuda hesaplanir; client'tan gelen bir skor asla dogrudan guvenilmez.
        public static double Hesapla(SqlConnection connection, int degerlendirmeId)
        {
            var sql = @"
                SELECT
                    ab.AgirlikYuzdesi,
                    AVG(CAST(dd.Puan AS FLOAT)) AS OrtalamaPuan
                FROM DegerlendirmeDetaylar dd
                INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
                INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
                WHERE dd.DegerlendirmeId = @DegerlendirmeId
                GROUP BY ab.Id, ab.AgirlikYuzdesi";

            var kategoriler = connection.Query(sql, new { DegerlendirmeId = degerlendirmeId }).ToList();

            double toplam = 0;
            foreach (var kategori in kategoriler)
            {
                toplam += (kategori.AgirlikYuzdesi / 100.0) * (kategori.OrtalamaPuan / 5.0) * 100.0;
            }
            return Math.Round(toplam, 2);
        }

        public static double YenidenHesaplaVeKaydet(SqlConnection connection, int degerlendirmeId)
        {
            var skor = Hesapla(connection, degerlendirmeId);
            connection.Execute("UPDATE Degerlendirmeler SET ToplamSkor = @Skor WHERE Id = @Id", new { Skor = skor, Id = degerlendirmeId });
            return skor;
        }
    }
}
