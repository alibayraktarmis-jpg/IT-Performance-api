using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ITPerformansAPI.Tests.TestOrtami
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((baglam, yapilandirma) =>
            {
                yapilandirma.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = VeritabaniFixture.BaglantiDizesi
                });
            });
        }
    }

    [CollectionDefinition("Api")]
    public class ApiKoleksiyonu : ICollectionFixture<VeritabaniFixture> { }
}
