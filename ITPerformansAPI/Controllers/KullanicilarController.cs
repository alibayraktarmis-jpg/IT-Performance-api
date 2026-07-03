using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITPerformansAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullanicilarController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public KullanicilarController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);

            if (rol == "Evaluator")
            {
                var liste = connection.Query<Kullanici>(
                    "SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi FROM Kullanicilar WHERE Rol = 'Employee' AND EvaluatorId = @Id",
                    new { Id = kullaniciId }).ToList();
                return Ok(liste);
            }

            if (rol == "Employee")
            {
                var kendisi = connection.Query<Kullanici>(
                    "SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi FROM Kullanicilar WHERE Id = @Id",
                    new { Id = kullaniciId }).ToList();
                return Ok(kendisi);
            }

            var tumListe = connection.Query<Kullanici>(
                "SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi, EvaluatorId FROM Kullanicilar"
            ).ToList();
            return Ok(tumListe);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateKullanici([FromBody] Kullanici yeniKullanici)
        {
            using var connection = new SqlConnection(_connectionString);
            yeniKullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(yeniKullanici.Sifre);
            var sql = "INSERT INTO Kullanicilar (Ad, Soyad, Email, Sifre, Rol, Departman, EvaluatorId) VALUES (@Ad, @Soyad, @Email, @Sifre, @Rol, @Departman, @EvaluatorId)";
            connection.Execute(sql, yeniKullanici);
            return Ok(new { mesaj = "Kullanici basariyla eklendi" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            using var connection = new SqlConnection(_connectionString);
            var kullanici = connection.QueryFirstOrDefault<Kullanici>(
                "SELECT * FROM Kullanicilar WHERE Email = @Email",
                new { Email = loginDto.Email });

            if (kullanici == null) return Unauthorized(new { mesaj = "Email veya sifre yanlis" });

            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(loginDto.Sifre, kullanici.Sifre);
            if (!sifreDogruMu) return Unauthorized(new { mesaj = "Email veya sifre yanlis" });

            if (!kullanici.AktifMi) return Unauthorized(new { mesaj = "Hesabınız pasife alınmış. Yöneticinizle iletişime geçin." });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Email, kullanici.Email),
                new Claim(ClaimTypes.Role, kullanici.Rol),
                new Claim("Ad", kullanici.Ad)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                kullanici.Id,
                kullanici.Ad,
                kullanici.Soyad,
                kullanici.Rol,
                kullanici.Departman
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateKullanici(int id, [FromBody] UpdateKullaniciDto guncelKullanici)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Kullanicilar SET Ad=@Ad, Soyad=@Soyad, Email=@Email, Rol=@Rol, Departman=@Departman, EvaluatorId=@EvaluatorId WHERE Id=@Id";
            var etkilenenSatir = connection.Execute(sql, new
            {
                guncelKullanici.Ad,
                guncelKullanici.Soyad,
                guncelKullanici.Email,
                guncelKullanici.Rol,
                guncelKullanici.Departman,
                guncelKullanici.EvaluatorId,
                Id = id
            });
            if (etkilenenSatir == 0) return NotFound(new { mesaj = "Guncellenecek kullanici bulunamadi" });
            return Ok(new { mesaj = "Kullanici basariyla guncellendi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteKullanici(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM Kullanicilar WHERE Id=@Id", new { Id = id });
            return Ok(new { mesaj = "Kullanici silindi" });
        }

        [HttpPatch("{id}/aktif")]
        [Authorize(Roles = "Admin")]
        public IActionResult AktifPasifYap(int id, [FromBody] bool aktifMi)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Kullanicilar SET AktifMi = @AktifMi WHERE Id = @Id";
            connection.Execute(sql, new { AktifMi = aktifMi, Id = id });
            return Ok(new { mesaj = aktifMi ? "Kullanici aktif edildi" : "Kullanici pasif edildi" });
        }
    }
}