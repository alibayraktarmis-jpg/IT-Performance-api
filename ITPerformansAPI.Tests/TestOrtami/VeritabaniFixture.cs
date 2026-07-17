using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ITPerformansAPI.Tests.TestOrtami
{
    public class VeritabaniFixture
    {
        public const string TestVeritabaniAdi = "ITPerformansDB_Test";
        public const string BaglantiDizesi =
            "Server=localhost\\SQLEXPRESS;Database=" + TestVeritabaniAdi +
            ";Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";

        public const string OrtakSifre = "ClaudeTest123!";
        public const string SifreHash = "$2a$11$OzdhcD6O5LIeGeOi3JDq8OooCFtzS3AnnQqwI8QuIW.0koLK6I.fa";

        public const int AdminId = 1;
        public const int EvaluatorAId = 2;
        public const int Emp1Id = 3;
        public const int Emp2Id = 4;
        public const int EvaluatorBId = 5;
        public const int PasifId = 6;

        public VeritabaniFixture()
        {
            var master = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";
            using (var baglanti = new SqlConnection(master))
            {
                baglanti.Open();
                Calistir(baglanti, $@"
                    IF DB_ID(N'{TestVeritabaniAdi}') IS NOT NULL
                    BEGIN
                        ALTER DATABASE [{TestVeritabaniAdi}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE [{TestVeritabaniAdi}];
                    END");
                Calistir(baglanti, $"CREATE DATABASE [{TestVeritabaniAdi}]");
            }

            var dbKlasoru = DatabaseKlasoruBul();
            using var test = new SqlConnection(BaglantiDizesi);
            test.Open();
            ScriptDosyasiCalistir(test, Path.Combine(dbKlasoru, "01_Tablolar.sql"));
            ScriptDosyasiCalistir(test, Path.Combine(dbKlasoru, "02_StoredProceduresKur.sql"));
            TohumVerisiYukle(test);
        }

        static string DatabaseKlasoruBul()
        {
            var dizin = new DirectoryInfo(AppContext.BaseDirectory);
            while (dizin != null)
            {
                var aday = Path.Combine(dizin.FullName, "Database");
                if (Directory.Exists(aday) && File.Exists(Path.Combine(aday, "01_Tablolar.sql")))
                    return aday;
                dizin = dizin.Parent;
            }
            throw new DirectoryNotFoundException("Database klasoru bulunamadi (01_Tablolar.sql aranıyor).");
        }

        static void ScriptDosyasiCalistir(SqlConnection baglanti, string yol)
        {
            var icerik = File.ReadAllText(yol);
            var parcalar = Regex.Split(icerik, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (var parca in parcalar)
            {
                if (string.IsNullOrWhiteSpace(parca)) continue;
                Calistir(baglanti, parca);
            }
        }

        static void Calistir(SqlConnection baglanti, string sql)
        {
            using var komut = baglanti.CreateCommand();
            komut.CommandText = sql;
            komut.CommandTimeout = 120;
            komut.ExecuteNonQuery();
        }

        void TohumVerisiYukle(SqlConnection baglanti)
        {
            Calistir(baglanti, $@"
                SET IDENTITY_INSERT dbo.Kullanicilar ON;
                INSERT INTO dbo.Kullanicilar (Id, Ad, Soyad, Email, Rol, Departman, Sifre, AktifMi, EvaluatorId) VALUES
                    ({AdminId}, N'Test', N'Admin', 'admin@test.local', 'Admin', N'Yönetim', '{SifreHash}', 1, NULL),
                    ({EvaluatorAId}, N'Test', N'EvaluatorA', 'evaluatora@test.local', 'Evaluator', N'Yazılımcılar', '{SifreHash}', 1, NULL),
                    ({Emp1Id}, N'Test', N'CalisanBir', 'emp1@test.local', 'Employee', N'Yazılımcılar', '{SifreHash}', 1, {EvaluatorAId}),
                    ({Emp2Id}, N'Test', N'CalisanIki', 'emp2@test.local', 'Employee', N'QA/Test Uzmanları', '{SifreHash}', 1, {EvaluatorBId}),
                    ({EvaluatorBId}, N'Test', N'EvaluatorB', 'evaluatorb@test.local', 'Evaluator', N'QA/Test Uzmanları', '{SifreHash}', 1, NULL),
                    ({PasifId}, N'Test', N'Pasif', 'pasif@test.local', 'Employee', N'Yazılımcılar', '{SifreHash}', 0, {EvaluatorAId});
                SET IDENTITY_INSERT dbo.Kullanicilar OFF;

                SET IDENTITY_INSERT dbo.AnaBasliklar ON;
                INSERT INTO dbo.AnaBasliklar (Id, Baslik, AgirlikYuzdesi, AktifMi) VALUES
                    (1, N'Teknik Yetkinlik', 40, 1),
                    (2, N'Takım Çalışması', 30, 1),
                    (3, N'Problem Çözme', 30, 1);
                SET IDENTITY_INSERT dbo.AnaBasliklar OFF;

                SET IDENTITY_INSERT dbo.AltKriterler ON;
                INSERT INTO dbo.AltKriterler (Id, AnaBaslikId, KriterAdi, AktifMi) VALUES
                    (1, 1, N'Kriter 1', 1),
                    (2, 1, N'Kriter 2', 1),
                    (3, 2, N'Kriter 3', 1),
                    (4, 2, N'Kriter 4', 1),
                    (5, 3, N'Kriter 5', 1),
                    (6, 3, N'Kriter 6', 1);
                SET IDENTITY_INSERT dbo.AltKriterler OFF;");
        }
    }
}
