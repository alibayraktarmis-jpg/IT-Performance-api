-- Sifre sifirlama baglantilari icin tek kullanimlik, sureli tokenlar.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SifreSifirlamaTokenlari')
BEGIN
    CREATE TABLE SifreSifirlamaTokenlari (
        Id INT IDENTITY PRIMARY KEY,
        KullaniciId INT NOT NULL,
        Token NVARCHAR(100) NOT NULL,
        OlusturmaZamani DATETIME NOT NULL DEFAULT GETDATE(),
        SonKullanmaTarihi DATETIME NOT NULL,
        KullanildiMi BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_SifreSifirlamaTokenlari_Kullanicilar FOREIGN KEY (KullaniciId) REFERENCES Kullanicilar(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UQ_SifreSifirlamaTokenlari_Token ON SifreSifirlamaTokenlari(Token);
END
