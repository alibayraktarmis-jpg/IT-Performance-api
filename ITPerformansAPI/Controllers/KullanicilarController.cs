using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Data;

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
            var liste = connection.Query<Kullanici>(
                "usp_Kullanicilar_GetAll",
                new { Rol = rol, KullaniciId = kullaniciId },
                commandType: CommandType.StoredProcedure).ToList();
            return Ok(liste);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateKullanici([FromBody] Kullanici yeniKullanici)
        {
            using var connection = new SqlConnection(_connectionString);
            yeniKullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(yeniKullanici.Sifre);

            try
            {
                connection.Execute("usp_Kullanicilar_Create", new
                {
                    yeniKullanici.Ad,
                    yeniKullanici.Soyad,
                    yeniKullanici.Email,
                    yeniKullanici.Sifre,
                    yeniKullanici.Rol,
                    yeniKullanici.Departman,
                    yeniKullanici.EvaluatorId
                }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok(new { mesaj = "Kullanici basariyla eklendi" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            using var connection = new SqlConnection(_connectionString);
            var kullanici = connection.QueryFirstOrDefault<Kullanici>(
                "usp_Kullanicilar_GetByEmail", new { Email = loginDto.Email },
                commandType: CommandType.StoredProcedure);

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
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (id == kullaniciId)
                return BadRequest(new { mesaj = "Kendi hesabınızı düzenleyemezsiniz." });

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var etkilenenSatir = connection.Execute("usp_Kullanicilar_Update", new
                {
                    Id = id,
                    guncelKullanici.Ad,
                    guncelKullanici.Soyad,
                    guncelKullanici.Email,
                    guncelKullanici.Rol,
                    guncelKullanici.Departman,
                    guncelKullanici.EvaluatorId
                }, commandType: CommandType.StoredProcedure);

                if (etkilenenSatir == 0) return NotFound(new { mesaj = "Guncellenecek kullanici bulunamadi" });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok(new { mesaj = "Kullanici basariyla guncellendi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteKullanici(int id)
        {
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (id == kullaniciId)
                return BadRequest(new { mesaj = "Kendi hesabınızı silemezsiniz." });

            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_Kullanicilar_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok(new { mesaj = "Kullanici silindi" });
        }

        [HttpPatch("{id}/aktif")]
        [Authorize(Roles = "Admin")]
        public IActionResult AktifPasifYap(int id, [FromBody] bool aktifMi)
        {
            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (id == kullaniciId)
                return BadRequest(new { mesaj = "Kendi hesabınızı pasife alamazsınız." });

            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Execute("usp_Kullanicilar_AktifPasifYap", new { Id = id, AktifMi = aktifMi }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { mesaj = ex.Message });
            }

            return Ok(new { mesaj = aktifMi ? "Kullanici aktif edildi" : "Kullanici pasif edildi" });
        }
    }
}