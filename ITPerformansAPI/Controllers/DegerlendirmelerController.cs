using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using System.Security.Claims;

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
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);

            if (rol == "Admin")
            {
                var liste = connection.Query<Degerlendirme>("SELECT * FROM Degerlendirmeler").ToList();
                return Ok(liste);
            }
            else if (rol == "Evaluator")
            {
                var liste = connection.Query<Degerlendirme>(
                    "SELECT * FROM Degerlendirmeler WHERE DegerlendiricId = @Id",
                    new { Id = kullaniciId }).ToList();
                return Ok(liste);
            }
            else
            {
                var liste = connection.Query<Degerlendirme>(
                    "SELECT * FROM Degerlendirmeler WHERE CalisanId = @Id",
                    new { Id = kullaniciId }).ToList();
                return Ok(liste);
            }
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
            var sql = @"INSERT INTO Degerlendirmeler 
                (DegerlendiricId, CalisanId, Tarih, Donem, Yorum, ToplamSkor) 
                OUTPUT INSERTED.Id
                VALUES (@DegerlendiricId, @CalisanId, @Tarih, @Donem, @Yorum, @ToplamSkor)";
            var yeniId = connection.ExecuteScalar<int>(sql, yeni);
            return Ok(new { mesaj = "Degerlendirme eklendi", id = yeniId });
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

        [HttpGet("skor/{calisanId}")]
        public IActionResult ToplamSkorHesapla(int calisanId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    ab.Baslik,
                    ab.AgirlikYuzdesi,
                    AVG(CAST(dd.Puan AS FLOAT)) AS OrtalmaPuan
                FROM DegerlendirmeDetaylar dd
                INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
                INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
                INNER JOIN Degerlendirmeler d ON dd.DegerlendirmeId = d.Id
                WHERE d.CalisanId = @CalisanId
                GROUP BY ab.Baslik, ab.AgirlikYuzdesi";

            var kategoriSkorlar = connection.Query(sql, new { CalisanId = calisanId }).ToList();

            double toplamSkor = 0;
            foreach (var kategori in kategoriSkorlar)
            {
                double kategorSkor = (kategori.AgirlikYuzdesi / 100.0) * kategori.OrtalmaPuan;
                toplamSkor += kategorSkor;
            }

            return Ok(new
            {
                calisanId,
                toplamSkor = Math.Round(toplamSkor, 2),
                kategoriDetay = kategoriSkorlar
            });
        }

        [HttpGet("siralama")]
        public IActionResult GetSiralama()
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);

            string sql;

            if (rol == "Admin")
            {
                sql = @"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }
            else if (rol == "Evaluator")
            {
                sql = @"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId
                    WHERE d.DegerlendiricId = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }
            else
            {
                sql = @"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId
                    WHERE k.Id = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }

            var sonuc = connection.Query(sql, new { KullaniciId = kullaniciId }).ToList();
            return Ok(sonuc);
        }

        [HttpGet("excel")]
        public IActionResult ExcelExport()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT k.Ad, k.Soyad, k.Departman, k.Rol,
                       AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                FROM Kullanicilar k
                LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId
                GROUP BY k.Ad, k.Soyad, k.Departman, k.Rol
                ORDER BY OrtalamaToplamSkor DESC";

            var veriler = connection.Query(sql).ToList();

            using var paket = new OfficeOpenXml.ExcelPackage();
            var sayfa = paket.Workbook.Worksheets.Add("Performans Raporu");

            sayfa.Cells[1, 1].Value = "Ad";
            sayfa.Cells[1, 2].Value = "Soyad";
            sayfa.Cells[1, 3].Value = "Departman";
            sayfa.Cells[1, 4].Value = "Rol";
            sayfa.Cells[1, 5].Value = "Ortalama Skor";

            for (int i = 0; i < veriler.Count; i++)
            {
                var v = veriler[i];
                sayfa.Cells[i + 2, 1].Value = v.Ad;
                sayfa.Cells[i + 2, 2].Value = v.Soyad;
                sayfa.Cells[i + 2, 3].Value = v.Departman;
                sayfa.Cells[i + 2, 4].Value = v.Rol;
                sayfa.Cells[i + 2, 5].Value = v.OrtalamaToplamSkor;
            }

            sayfa.Cells.AutoFitColumns();

            var dosya = paket.GetAsByteArray();
            return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PerformansRaporu.xlsx");
        }
    }
}