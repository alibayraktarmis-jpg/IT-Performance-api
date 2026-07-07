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

        // Aktif ana basliklarin agirlik toplami %100'u gecemez. id parametresi Update sirasinda
        // guncellenen kaydin kendisini toplamdan haric tutmak icin kullanilir (Create'de null).
        private IActionResult? AgirlikToplamiKontrolu(SqlConnection connection, int agirlikYuzdesi, bool aktifMi, int? id)
        {
            if (agirlikYuzdesi < 0 || agirlikYuzdesi > 100)
                return new BadRequestObjectResult(new { mesaj = "Ağırlık yüzdesi 0 ile 100 arasında olmalıdır." });

            if (!aktifMi) return null;

            var sql = "SELECT ISNULL(SUM(AgirlikYuzdesi), 0) FROM AnaBasliklar WHERE AktifMi = 1" + (id != null ? " AND Id != @Id" : "");
            var digerAktifToplam = connection.ExecuteScalar<int>(sql, new { Id = id });

            if (digerAktifToplam + agirlikYuzdesi > 100)
            {
                return new BadRequestObjectResult(new
                {
                    mesaj = "Aktif ana kriterlerin toplam ağırlığı %100'ü geçemez."
                });
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] AnaBaslik yeni)
        {
            using var connection = new SqlConnection(_connectionString);

            var agirlikHatasi = AgirlikToplamiKontrolu(connection, yeni.AgirlikYuzdesi, yeni.AktifMi, null);
            if (agirlikHatasi != null) return agirlikHatasi;

            var sql = "INSERT INTO AnaBasliklar (Baslik, AgirlikYuzdesi, AktifMi) VALUES (@Baslik, @AgirlikYuzdesi, @AktifMi)";
            connection.Execute(sql, yeni);
            return Ok("Eklendi");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] AnaBaslik guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;

            var agirlikHatasi = AgirlikToplamiKontrolu(connection, guncellendi.AgirlikYuzdesi, guncellendi.AktifMi, id);
            if (agirlikHatasi != null) return agirlikHatasi;

            connection.Execute("UPDATE AnaBasliklar SET Baslik=@Baslik, AgirlikYuzdesi=@AgirlikYuzdesi, AktifMi=@AktifMi WHERE Id=@Id", guncellendi);

            // Ana baslik pasife/aktif alindiginda, altindaki tum alt kriterler de ayni duruma getirilir.
            connection.Execute("UPDATE AltKriterler SET AktifMi=@AktifMi WHERE AnaBaslikId=@Id", new { guncellendi.AktifMi, Id = id });

            return Ok("Guncellendi");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var altKriterSayisi = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM AltKriterler WHERE AnaBaslikId = @Id", new { Id = id });
            if (altKriterSayisi > 0)
            {
                return BadRequest(new
                {
                    mesaj = "Bu ana kriterin altında hâlâ alt kriterler var. Önce onları silin veya başka bir ana kritere taşıyın."
                });
            }

            connection.Execute("DELETE FROM AnaBasliklar WHERE Id=@Id", new { Id = id });
            return Ok("Silindi");
        }
    }
}