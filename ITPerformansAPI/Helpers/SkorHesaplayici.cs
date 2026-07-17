using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ITPerformansAPI.Helpers
{
    public static class SkorHesaplayici
    {
        public static double Hesapla(SqlConnection connection, int degerlendirmeId, SqlTransaction? transaction = null)
        {
            var parametreler = new DynamicParameters();
            parametreler.Add("DegerlendirmeId", degerlendirmeId);
            parametreler.Add("Skor", dbType: DbType.Double, direction: ParameterDirection.Output);

            connection.Execute("usp_Degerlendirmeler_SkorHesapla", parametreler, transaction, commandType: CommandType.StoredProcedure);

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
