using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Helpers
{
    public class AktifKullaniciMiddleware
    {
        private readonly RequestDelegate _next;

        public AktifKullaniciMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (idClaim != null && int.TryParse(idClaim, out var kullaniciId))
                {
                    using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
                    var kullanici = await connection.QueryFirstOrDefaultAsync(
                        "usp_Kullanicilar_AktifMiKontrol", new { Id = kullaniciId },
                        commandType: CommandType.StoredProcedure);

                    bool aktifMi = kullanici != null && (bool)kullanici.AktifMi;
                    string? guncelRol = kullanici?.Rol;
                    var tokenRol = context.User.FindFirst(ClaimTypes.Role)?.Value;

                    if (!aktifMi)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { mesaj = "Hesabınız pasife alınmış. Yöneticinizle iletişime geçin." });
                        return;
                    }

                    if (guncelRol != null && guncelRol != tokenRol)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { mesaj = "Yetkileriniz değişti. Lütfen tekrar giriş yapın." });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
