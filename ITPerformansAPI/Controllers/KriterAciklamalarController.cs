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
            var liste = connection.Query<KriterAciklama>("SELECT * FROM KriterAciklamalar").ToList();
            return Ok(liste);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var aciklama = connection.QueryFirstOrDefault<KriterAciklama>("SELECT * FROM KriterAciklamalar WHERE Id = @Id", new { Id = id });
            if (aciklama == null) return NotFound();
            return Ok(aciklama);
        }

        [HttpGet("kriter/{altKriterId}")]
        public IActionResult GetByAltKriter(int altKriterId)
        {
            using var connection = new SqlConnection(_connectionString);
            var liste = connection.Query<KriterAciklama>("SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId", new { AltKriterId = altKriterId }).ToList();
            return Ok(liste);
        }

        [HttpGet("kriter/{altKriterId}/rol/{rol}")]
        public IActionResult GetByAltKriterVeRol(int altKriterId, string rol)
        {
            using var connection = new SqlConnection(_connectionString);
            var aciklama = connection.QueryFirstOrDefault<KriterAciklama>(
                "SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol",
                new { AltKriterId = altKriterId, Rol = rol });
            if (aciklama == null) return NotFound();
            return Ok(aciklama);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] KriterAciklama yeni)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama) VALUES (@AltKriterId, @Rol, @Aciklama)";
            connection.Execute(sql, yeni);
            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] KriterAciklama guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE KriterAciklamalar SET AltKriterId=@AltKriterId, Rol=@Rol, Aciklama=@Aciklama WHERE Id=@Id";
            guncellendi.Id = id;
            connection.Execute(sql, guncellendi);
            return Ok("Guncellendi");
        }

        [HttpPost("kriter/{altKriterId}/upsert")]
        [Authorize(Roles = "Admin")]
        public IActionResult Upsert(int altKriterId, [FromBody] List<KriterAciklama> aciklamalar)
        {
            using var connection = new SqlConnection(_connectionString);
            foreach (var a in aciklamalar)
            {
                var mevcut = connection.QueryFirstOrDefault<KriterAciklama>(
                    "SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol",
                    new { AltKriterId = altKriterId, Rol = a.Rol });

                if (mevcut != null)
                {
                    connection.Execute(
                        "UPDATE KriterAciklamalar SET Aciklama = @Aciklama WHERE Id = @Id",
                        new { Aciklama = a.Aciklama, Id = mevcut.Id });
                }
                else if (!string.IsNullOrWhiteSpace(a.Aciklama))
                {
                    connection.Execute(
                        "INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama) VALUES (@AltKriterId, @Rol, @Aciklama)",
                        new { AltKriterId = altKriterId, Rol = a.Rol, Aciklama = a.Aciklama });
                }
            }
            return Ok(new { mesaj = "Açıklamalar kaydedildi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM KriterAciklamalar WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}