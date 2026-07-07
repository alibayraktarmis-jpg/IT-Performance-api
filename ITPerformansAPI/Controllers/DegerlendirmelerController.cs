using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using ITPerformansAPI.Helpers;
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
            var erisimHatasi = CalisanErisimKontrolu(connection, calisanId);
            if (erisimHatasi != null) return erisimHatasi;

            var degerlendirmeler = connection.Query<Degerlendirme>("SELECT * FROM Degerlendirmeler WHERE CalisanId = @CalisanId", new { CalisanId = calisanId }).ToList();
            return Ok(degerlendirmeler);
        }

        [HttpGet("calisan/{calisanId}/donem")]
        public IActionResult GetByCalisanDonem(int calisanId, [FromQuery] string donem)
        {
            using var connection = new SqlConnection(_connectionString);
            var erisimHatasi = CalisanErisimKontrolu(connection, calisanId);
            if (erisimHatasi != null) return erisimHatasi;

            var deg = connection.QueryFirstOrDefault<Degerlendirme>(
                "SELECT TOP 1 * FROM Degerlendirmeler WHERE CalisanId = @CalisanId AND Donem = @Donem ORDER BY Id DESC",
                new { CalisanId = calisanId, Donem = donem });
            if (deg == null) return Ok(null);
            var detaylar = connection.Query<DegerlendirmeDetay>(
                "SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @Id",
                new { Id = deg.Id }).ToList();
            return Ok(new { degerlendirme = deg, detaylar });
        }

        [HttpPut("{id}/detaylar")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult UpdateDetaylar(int id, [FromBody] UpdateDetaylarDto dto)
        {
            if (dto.Detaylar.Any(d => d.Puan < 1 || d.Puan > 5))
                return BadRequest(new { mesaj = "Puanlar 1 ile 5 arasinda olmalidir." });

            using var connection = new SqlConnection(_connectionString);

            var calisanId = connection.QueryFirstOrDefault<int?>("SELECT CalisanId FROM Degerlendirmeler WHERE Id = @Id", new { Id = id });
            if (calisanId == null) return NotFound(new { mesaj = "Degerlendirme bulunamadi" });

            var erisimHatasi = CalisanErisimKontrolu(connection, calisanId.Value);
            if (erisimHatasi != null) return erisimHatasi;

            connection.Execute("DELETE FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @Id", new { Id = id });
            foreach (var d in dto.Detaylar)
            {
                connection.Execute("INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan) VALUES (@DegerlendirmeId, @AltKriterId, @Puan)",
                    new { DegerlendirmeId = id, d.AltKriterId, d.Puan });
            }

            // Toplam skor istemciden gelen degerle degil, kaydedilen detaylardan sunucuda yeniden hesaplanir
            var hesaplananSkor = SkorHesaplayici.Hesapla(connection, id);
            connection.Execute("UPDATE Degerlendirmeler SET Yorum=@Yorum, ToplamSkor=@ToplamSkor, Tarih=@Tarih WHERE Id=@Id",
                new { dto.Yorum, ToplamSkor = hesaplananSkor, Tarih = DateTime.Now, Id = id });

            return Ok(new { mesaj = "Degerlendirme guncellendi", toplamSkor = hesaplananSkor });
        }

        // Employee sadece kendi verisine, Evaluator sadece kendi ekibindeki calisanlara, Admin ise herkese erisebilir
        private IActionResult? CalisanErisimKontrolu(SqlConnection connection, int calisanId)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Admin") return null;

            if (rol == "Employee")
            {
                var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                return calisanId == kullaniciId ? null : Forbid();
            }

            if (rol == "Evaluator")
                return ErisimKontrol.EvaluatorKendiEkibindeMi(connection, User, calisanId) ? null : Forbid();

            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult CreateDegerlendirme([FromBody] Degerlendirme yeni)
        {
            using var connection = new SqlConnection(_connectionString);

            var erisimHatasi = CalisanErisimKontrolu(connection, yeni.CalisanId);
            if (erisimHatasi != null) return erisimHatasi;

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Evaluator")
                yeni.DegerlendiricId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var mevcutMu = connection.QueryFirstOrDefault<int?>(
                "SELECT Id FROM Degerlendirmeler WHERE CalisanId = @CalisanId AND Donem = @Donem",
                new { yeni.CalisanId, yeni.Donem });
            if (mevcutMu != null)
                return Conflict(new { mesaj = "Bu calisan icin bu doneme ait bir degerlendirme zaten mevcut.", id = mevcutMu });

            // Toplam skor istemciden gelen degerle degil, kayitli detaylardan hesaplanir;
            // olusturma anda henuz detay girilmedigi icin baslangicta 0 kaydedilir ve
            // her DegerlendirmeDetaylar eklendikce sunucuda yeniden hesaplanip guncellenir.
            var sql = @"INSERT INTO Degerlendirmeler
                (DegerlendiricId, CalisanId, Tarih, Donem, Yorum, ToplamSkor)
                OUTPUT INSERTED.Id
                VALUES (@DegerlendiricId, @CalisanId, @Tarih, @Donem, @Yorum, 0)";
            var yeniId = connection.ExecuteScalar<int>(sql, yeni);
            return Ok(new { mesaj = "Degerlendirme eklendi", id = yeniId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Evaluator")]
        public IActionResult UpdateDegerlendirme(int id, [FromBody] Degerlendirme guncellendi)
        {
            using var connection = new SqlConnection(_connectionString);

            var mevcut = connection.QueryFirstOrDefault<Degerlendirme>("SELECT * FROM Degerlendirmeler WHERE Id = @Id", new { Id = id });
            if (mevcut == null) return NotFound(new { mesaj = "Degerlendirme bulunamadi" });

            var erisimHatasi = CalisanErisimKontrolu(connection, mevcut.CalisanId);
            if (erisimHatasi != null) return erisimHatasi;

            if (guncellendi.CalisanId != mevcut.CalisanId)
            {
                var yeniCalisanErisimHatasi = CalisanErisimKontrolu(connection, guncellendi.CalisanId);
                if (yeniCalisanErisimHatasi != null) return yeniCalisanErisimHatasi;
            }

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol == "Evaluator")
                guncellendi.DegerlendiricId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Toplam skor burada da client'tan degil, kayitli detaylardan sunucuda hesaplanir
            var hesaplananSkor = SkorHesaplayici.Hesapla(connection, id);
            var sql = "UPDATE Degerlendirmeler SET DegerlendiricId=@DegerlendiricId, CalisanId=@CalisanId, Tarih=@Tarih, Donem=@Donem, Yorum=@Yorum, ToplamSkor=@ToplamSkor WHERE Id=@Id";
            connection.Execute(sql, new
            {
                guncellendi.DegerlendiricId,
                guncellendi.CalisanId,
                guncellendi.Tarih,
                guncellendi.Donem,
                guncellendi.Yorum,
                ToplamSkor = hesaplananSkor,
                Id = id
            });
            return Ok(new { mesaj = "Degerlendirme guncellendi", toplamSkor = hesaplananSkor });
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
            var erisimHatasi = CalisanErisimKontrolu(connection, calisanId);
            if (erisimHatasi != null) return erisimHatasi;

            var sql = @"
                SELECT
                    ab.Baslik AS baslik,
                    ab.AgirlikYuzdesi AS agirlikYuzdesi,
                    AVG(CAST(dd.Puan AS FLOAT)) AS ortalamaPuan
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
                double kategorSkor = (kategori.agirlikYuzdesi / 100.0) * (kategori.ortalamaPuan / 5.0) * 100.0;
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
                    SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
                           AVG(d.ToplamSkor) AS ortalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY ortalamaToplamSkor DESC";
            }
            else if (rol == "Evaluator")
            {
                sql = $@"
                    SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
                           AVG(d.ToplamSkor) AS ortalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    WHERE k.Rol = 'Employee' AND k.EvaluatorId = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY ortalamaToplamSkor DESC";
            }
            else
            {
                sql = $@"
                    SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
                           AVG(d.ToplamSkor) AS ortalamaToplamSkor
                    FROM Kullanicilar k
                    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId {donemFilter}
                    WHERE k.Id = @KullaniciId
                    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
                    ORDER BY ortalamaToplamSkor DESC";
            }

            var sonuc = connection.Query(sql, new { KullaniciId = kullaniciId, Donem = donem }).ToList();
            return Ok(sonuc);
        }

        [HttpGet("excel")]
        [Authorize(Roles = "Admin")]
        public IActionResult ExcelExport([FromQuery] string? donem)
        {
            using var connection = new SqlConnection(_connectionString);
            var donemParam = string.IsNullOrEmpty(donem) ? null : donem;

            var sql = @"
                SELECT k.Id, k.Ad, k.Soyad, k.Departman,
                       AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                FROM Kullanicilar k
                LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
                WHERE k.Rol = 'Employee'
                GROUP BY k.Id, k.Ad, k.Soyad, k.Departman
                ORDER BY OrtalamaToplamSkor DESC";

            var veriler = connection.Query(sql, new { Donem = donemParam }).ToList();

            var departmanOzet = veriler
                .GroupBy(v => (string)v.Departman)
                .Select(g => new
                {
                    Departman = g.Key,
                    CalisanSayisi = g.Count(),
                    OrtalamaSkor = g.Any(v => v.OrtalamaToplamSkor != null)
                        ? g.Where(v => v.OrtalamaToplamSkor != null).Average(v => (double)v.OrtalamaToplamSkor)
                        : (double?)null
                })
                .OrderByDescending(d => d.OrtalamaSkor ?? -1)
                .ToList();

            var kategoriler = connection.Query<string>(
                "SELECT Baslik FROM AnaBasliklar WHERE AktifMi = 1 ORDER BY AgirlikYuzdesi DESC").ToList();

            var kategoriSql = @"
                SELECT d.CalisanId AS CalisanId, ab.Baslik AS Baslik, AVG(CAST(dd.Puan AS FLOAT)) AS OrtalamaPuan
                FROM DegerlendirmeDetaylar dd
                INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
                INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
                INNER JOIN Degerlendirmeler d ON dd.DegerlendirmeId = d.Id
                INNER JOIN Kullanicilar k ON d.CalisanId = k.Id
                WHERE k.Rol = 'Employee' AND (@Donem IS NULL OR d.Donem = @Donem)
                GROUP BY d.CalisanId, ab.Baslik";

            var kategoriPuanlari = new Dictionary<(int CalisanId, string Baslik), double>();
            foreach (var kv in connection.Query(kategoriSql, new { Donem = donemParam }))
            {
                kategoriPuanlari[((int)kv.CalisanId, (string)kv.Baslik)] = (double)kv.OrtalamaPuan;
            }

            var baslikRengi = System.Drawing.ColorTranslator.FromHtml("#F1F5F9");
            var baslikYaziRengi = System.Drawing.ColorTranslator.FromHtml("#334155");
            var cizgiRengi = System.Drawing.ColorTranslator.FromHtml("#E2E8F0");

            static System.Drawing.Color SkorRenk(double? skor) => skor switch
            {
                >= 80 => System.Drawing.ColorTranslator.FromHtml("#16A34A"),
                >= 60 => System.Drawing.ColorTranslator.FromHtml("#D97706"),
                null => System.Drawing.ColorTranslator.FromHtml("#94A3B8"),
                _ => System.Drawing.ColorTranslator.FromHtml("#DC2626"),
            };

            void BaslikStilVer(OfficeOpenXml.ExcelRange aralik)
            {
                aralik.Style.Font.Bold = true;
                aralik.Style.Font.Color.SetColor(baslikYaziRengi);
                aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                aralik.Style.Fill.BackgroundColor.SetColor(baslikRengi);
                aralik.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                aralik.Style.Border.Bottom.Color.SetColor(cizgiRengi);
            }

            using var paket = new OfficeOpenXml.ExcelPackage();

            // Sayfa 1: Genel Sıralama
            var s1 = paket.Workbook.Worksheets.Add("Genel Sıralama");
            s1.Cells[1, 1].Value = "Ad Soyad";
            s1.Cells[1, 2].Value = "Departman";
            s1.Cells[1, 3].Value = "Ortalama Skor";
            BaslikStilVer(s1.Cells[1, 1, 1, 3]);
            s1.Cells[1, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            for (int i = 0; i < veriler.Count; i++)
            {
                var v = veriler[i];
                double? skor = v.OrtalamaToplamSkor;
                int satir = i + 2;

                s1.Cells[satir, 1].Value = $"{v.Ad} {v.Soyad}";
                s1.Cells[satir, 2].Value = v.Departman;
                if (skor.HasValue) s1.Cells[satir, 3].Value = Math.Round(skor.Value, 1);
                s1.Cells[satir, 3].Style.Numberformat.Format = "0.0";
                s1.Cells[satir, 3].Style.Font.Bold = true;
                s1.Cells[satir, 3].Style.Font.Color.SetColor(SkorRenk(skor));
                s1.Cells[satir, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            if (veriler.Count > 0) s1.Cells[1, 1, veriler.Count + 1, 3].AutoFilter = true;
            s1.View.FreezePanes(2, 1);
            s1.Cells[s1.Dimension.Address].AutoFitColumns();

            // Sayfa 2: Departman Özeti
            var s2 = paket.Workbook.Worksheets.Add("Departman Özeti");
            s2.Cells[1, 1].Value = "Departman";
            s2.Cells[1, 2].Value = "Çalışan Sayısı";
            s2.Cells[1, 3].Value = "Ortalama Skor";
            BaslikStilVer(s2.Cells[1, 1, 1, 3]);
            s2.Cells[1, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            s2.Cells[1, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            for (int i = 0; i < departmanOzet.Count; i++)
            {
                var d = departmanOzet[i];
                int satir = i + 2;

                s2.Cells[satir, 1].Value = d.Departman;
                s2.Cells[satir, 2].Value = d.CalisanSayisi;
                s2.Cells[satir, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (d.OrtalamaSkor.HasValue) s2.Cells[satir, 3].Value = Math.Round(d.OrtalamaSkor.Value, 1);
                s2.Cells[satir, 3].Style.Numberformat.Format = "0.0";
                s2.Cells[satir, 3].Style.Font.Bold = true;
                s2.Cells[satir, 3].Style.Font.Color.SetColor(SkorRenk(d.OrtalamaSkor));
                s2.Cells[satir, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            if (departmanOzet.Count > 0) s2.Cells[1, 1, departmanOzet.Count + 1, 3].AutoFilter = true;
            s2.View.FreezePanes(2, 1);
            s2.Cells[s2.Dimension.Address].AutoFitColumns();

            // Sayfa 3: Kategori Puanları
            if (kategoriler.Count > 0)
            {
                var s3 = paket.Workbook.Worksheets.Add("Kategori Puanları");
                s3.Cells[1, 1].Value = "Ad Soyad";
                for (int k = 0; k < kategoriler.Count; k++) s3.Cells[1, k + 2].Value = kategoriler[k];
                BaslikStilVer(s3.Cells[1, 1, 1, kategoriler.Count + 1]);

                for (int i = 0; i < veriler.Count; i++)
                {
                    var v = veriler[i];
                    int calisanId = (int)v.Id;
                    int satir = i + 2;

                    s3.Cells[satir, 1].Value = $"{v.Ad} {v.Soyad}";
                    for (int k = 0; k < kategoriler.Count; k++)
                    {
                        var puan = kategoriPuanlari.TryGetValue((calisanId, kategoriler[k]), out var p) ? p : (double?)null;
                        var hucre = s3.Cells[satir, k + 2];
                        if (puan.HasValue) hucre.Value = Math.Round(puan.Value, 1);
                        hucre.Style.Numberformat.Format = "0.0";
                        hucre.Style.Font.Bold = true;
                        hucre.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        hucre.Style.Font.Color.SetColor(puan switch
                        {
                            >= 4 => System.Drawing.ColorTranslator.FromHtml("#16A34A"),
                            >= 3 => System.Drawing.ColorTranslator.FromHtml("#D97706"),
                            null => System.Drawing.ColorTranslator.FromHtml("#94A3B8"),
                            _ => System.Drawing.ColorTranslator.FromHtml("#DC2626"),
                        });
                    }
                }
                if (veriler.Count > 0) s3.Cells[1, 1, veriler.Count + 1, kategoriler.Count + 1].AutoFilter = true;
                s3.View.FreezePanes(2, 2);
                s3.Cells[s3.Dimension.Address].AutoFitColumns();
            }

            paket.Workbook.View.ActiveTab = 0;

            var dosyaAdi = string.IsNullOrEmpty(donem) ? "PerformansRaporu.xlsx" : $"PerformansRaporu_{donem.Replace(" ", "")}.xlsx";
            var dosya = paket.GetAsByteArray();
            return File(dosya, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
        }

        [HttpGet("pdf")]
        [Authorize(Roles = "Admin")]
        public IActionResult PdfExport([FromQuery] string? donem)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var connection = new SqlConnection(_connectionString);
            var donemParam = string.IsNullOrEmpty(donem) ? null : donem;
            var donemMetni = donemParam ?? "Tüm Dönemler";

            var sql = @"
                SELECT k.Id, k.Ad, k.Soyad, k.Departman,
                       AVG(d.ToplamSkor) AS OrtalamaToplamSkor
                FROM Kullanicilar k
                LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
                WHERE k.Rol = 'Employee'
                GROUP BY k.Id, k.Ad, k.Soyad, k.Departman
                ORDER BY OrtalamaToplamSkor DESC";

            var veriler = connection.Query(sql, new { Donem = donemParam }).ToList();

            var degerlendirilenler = veriler.Where(v => v.OrtalamaToplamSkor != null).ToList();
            int toplamCalisan = veriler.Count;
            double? genelOrtalama = degerlendirilenler.Count > 0 ? degerlendirilenler.Average(v => (double)v.OrtalamaToplamSkor) : null;
            var enYuksek = degerlendirilenler.Count > 0 ? degerlendirilenler.OrderByDescending(v => (double)v.OrtalamaToplamSkor).First() : null;
            var enDusuk = degerlendirilenler.Count > 0 ? degerlendirilenler.OrderBy(v => (double)v.OrtalamaToplamSkor).First() : null;

            var departmanOzet = veriler
                .GroupBy(v => (string)v.Departman)
                .Select(g => new
                {
                    Departman = g.Key,
                    CalisanSayisi = g.Count(),
                    OrtalamaSkor = g.Any(v => v.OrtalamaToplamSkor != null)
                        ? g.Where(v => v.OrtalamaToplamSkor != null).Average(v => (double)v.OrtalamaToplamSkor)
                        : (double?)null
                })
                .OrderByDescending(d => d.OrtalamaSkor ?? -1)
                .ToList();

            var kategoriler = connection.Query<string>(
                "SELECT Baslik FROM AnaBasliklar WHERE AktifMi = 1 ORDER BY AgirlikYuzdesi DESC").ToList();

            var kategoriSql = @"
                SELECT d.CalisanId AS CalisanId, ab.Baslik AS Baslik, AVG(CAST(dd.Puan AS FLOAT)) AS OrtalamaPuan
                FROM DegerlendirmeDetaylar dd
                INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
                INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
                INNER JOIN Degerlendirmeler d ON dd.DegerlendirmeId = d.Id
                INNER JOIN Kullanicilar k ON d.CalisanId = k.Id
                WHERE k.Rol = 'Employee' AND (@Donem IS NULL OR d.Donem = @Donem)
                GROUP BY d.CalisanId, ab.Baslik";

            var kategoriPuanlari = new Dictionary<(int CalisanId, string Baslik), double>();
            foreach (var kv in connection.Query(kategoriSql, new { Donem = donemParam }))
            {
                kategoriPuanlari[((int)kv.CalisanId, (string)kv.Baslik)] = (double)kv.OrtalamaPuan;
            }

            var bgSayfa = Colors.White;
            var bgBaslikTablo = Color.FromHex("#F1F5F9");
            var metinAna = Color.FromHex("#1E293B");
            var metinBaslikTablo = Color.FromHex("#334155");
            var metinSoluk = Color.FromHex("#64748B");
            var cizgiRengi = Color.FromHex("#E2E8F0");

            static Color skorRengi(double? skor) => skor switch
            {
                >= 80 => Color.FromHex("#16A34A"),
                >= 60 => Color.FromHex("#D97706"),
                null => Color.FromHex("#94A3B8"),
                _ => Color.FromHex("#DC2626"),
            };

            static Color kategoriRengi(double? puan) => puan switch
            {
                >= 4 => Color.FromHex("#16A34A"),
                >= 3 => Color.FromHex("#D97706"),
                null => Color.FromHex("#94A3B8"),
                _ => Color.FromHex("#DC2626"),
            };

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(57);
                    page.PageColor(bgSayfa);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(metinAna));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("IT Departmanı Performans Raporu")
                            .FontSize(18).Bold().FontColor(metinAna);
                        col.Item().Text($"Dönem: {donemMetni}  |  Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(10).FontColor(metinSoluk);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(cizgiRengi);
                    });

                    page.Content().PaddingTop(20).Column(mainCol =>
                    {
                        mainCol.Item().PaddingBottom(24).Row(row =>
                        {
                            void Divider() => row.ConstantItem(1).Height(48).Background(cizgiRengi);

                            row.RelativeItem().Padding(4).Column(c =>
                            {
                                c.Item().Text("TOPLAM ÇALIŞAN").FontSize(8).FontColor(metinSoluk).Bold();
                                c.Item().PaddingTop(4).Text(toplamCalisan.ToString()).FontSize(22).Bold().FontColor(metinAna);
                            });
                            Divider();
                            row.RelativeItem().Padding(4).Column(c =>
                            {
                                c.Item().Text("GENEL ORTALAMA").FontSize(8).FontColor(metinSoluk).Bold();
                                c.Item().PaddingTop(4).Text(genelOrtalama.HasValue ? genelOrtalama.Value.ToString("F1") : "-")
                                    .FontSize(22).Bold().FontColor(skorRengi(genelOrtalama));
                            });
                            Divider();
                            row.RelativeItem().Padding(4).Column(c =>
                            {
                                c.Item().Text("EN YÜKSEK").FontSize(8).FontColor(metinSoluk).Bold();
                                c.Item().PaddingTop(4).Text(enYuksek != null ? $"{enYuksek.Ad} {enYuksek.Soyad}" : "-")
                                    .FontSize(11).Bold().FontColor(metinAna);
                                if (enYuksek != null)
                                    c.Item().Text(((double)enYuksek.OrtalamaToplamSkor).ToString("F1")).FontSize(13).Bold().FontColor(Color.FromHex("#16A34A"));
                            });
                            Divider();
                            row.RelativeItem().Padding(4).Column(c =>
                            {
                                c.Item().Text("EN DÜŞÜK").FontSize(8).FontColor(metinSoluk).Bold();
                                c.Item().PaddingTop(4).Text(enDusuk != null ? $"{enDusuk.Ad} {enDusuk.Soyad}" : "-")
                                    .FontSize(11).Bold().FontColor(metinAna);
                                if (enDusuk != null)
                                    c.Item().Text(((double)enDusuk.OrtalamaToplamSkor).ToString("F1")).FontSize(13).Bold().FontColor(Color.FromHex("#DC2626"));
                            });
                        });

                        mainCol.Item().Text("Departman Karşılaştırması").FontSize(13).Bold().FontColor(metinAna);

                        mainCol.Item().PaddingTop(8).PaddingBottom(24).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Departman").FontColor(metinBaslikTablo).Bold();
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Çalışan Sayısı").FontColor(metinBaslikTablo).Bold();
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Ortalama Skor").FontColor(metinBaslikTablo).Bold();
                            });

                            foreach (var d in departmanOzet)
                            {
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(d.Departman ?? "-").FontColor(metinAna);
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(d.CalisanSayisi.ToString()).FontColor(metinSoluk);
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(d.OrtalamaSkor.HasValue ? d.OrtalamaSkor.Value.ToString("F1") : "-")
                                    .Bold().FontColor(skorRengi(d.OrtalamaSkor));
                            }
                        });

                        mainCol.Item().Text("Genel Sıralama").FontSize(13).Bold().FontColor(metinAna);

                        mainCol.Item().PaddingTop(8).PaddingBottom(24).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(35);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .AlignCenter().Text("#").FontColor(metinBaslikTablo).Bold();
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Ad Soyad").FontColor(metinBaslikTablo).Bold();
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Departman").FontColor(metinBaslikTablo).Bold();
                                header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                    .Text("Ortalama Skor").FontColor(metinBaslikTablo).Bold();
                            });

                            for (int i = 0; i < veriler.Count; i++)
                            {
                                var v = veriler[i];
                                double? skor = v.OrtalamaToplamSkor;

                                string ad = $"{v.Ad} {v.Soyad}";
                                string departman = (string?)v.Departman ?? "-";
                                string skorStr = skor.HasValue ? skor.Value.ToString("F1") : "-";

                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).AlignCenter().Text((i + 1).ToString()).FontColor(metinSoluk);
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(ad).FontColor(metinAna);
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(departman).FontColor(metinSoluk);
                                table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text(skorStr).Bold().FontColor(skorRengi(skor));
                            }
                        });

                        if (kategoriler.Count > 0)
                        {
                            mainCol.Item().PageBreak();
                            mainCol.Item().Text("Kategori Bazlı Puanlar").FontSize(13).Bold().FontColor(metinAna);
                            mainCol.Item().PaddingTop(2).Text("Alt kriterlerin 5 üzerinden ortalama puanı").FontSize(9).FontColor(metinSoluk);

                            mainCol.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2.4f);
                                    foreach (var _ in kategoriler) cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                        .Text("Ad Soyad").FontColor(metinBaslikTablo).Bold();
                                    foreach (var kategori in kategoriler)
                                    {
                                        header.Cell().Background(bgBaslikTablo).BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6)
                                            .AlignCenter().Text(kategori).FontSize(9).FontColor(metinBaslikTablo).Bold();
                                    }
                                });

                                foreach (var v in veriler)
                                {
                                    int calisanId = (int)v.Id;

                                    table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).Text($"{v.Ad} {v.Soyad}").FontColor(metinAna);
                                    foreach (var kategori in kategoriler)
                                    {
                                        var puan = kategoriPuanlari.TryGetValue((calisanId, kategori), out var p) ? p : (double?)null;
                                        var puanStr = puan.HasValue ? puan.Value.ToString("F1") : "-";
                                        table.Cell().BorderBottom(1).BorderColor(cizgiRengi).PaddingVertical(8).PaddingHorizontal(6).AlignCenter().Text(puanStr).Bold().FontColor(kategoriRengi(puan));
                                    }
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontColor(metinSoluk));
                        t.Span("IT Performans Değerlendirme Sistemi  |  Sayfa ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });

            var bytes = pdf.GeneratePdf();
            var dosyaAdi = string.IsNullOrEmpty(donem) ? "PerformansRaporu.pdf" : $"PerformansRaporu_{donem.Replace(" ", "")}.pdf";
            return File(bytes, "application/pdf", dosyaAdi);
        }
    }
}