using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ITPerformansAPI.Models;
using ITPerformansAPI.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

            // Yanitta bir onceki girisin tarihi donulur (bu giristen hemen once); sonra
            // veritabani bu girisin zamaniyla guncellenir (bir sonraki giris icin referans olsun).
            var oncekiGiris = kullanici.SonGirisTarihi;
            connection.Execute("usp_Kullanicilar_SonGirisGuncelle", new { kullanici.Id }, commandType: CommandType.StoredProcedure);

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
                kullanici.Email,
                kullanici.Rol,
                kullanici.Departman,
                kullanici.KayitTarihi,
                sonGirisTarihi = oncekiGiris
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

        [HttpPut("sifre-degistir")]
        [Authorize]
        public IActionResult SifreDegistir([FromBody] SifreDegistirDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.YeniSifre) || dto.YeniSifre.Length < 6)
                return BadRequest(new { mesaj = "Yeni şifre en az 6 karakter olmalıdır." });

            var kullaniciId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            using var connection = new SqlConnection(_connectionString);

            var mevcutHash = connection.QueryFirstOrDefault<string>(
                "usp_Kullanicilar_GetSifreHash", new { Id = kullaniciId },
                commandType: CommandType.StoredProcedure);

            if (mevcutHash == null || !BCrypt.Net.BCrypt.Verify(dto.MevcutSifre, mevcutHash))
                return BadRequest(new { mesaj = "Mevcut şifre yanlış." });

            var yeniHash = BCrypt.Net.BCrypt.HashPassword(dto.YeniSifre);
            connection.Execute("usp_Kullanicilar_SifreDegistir",
                new { Id = kullaniciId, YeniSifre = yeniHash },
                commandType: CommandType.StoredProcedure);

            return Ok(new { mesaj = "Şifreniz başarıyla değiştirildi." });
        }

        [HttpPost("sifremi-unuttum")]
        public IActionResult SifremiUnuttum([FromBody] SifremiUnuttumDto dto)
        {
            using var connection = new SqlConnection(_connectionString);

            var kullanici = connection.QueryFirstOrDefault<Kullanici>(
                "usp_Kullanicilar_GetByEmail", new { Email = dto.Email },
                commandType: CommandType.StoredProcedure);

            if (kullanici != null && kullanici.AktifMi)
            {
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    .Replace("+", "-").Replace("/", "_").Replace("=", "");

                connection.Execute("usp_SifreSifirlama_TokenOlustur",
                    new { KullaniciId = kullanici.Id, Token = token, GecerlilikDakika = 15 },
                    commandType: CommandType.StoredProcedure);

                var link = $"{_configuration["FrontendUrl"]}/sifre-sifirla?token={token}";
                try
                {
                    EmailGonderici.SifreSifirlamaMailiGonder(_configuration, kullanici.Email, link);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sifre sifirlama maili gonderilemedi: {ex.Message}");
                }
            }

            return Ok(new { mesaj = "Eğer bu email adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderildi." });
        }

        [HttpPost("sifre-sifirla")]
        public IActionResult SifreSifirla([FromBody] SifreSifirlaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.YeniSifre) || dto.YeniSifre.Length < 6)
                return BadRequest(new { mesaj = "Yeni şifre en az 6 karakter olmalıdır." });

            using var connection = new SqlConnection(_connectionString);
            var yeniHash = BCrypt.Net.BCrypt.HashPassword(dto.YeniSifre);

            var parametreler = new DynamicParameters();
            parametreler.Add("Token", dto.Token);
            parametreler.Add("YeniSifreHash", yeniHash);
            parametreler.Add("BasariliMi", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            connection.Execute("usp_SifreSifirlama_SifreyiSifirla", parametreler, commandType: CommandType.StoredProcedure);

            var basarili = parametreler.Get<bool>("BasariliMi");
            if (!basarili)
                return BadRequest(new { mesaj = "Bağlantı geçersiz veya süresi dolmuş. Lütfen şifremi unuttum işlemini tekrar başlatın." });

            return Ok(new { mesaj = "Şifreniz başarıyla sıfırlandı. Şimdi giriş yapabilirsiniz." });
        }
    }
}