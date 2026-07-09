-- Her istekte (middleware icinde) kullanicinin hala aktif olup olmadigini,
-- var olup olmadigini ve rolunun token'daki roldan farkli olup olmadigini
-- kontrol etmek icin kullanilir.
CREATE OR ALTER PROCEDURE usp_Kullanicilar_AktifMiKontrol
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AktifMi, Rol FROM Kullanicilar WHERE Id = @Id;
END
