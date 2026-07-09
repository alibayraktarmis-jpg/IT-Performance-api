-- Ayni calisan+donem icin iki degerlendirme olusmasini DB seviyesinde de imkansiz kilar.
-- usp_Degerlendirmeler_Create/Update icindeki "IF EXISTS" kontrolu atomik degildir; iki
-- es zamanli istek ayni anda kontrolu gecebilir. Bu unique index, o yaris durumunda ikinci
-- istegi bir SqlException (2601/2627) ile reddeder; Program.cs'teki global handler bunu
-- temiz bir 409 mesajina cevirir.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Degerlendirmeler_CalisanDonem')
BEGIN
    CREATE UNIQUE INDEX UQ_Degerlendirmeler_CalisanDonem ON Degerlendirmeler(CalisanId, Donem);
END
