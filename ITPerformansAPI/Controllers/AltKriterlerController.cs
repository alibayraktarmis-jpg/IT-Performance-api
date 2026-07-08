using Microsoft.AspNetCore.Authorization;
using Dapper;
using ITPerformansAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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
            var liste = connection.Query<AltKriter>(
                "usp_AltKriterler_GetAll", new { SadeceAktif = sadaceAktif },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var kriter = connection.QueryFirstOrDefault<AltKriter>(
                "usp_AltKriterler_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure);
            if (kriter == null) return NotFound();
            return Ok(kriter);
        }

        [HttpGet("baslik/{anaBaslikId}")]
        public IActionResult GetByAnaBaslik(int anaBaslikId)
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<AltKriter>(
                "usp_AltKriterler_GetByAnaBaslik", new { AnaBaslikId = anaBaslikId },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] AltKriter yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var yeniId = connection.ExecuteScalar<int>(
                "usp_AltKriterler_Create", new { yeni.AnaBaslikId, yeni.KriterAdi, yeni.AktifMi },
                commandType: CommandType.StoredProcedure);
            return Ok(new { mesaj = "Eklendi", id = yeniId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] AltKriter guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;
            connection.Execute("usp_AltKriterler_Update", guncellendi, commandType: CommandType.StoredProcedure);
            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_AltKriterler_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok("Silindi");
        }
    }
}
