using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using System.Security.Claims;

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

            if (rol == "Admin" || rol == "Evaluator")
            {
                var liste = connection.Query(@"
                    SELECT h.Id AS id, h.CalisanId AS calisanId, h.Aciklama AS aciklama, h.BitisTarihi AS bitisTarihi, h.TamamlandiMi AS tamamlandiMi,
                           k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman
                    FROM Hedefler h
                    INNER JOIN Kullanicilar k ON h.CalisanId = k.Id
                    ORDER BY h.TamamlandiMi ASC, h.BitisTarihi ASC").ToList();
                return Ok(liste);
            }
            else
            {
                var liste = connection.Query(@"
                    SELECT h.Id AS id, h.CalisanId AS calisanId, h.Aciklama AS aciklama, h.BitisTarihi AS bitisTarihi, h.TamamlandiMi AS tamamlandiMi,
                           k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman
                    FROM Hedefler h
                    INNER JOIN Kullanicilar k ON h.CalisanId = k.Id
                    WHERE h.CalisanId = @KullaniciId
                    ORDER BY h.TamamlandiMi ASC, h.BitisTarihi ASC",
                    new { KullaniciId = kullaniciId }).ToList();
                return Ok(liste);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult CreateHedef([FromBody] Hedef yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!CalisanErisimVarMi(connection, yeni.CalisanId)) return Forbid();

            var sql = @"INSERT INTO Hedefler (CalisanId, Aciklama, BitisTarihi, TamamlandiMi)
                        OUTPUT INSERTED.Id
                        VALUES (@CalisanId, @Aciklama, @BitisTarihi, 0)";
            var yeniId = connection.ExecuteScalar<int>(sql, yeni);
            return Ok(new { mesaj = "Hedef eklendi", id = yeniId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult UpdateHedef(int id, [FromBody] Hedef guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();

            connection.Execute("UPDATE Hedefler SET Aciklama=@Aciklama, BitisTarihi=@BitisTarihi WHERE Id=@Id",
                new { guncellendi.Aciklama, guncellendi.BitisTarihi, Id = id });
            return Ok(new { mesaj = "Hedef güncellendi" });
        }

        [HttpPut("{id}/tamamla")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult Tamamla(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();
            connection.Execute("UPDATE Hedefler SET TamamlandiMi = 1 WHERE Id = @Id", new { Id = id });
            return Ok(new { mesaj = "Tamamlandı" });
        }

        [HttpPut("{id}/geriAl")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult GeriAl(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();
            connection.Execute("UPDATE Hedefler SET TamamlandiMi = 0 WHERE Id = @Id", new { Id = id });
            return Ok(new { mesaj = "Geri alındı" });
        }

        // Admin her calisana/hedefe, Evaluator sadece kendi ekibindeki calisanlara/hedeflere erisebilir
        private bool CalisanErisimVarMi(SqlConnection connection, int calisanId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return true;

            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var gecerliMi = connection.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Kullanicilar WHERE Id = @CalisanId AND EvaluatorId = @EvaluatorId",
                new { CalisanId = calisanId, EvaluatorId = kullaniciId });
            return gecerliMi != null;
        }

        private bool HedefeErisimVarMi(SqlConnection connection, int hedefId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return true;

            var calisanId = connection.QueryFirstOrDefault<int?>("SELECT CalisanId FROM Hedefler WHERE Id = @Id", new { Id = hedefId });
            if (calisanId == null) return false;

            return CalisanErisimVarMi(connection, calisanId.Value);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult DeleteHedef(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            if (!HedefeErisimVarMi(connection, id)) return Forbid();

            connection.Execute("DELETE FROM Hedefler WHERE Id = @Id", new { Id = id });
            return Ok(new { mesaj = "Hedef silindi" });
        }
    }
}
