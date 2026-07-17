using System.Security.Claims;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Helpers
{
    public static class ErisimKontrol
    {
        public static bool EvaluatorKendiEkibindeMi(SqlConnection connection, ClaimsPrincipal user, int calisanId)
        {
            var rol = user.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return true;
            if (rol != "Evaluator") return false;

            var kullaniciId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var gecerliMi = connection.QueryFirstOrDefault<int?>(
                "usp_Kullanicilar_EvaluatorKendiEkibindeMi",
                new { CalisanId = calisanId, EvaluatorId = kullaniciId },
                commandType: CommandType.StoredProcedure);
            return gecerliMi != null;
        }
    }
}
