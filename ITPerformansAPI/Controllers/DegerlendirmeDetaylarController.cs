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
        public IActionResult Create([FromBody] DegerlendirmeDetay yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan) VALUES (@DegerlendirmeId, @AltKriterId, @Puan)";
            connection.Execute(sql, yeni);
            return Ok("Eklendi");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM DegerlendirmeDetaylar WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}