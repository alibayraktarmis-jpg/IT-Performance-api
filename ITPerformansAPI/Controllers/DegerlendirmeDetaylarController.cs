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
    public class DegerlendirmeDetaylarController : ControllerBase
    {
        private readonly string _connectionString;

        public DegerlendirmeDetaylarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet("degerlendirme/{degerlendirmeId}")]
        public IActionResult GetByDegerlendirme(int degerlendirmeId)
        {
            using var connection = new SqlConnection(_connectionString);

            var calisanId = connection.QueryFirstOrDefault<int?>(
                "usp_Degerlendirmeler_GetCalisanId", new { Id = degerlendirmeId },
                commandType: CommandType.StoredProcedure);
            if (calisanId == null) return NotFound();

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var kendiKaydiMi = rol == "Employee" && calisanId == kullaniciId;
            if (!kendiKaydiMi && !ErisimKontrol.EvaluatorKendiEkibindeMi(connection, User, calisanId.Value))
                return Forbid();

            var liste = connection.Query<DegerlendirmeDetay>(
                "usp_DegerlendirmeDetaylar_GetByDegerlendirme", new { DegerlendirmeId = degerlendirmeId },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult Create([FromBody] DegerlendirmeDetay yeni)
        {
            if (yeni.Puan < 1 || yeni.Puan > 5)
                return BadRequest(new { mesaj = "Puan 1 ile 5 arasinda olmalidir." });

            using var connection = new SqlConnection(_connectionString);

            var erisimHatasi = DegerlendirmeErisimKontrolu(connection, yeni.DegerlendirmeId);
            if (erisimHatasi != null) return erisimHatasi;

            connection.Execute("usp_DegerlendirmeDetaylar_Create",
                new { yeni.DegerlendirmeId, yeni.AltKriterId, yeni.Puan },
                commandType: CommandType.StoredProcedure);

            // Yeni detay eklendikce ust degerlendirmenin toplam skoru sunucuda yeniden hesaplanir
            SkorHesaplayici.YenidenHesaplaVeKaydet(connection, yeni.DegerlendirmeId);
            return Ok("Eklendi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var degerlendirmeId = connection.QueryFirstOrDefault<int?>(
                "usp_DegerlendirmeDetaylar_GetDegerlendirmeId", new { Id = id },
                commandType: CommandType.StoredProcedure);
            if (degerlendirmeId == null) return NotFound();

            var erisimHatasi = DegerlendirmeErisimKontrolu(connection, degerlendirmeId.Value);
            if (erisimHatasi != null) return erisimHatasi;

            connection.Execute("usp_DegerlendirmeDetaylar_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);

            // Detay silindikce ust degerlendirmenin toplam skoru sunucuda yeniden hesaplanir
            SkorHesaplayici.YenidenHesaplaVeKaydet(connection, degerlendirmeId.Value);
            return Ok("Silindi");
        }

        // Admin her degerlendirmeye, Evaluator sadece kendi ekibindeki calisanlarin degerlendirmelerine erisebilir
        private IActionResult? DegerlendirmeErisimKontrolu(SqlConnection connection, int degerlendirmeId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return null;

            var calisanId = connection.QueryFirstOrDefault<int?>(
                "usp_Degerlendirmeler_GetCalisanId", new { Id = degerlendirmeId },
                commandType: CommandType.StoredProcedure);
            if (calisanId == null) return NotFound();

            return ErisimKontrol.EvaluatorKendiEkibindeMi(connection, User, calisanId.Value) ? null : Forbid();
        }
    }
}
