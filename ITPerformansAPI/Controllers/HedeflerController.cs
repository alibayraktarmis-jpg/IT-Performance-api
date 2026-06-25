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
    public class HedeflerController : ControllerBase
    {
        private readonly string _connectionString;

        public HedeflerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<Hedef>("SELECT * FROM Hedefler").ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var hedef = connection.QueryFirstOrDefault<Hedef>("SELECT * FROM Hedefler WHERE Id = @Id", new { Id = id });
            if (hedef == null) return NotFound();
            return Ok(hedef);
        }

        [HttpGet("calisan/{calisanId}")]
        public IActionResult GetByCalisan(int calisanId)
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<Hedef>(
                "SELECT * FROM Hedefler WHERE CalisanId = @CalisanId",
                new { CalisanId = calisanId }).ToList();
            return Ok(liste);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Hedef yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO Hedefler (CalisanId, Aciklama, BitisTarihi, TamamlandiMi) VALUES (@CalisanId, @Aciklama, @BitisTarihi, @TamamlandiMi)";
            connection.Execute(sql, yeni);
            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Hedef guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Hedefler SET CalisanId=@CalisanId, Aciklama=@Aciklama, BitisTarihi=@BitisTarihi, TamamlandiMi=@TamamlandiMi WHERE Id=@Id";
            guncellendi.Id = id;
            connection.Execute(sql, guncellendi);
            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM Hedefler WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}