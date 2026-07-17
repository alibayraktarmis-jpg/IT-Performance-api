CREATE OR ALTER PROCEDURE usp_Kullanicilar_AktifMiKontrol
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET SonAktiflikZamani = GETDATE() WHERE Id = @Id;
    SELECT AktifMi, Rol FROM Kullanicilar WHERE Id = @Id;
END
