using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;

namespace ITPerformansAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DegerlendirmelerController : ControllerBase
    {
        private readonly string _connectionString;

        public DegerlendirmelerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetDegerlendirmeler()
        {
            using var connection = new SqlConnection(_connectionString);
            var degerlendirmeler = connection.Query<Degerlendirme>("SELECT * FROM Degerlendirmeler").ToList();
            return Ok(degerlendirmeler);
        }

        [HttpGet("calisan/{calisanId}")]
        public IActionResult GetByCalisanId(int calisanId)
        {
            using var connection = new SqlConnection(_connectionString);
            var degerlendirmeler = connection.Query<Degerlendirme>("SELECT * FROM Degerlendirmeler WHERE CalisanId = @CalisanId", new { CalisanId = calisanId }).ToList();
            return Ok(degerlendirmeler);
        }

        [HttpPost]
        public IActionResult CreateDegerlendirme([FromBody] Degerlendirme yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO Degerlendirmeler (DegerlendiricId, CalisanId, Tarih, Donem, Yorum, ToplamSkor) VALUES (@DegerlendiricId, @CalisanId, @Tarih, @Donem, @Yorum, @ToplamSkor)";
            connection.Execute(sql, yeni);
            return Ok(new { mesaj = "Degerlendirme eklendi" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDegerlendirme(int id, [FromBody] Degerlendirme guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;
            var sql = "UPDATE Degerlendirmeler SET DegerlendiricId=@DegerlendiricId, CalisanId=@CalisanId, Tarih=@Tarih, Donem=@Donem, Yorum=@Yorum, ToplamSkor=@ToplamSkor WHERE Id=@Id";
            connection.Execute(sql, guncellendi);
            return Ok(new { mesaj = "Degerlendirme guncellendi" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDegerlendirme(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM Degerlendirmeler WHERE Id=@Id", new { Id = id });
            return Ok(new { mesaj = "Degerlendirme silindi" });
        }
    }
}