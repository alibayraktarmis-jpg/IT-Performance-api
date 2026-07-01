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
    public class AnaBasliklarController : ControllerBase
    {
        private readonly string _connectionString;

        public AnaBasliklarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] bool sadaceAktif = false)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = sadaceAktif ? "SELECT * FROM AnaBasliklar WHERE AktifMi = 1" : "SELECT * FROM AnaBasliklar";
            var liste = connection.Query<AnaBaslik>(sql).ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var baslik = connection.QueryFirstOrDefault<AnaBaslik>("SELECT * FROM AnaBasliklar WHERE Id = @Id", new { Id = id });
            if (baslik == null) return NotFound();
            return Ok(baslik);
        }

        [HttpPost]
        public IActionResult Create([FromBody] AnaBaslik yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO AnaBasliklar (Baslik, AgirlikYuzdesi, AktifMi) VALUES (@Baslik, @AgirlikYuzdesi, @AktifMi)";
            connection.Execute(sql, yeni);
            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] AnaBaslik guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;
            connection.Execute("UPDATE AnaBasliklar SET Baslik=@Baslik, AgirlikYuzdesi=@AgirlikYuzdesi, AktifMi=@AktifMi WHERE Id=@Id", guncellendi);
            connection.Execute(
                guncellendi.AktifMi
                    ? "UPDATE AltKriterler SET AktifMi=1 WHERE AnaBaslikId=@Id"
                    : "UPDATE AltKriterler SET AktifMi=0 WHERE AnaBaslikId=@Id",
                new { Id = id });
            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM AnaBasliklar WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}