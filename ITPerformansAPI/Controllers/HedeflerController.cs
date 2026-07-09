using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using ITPerformansAPI.Helpers;
using System.Security.Claims;
using System.Data;

namespace ITPerformansAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HedeflerController : ControllerBase
    {
        private readonly string _connectionString;

        public HedeflerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetHedefler()
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query(
                "usp_Hedefler_GetAll", new { Rol = rol, KullaniciId = kullaniciId },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult CreateHedef([FromBody] Hedef yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!CalisanErisimVarMi(connection, yeni.CalisanId)) return Forbid();

            try
            {
                var yeniId = connection.ExecuteScalar<int>(
                    "usp_Hedefler_Create", new { yeni.CalisanId, yeni.Aciklama, yeni.BitisTarihi },
                    commandType: CommandType.StoredProcedure);
                return Ok(new { mesaj = "Hedef eklendi", id = yeniId });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult UpdateHedef(int id, [FromBody] Hedef guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();

            connection.Execute("usp_Hedefler_Update",
                new { Id = id, guncellendi.Aciklama, guncellendi.BitisTarihi },
                commandType: CommandType.StoredProcedure);
            return Ok(new { mesaj = "Hedef güncellendi" });
        }

        [HttpPut("{id}/tamamla")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult Tamamla(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();
            connection.Execute("usp_Hedefler_Tamamla", new { Id = id }, commandType: CommandType.StoredProcedure);
            return Ok(new { mesaj = "Tamamlandı" });
        }

        [HttpPut("{id}/geriAl")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult GeriAl(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();
            connection.Execute("usp_Hedefler_GeriAl", new { Id = id }, commandType: CommandType.StoredProcedure);
            return Ok(new { mesaj = "Geri alındı" });
        }

        // Admin her calisana/hedefe, Evaluator sadece kendi ekibindeki calisanlara/hedeflere erisebilir
        private bool CalisanErisimVarMi(SqlConnection connection, int calisanId)
            => ErisimKontrol.EvaluatorKendiEkibindeMi(connection, User, calisanId);

        private bool HedefeErisimVarMi(SqlConnection connection, int hedefId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return true;

            var calisanId = connection.QueryFirstOrDefault<int?>(
                "usp_Hedefler_GetCalisanId", new { Id = hedefId },
                commandType: CommandType.StoredProcedure);
            if (calisanId == null) return false;

            return CalisanErisimVarMi(connection, calisanId.Value);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult DeleteHedef(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();

            connection.Execute("usp_Hedefler_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            return Ok(new { mesaj = "Hedef silindi" });
        }
    }
}
