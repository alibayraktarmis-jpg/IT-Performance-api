using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using ITPerformansAPI.Helpers;
using System.Security.Claims;

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
            var liste = connection.Query<DegerlendirmeDetay>(
                "SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId",
                new { DegerlendirmeId = degerlendirmeId }).ToList();
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

            var sql = "INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan) VALUES (@DegerlendirmeId, @AltKriterId, @Puan)";
            connection.Execute(sql, yeni);

            // Yeni detay eklendikce ust degerlendirmenin toplam skoru sunucuda yeniden hesaplanir
            SkorHesaplayici.YenidenHesaplaVeKaydet(connection, yeni.DegerlendirmeId);
            return Ok("Eklendi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var degerlendirmeId = connection.QueryFirstOrDefault<int?>("SELECT DegerlendirmeId FROM DegerlendirmeDetaylar WHERE Id = @Id", new { Id = id });
            if (degerlendirmeId == null) return NotFound();

            var erisimHatasi = DegerlendirmeErisimKontrolu(connection, degerlendirmeId.Value);
            if (erisimHatasi != null) return erisimHatasi;

            connection.Execute("DELETE FROM DegerlendirmeDetaylar WHERE Id=@Id", new { Id = id });

            // Detay silindikce ust degerlendirmenin toplam skoru sunucuda yeniden hesaplanir
            SkorHesaplayici.YenidenHesaplaVeKaydet(connection, degerlendirmeId.Value);
            return Ok("Silindi");
        }

        // Admin her degerlendirmeye, Evaluator sadece kendi ekibindeki calisanlarin degerlendirmelerine erisebilir
        private IActionResult? DegerlendirmeErisimKontrolu(SqlConnection connection, int degerlendirmeId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return null;

            var calisanId = connection.QueryFirstOrDefault<int?>("SELECT CalisanId FROM Degerlendirmeler WHERE Id = @Id", new { Id = degerlendirmeId });
            if (calisanId == null) return NotFound();

            return ErisimKontrol.EvaluatorKendiEkibindeMi(connection, User, calisanId.Value) ? null : Forbid();
        }
    }
}