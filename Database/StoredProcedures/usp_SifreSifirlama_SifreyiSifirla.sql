CREATE OR ALTER PROCEDURE usp_SifreSifirlama_SifreyiSifirla
    @Token NVARCHAR(100),
    @YeniSifreHash NVARCHAR(300),
    @BasariliMi BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @BasariliMi = 0;

    BEGIN TRANSACTION;

    DECLARE @KullaniciId INT;
    SELECT @KullaniciId = KullaniciId
    FROM SifreSifirlamaTokenlari WITH (UPDLOCK, ROWLOCK)
    WHERE Token = @Token AND KullanildiMi = 0 AND SonKullanmaTarihi > GETDATE();

    IF @KullaniciId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE Kullanicilar SET Sifre = @YeniSifreHash WHERE Id = @KullaniciId;
    UPDATE SifreSifirlamaTokenlari SET KullanildiMi = 1 WHERE Token = @Token;

    COMMIT TRANSACTION;
    SET @BasariliMi = 1;
END
