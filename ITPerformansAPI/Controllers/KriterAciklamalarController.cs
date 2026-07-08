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
    public class KriterAciklamalarController : ControllerBase
    {
        private readonly string _connectionString;

        public KriterAciklamalarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<KriterAciklama>("usp_KriterAciklamalar_GetAll", commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var aciklama = connection.QueryFirstOrDefault<KriterAciklama>(
                "usp_KriterAciklamalar_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure);
            if (aciklama == null) return NotFound();
            return Ok(aciklama);
        }

        [HttpGet("kriter/{altKriterId}")]
        public IActionResult GetByAltKriter(int altKriterId)
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<KriterAciklama>(
                "usp_KriterAciklamalar_GetByAltKriter", new { AltKriterId = altKriterId },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpGet("kriter/{altKriterId}/rol/{rol}")]
        public IActionResult GetByAltKriterVeRol(int altKriterId, string rol)
        {
            using var connection = new SqlConnection(_connectionString);
            var aciklama = connection.QueryFirstOrDefault<KriterAciklama>(
                "usp_KriterAciklamalar_GetByAltKriterVeRol", new { AltKriterId = altKriterId, Rol = rol },
                commandType: CommandType.StoredProcedure);
            if (aciklama == null) return NotFound();
            return Ok(aciklama);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] KriterAciklama yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("usp_KriterAciklamalar_Create",
                new { yeni.AltKriterId, yeni.Rol, yeni.Aciklama },
                commandType: CommandType.StoredProcedure);
            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] KriterAciklama guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;
            connection.Execute("usp_KriterAciklamalar_Update", guncellendi, commandType: CommandType.StoredProcedure);
            return Ok("Guncellendi");
        }

        [HttpPost("kriter/{altKriterId}/upsert")]
        [Authorize(Roles = "Admin")]
        public IActionResult Upsert(int altKriterId, [FromBody] List<KriterAciklama> aciklamalar)
        {
            using var connection = new SqlConnection(_connectionString);
            foreach (var a in aciklamalar)
            {
                connection.Execute("usp_KriterAciklamalar_Upsert",
                    new { AltKriterId = altKriterId, a.Rol, a.Aciklama },
                    commandType: CommandType.StoredProcedure);
            }
            return Ok(new { mesaj = "Açıklamalar kaydedildi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("usp_KriterAciklamalar_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            return Ok("Silindi");
        }
    }
}
