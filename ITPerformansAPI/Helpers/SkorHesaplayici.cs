using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ITPerformansAPI.Helpers
{
    public static class SkorHesaplayici
    {
        // Toplam skor her zaman kayitli DegerlendirmeDetaylar satirlarindan
        // sunucuda hesaplanir; client'tan gelen bir skor asla dogrudan guvenilmez.
        public static double Hesapla(SqlConnection connection, int degerlendirmeId)
        {
            var parametreler = new DynamicParameters();
            parametreler.Add("DegerlendirmeId", degerlendirmeId);
            parametreler.Add("Skor", dbType: DbType.Double, direction: ParameterDirection.Output);

            connection.Execute("usp_Degerlendirmeler_SkorHesapla", parametreler, commandType: CommandType.StoredProcedure);

            return parametreler.Get<double>("Skor");
        }

        public static double YenidenHesaplaVeKaydet(SqlConnection connection, int degerlendirmeId)
        {
            var parametreler = new DynamicParameters();
            parametreler.Add("DegerlendirmeId", degerlendirmeId);
            parametreler.Add("Skor", dbType: DbType.Double, direction: ParameterDirection.Output);

            connection.Execute("usp_Degerlendirmeler_SkorHesaplaVeKaydet", parametreler, commandType: CommandType.StoredProcedure);

            return parametreler.Get<double>("Skor");
        }
    }
}
