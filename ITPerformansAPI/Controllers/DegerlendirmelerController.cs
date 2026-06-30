using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        [HttpGet("calisan/{calisanId}/donem")]
        public IActionResult GetByCalisanDonem(int calisanId, [FromQuery] string donem)
        {
            using var connection = new SqlConnection(_connectionString);
            var deg = connection.QueryFirstOrDefault<Degerlendirme>(
                "SELECT * FROM Degerlendirmeler WHERE CalisanId = @CalisanId AND Donem = @Donem",
                new { CalisanId = calisanId, Donem = donem });
            if (deg == null) return Ok(null);
            var detaylar = connection.Query(
                "SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @Id",
                new { Id = deg.Id }).ToList();
            return Ok(new { degerlendirme = deg, detaylar });
        }

        [HttpPut("{id}/detaylar")]
        public IActionResult UpdateDetaylar(int id, [FromBody] UpdateDetaylarDto dto)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("UPDATE Degerlendirmeler SET Yorum=@Yorum, ToplamSkor=@ToplamSkor, Tarih=@Tarih WHERE Id=@Id",
                new { dto.Yorum, dto.ToplamSkor, Tarih = DateTime.Now, Id = id });
            connection.Execute("DELETE FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @Id", new { Id = id });
            foreach (var d in dto.Detaylar)
            {
                connection.Execute("INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan) VALUES (@DegerlendirmeId, @AltKriterId, @Puan)",
                    new { DegerlendirmeId = id, d.AltKriterId, d.Puan });
            }
            return Ok(new { mesaj = "Degerlendirme guncellendi" });
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
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult UpdateDegerlendirme(int id, [FromBody] Degerlendirme guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);
            guncellendi.Id = id;
            var sql = "UPDATE Degerlendirmeler SET DegerlendiricId=@DegerlendiricId, CalisanId=@CalisanId, Tarih=@Tarih, Donem=@Donem, Yorum=@Yorum, ToplamSkor=@ToplamSkor WHERE Id=@Id";
            connection.Execute(sql, guncellendi);
            return Ok(new { mesaj = "Degerlendirme guncellendi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteDegerlendirme(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM Degerlendirmeler WHERE Id=@Id", new { Id = id });
            return Ok(new { mesaj = "Degerlendirme silindi" });
        }

        [HttpGet("donemler")]
        public IActionResult GetDonemler()
        {
            using var connection = new SqlConnection(_connectionString);
            var donemler = connection.Query<string>("SELECT DISTINCT Donem FROM Degerlendirmeler WHERE Donem IS NOT NULL AND Donem != '' ORDER BY Donem DESC").ToList();
            return Ok(donemler);
        }

        [HttpGet("skor/{calisanId}")]
        public IActionResult ToplamSkorHesapla(int calisanId, [FromQuery] string? donem = null)
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
                " + (donem != null ? "AND d.Donem = @Donem" : "") + @"
                GROUP BY ab.Baslik, ab.AgirlikYuzdesi";

            var kategoriSkorlar = connection.Query(sql, new { CalisanId = calisanId, Donem = donem }).ToList();

            double toplamSkor = 0;
            foreach (var kategori in kategoriSkorlar)
            {
                double kategorSkor = (kategori.AgirlikYuzdesi / 100.0) * (kategori.OrtalmaPuan / 5.0) * 100.0;
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
        public IActionResult GetSiralama([FromQuery] string? donem = null)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);

            string donemFilter = donem != null ? "AND d.Donem = @Donem" : "";
            string sql;

            if (rol == "Admin")
            {
                sql = $@"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }
            else if (rol == "Evaluator")
            {
                sql = $@"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    WHERE k.Rol = 'Employee' AND k.EvaluatorId = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }
            else
            {
                sql = $@"
                    SELECT k.Id, k.Ad, k.Soyad, k.Departman, k.Rol,
                           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    WHERE k.Id = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY OrtalamaToplamSkor DESC";
            }

            var sonuc = connection.Query(sql, new { KullaniciId = kullaniciId, Donem = donem }).ToList();
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

        [HttpGet("pdf")]
        [Authorize(Roles = "Admin")]
        public IActionResult PdfExport()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT k.Ad, k.Soyad, k.Departman, k.Rol,
                       AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                FROM Kullanicilar k
                LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId
                GROUP BY k.Ad, k.Soyad, k.Departman, k.Rol
                ORDER BY OrtalamaToplamSkor DESC";

            var veriler = connection.Query(sql).ToList();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("IT Departmanı Performans Raporu")
                            .FontSize(18).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(30);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(8)
                                .Text("#").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(8)
                                .Text("Ad Soyad").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(8)
                                .Text("Departman").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(8)
                                .Text("Rol").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(8)
                                .Text("Ortalama Skor").FontColor(Colors.White).Bold();
                        });

                        for (int i = 0; i < veriler.Count; i++)
                        {
                            var v = veriler[i];
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            double? skor = v.OrtalamaToplamSkor;

                            string ad = $"{v.Ad} {v.Soyad}";
                            string departman = (string?)v.Departman ?? "-";
                            string rol = (string?)v.Rol ?? "-";
                            string skorStr = skor.HasValue ? skor.Value.ToString("F1") : "-";

                            table.Cell().Background(bg).Padding(8).Text((i + 1).ToString());
                            table.Cell().Background(bg).Padding(8).Text(ad);
                            table.Cell().Background(bg).Padding(8).Text(departman);
                            table.Cell().Background(bg).Padding(8).Text(rol);
                            table.Cell().Background(bg).Padding(8).Text(skorStr);
                        }
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("IT Performans Değerlendirme Sistemi  |  Sayfa ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });

            var bytes = pdf.GeneratePdf();
            return File(bytes, "application/pdf", "PerformansRaporu.pdf");
        }
    }
}