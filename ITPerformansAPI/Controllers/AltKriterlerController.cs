using Microsoft.AspNetCore.Authorization;
using Dapper;
using ITPerformansAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AltKriterlerController : ControllerBase
    {
        private readonly string _connectionString;

        public AltKriterlerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] bool sadaceAktif = false)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = sadaceAktif ? "SELECT * FROM AltKriterler WHERE AktifMi = 1" : "SELECT * FROM AltKriterler";
            var liste = connection.Query<AltKriter>(sql).ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var kriter = connection.QueryFirstOrDefault<AltKriter>("SELECT * FROM AltKriterler WHERE Id = @Id", new { Id = id });
            if (kriter == null) return NotFound();
            return Ok(kriter);
        }

        [HttpGet("baslik/{anaBaslikId}")]
        public IActionResult GetByAnaBaslik(int anaBaslikId)
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<AltKriter>("SELECT * FROM AltKriterler WHERE AnaBaslikId = @AnaBaslikId", new { AnaBaslikId = anaBaslikId }).ToList();
            return Ok(liste);
        }

        [HttpPost]
        public IActionResult Create([FromBody] AltKriter yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO AltKriterler (AnaBaslikId, KriterAdi, AktifMi) OUTPUT INSERTED.Id VALUES (@AnaBaslikId, @KriterAdi, @AktifMi)";
            var yeniId = connection.ExecuteScalar<int>(sql, yeni);
            return Ok(new { mesaj = "Eklendi", id = yeniId });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] AltKriter guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE AltKriterler SET AnaBaslikId=@AnaBaslikId, KriterAdi=@KriterAdi, AktifMi=@AktifMi WHERE Id=@Id";
            guncellendi.Id = id;
            connection.Execute(sql, guncellendi);
            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM AltKriterler WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}