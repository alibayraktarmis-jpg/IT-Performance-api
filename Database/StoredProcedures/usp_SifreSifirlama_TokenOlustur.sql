CREATE OR ALTER PROCEDURE usp_SifreSifirlama_TokenOlustur
    @KullaniciId INT,
    @Token NVARCHAR(100),
    @GecerlilikDakika INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO SifreSifirlamaTokenlari (KullaniciId, Token, OlusturmaZamani, SonKullanmaTarihi, KullanildiMi)
    VALUES (@KullaniciId, @Token, GETDATE(), DATEADD(MINUTE, @GecerlilikDakika, GETDATE()), 0);
END
