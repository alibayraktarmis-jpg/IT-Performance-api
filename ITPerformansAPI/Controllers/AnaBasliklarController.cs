using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using System.Data;

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
            var liste = connection.Query<AnaBaslik>(
                "usp_AnaBasliklar_GetAll", new { SadeceAktif = sadaceAktif },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var baslik = connection.QueryFirstOrDefault<AnaBaslik>(
                "usp_AnaBasliklar_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure);
            if (baslik == null) return NotFound();
            return Ok(baslik);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] AnaBaslik yeni)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_AnaBasliklar_Create", new
                {
                    yeni.Baslik,
                    yeni.AgirlikYuzdesi,
                    yeni.AktifMi
                }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] AnaBaslik guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_AnaBasliklar_Update", new
                {
                    Id = id,
                    guncellendi.Baslik,
                    guncellendi.AgirlikYuzdesi,
                    guncellendi.AktifMi
                }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_AnaBasliklar_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok("Silindi");
        }
    }
}
