-- Kullanicilar tablosuna kayit ve son giris tarihi kolonlarini ekler.
-- KayitTarihi mevcut satirlar icin bu script'in calistirildigi ana ayarlanir
-- (gercek gecmis kayit tarihleri bilinmedigi icin uydurulmaz).
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Kullanicilar') AND name = 'KayitTarihi')
BEGIN
    ALTER TABLE Kullanicilar ADD KayitTarihi DATETIME NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Kullanicilar') AND name = 'SonGirisTarihi')
BEGIN
    ALTER TABLE Kullanicilar ADD SonGirisTarihi DATETIME NULL;
END
