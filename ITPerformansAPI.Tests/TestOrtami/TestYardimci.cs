using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ITPerformansAPI.Tests.TestOrtami
{
    public static class TestYardimci
    {
        public static HttpClient YetkiliClient(this ApiFactory fabrika, int kullaniciId, string email, string rol, string ad = "Test")
        {
            var yapilandirma = fabrika.Services.GetRequiredService<IConfiguration>();
            var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(yapilandirma["Jwt:Key"]!));
            var kimlik = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);

            var talepler = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kullaniciId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, rol),
                new Claim("Ad", ad)
            };

            var token = new JwtSecurityToken(
                issuer: yapilandirma["Jwt:Issuer"],
                audience: yapilandirma["Jwt:Audience"],
                claims: talepler,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: kimlik);

            var client = fabrika.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer", new JwtSecurityTokenHandler().WriteToken(token));
            return client;
        }

        public static HttpClient AdminClient(this ApiFactory f) =>
            f.YetkiliClient(VeritabaniFixture.AdminId, "admin@test.local", "Admin");

        public static HttpClient EvaluatorAClient(this ApiFactory f) =>
            f.YetkiliClient(VeritabaniFixture.EvaluatorAId, "evaluatora@test.local", "Evaluator");

        public static HttpClient Emp1Client(this ApiFactory f) =>
            f.YetkiliClient(VeritabaniFixture.Emp1Id, "emp1@test.local", "Employee");

        public static HttpClient Emp2Client(this ApiFactory f) =>
            f.YetkiliClient(VeritabaniFixture.Emp2Id, "emp2@test.local", "Employee");
    }
}
