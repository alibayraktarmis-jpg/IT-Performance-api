-- Her istekte (middleware icinde) kullanicinin hala aktif olup olmadigini,
-- var olup olmadigini ve rolunun token'daki roldan farkli olup olmadigini
-- kontrol etmek icin kullanilir. Ayni zamanda "su an cevrimici mi" gostergesi
-- icin son aktiflik zamanini gunceller (ayri bir heartbeat cagrisina gerek kalmaz).
CREATE OR ALTER PROCEDURE usp_Kullanicilar_AktifMiKontrol
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET SonAktiflikZamani = GETDATE() WHERE Id = @Id;
    SELECT AktifMi, Rol FROM Kullanicilar WHERE Id = @Id;
END
