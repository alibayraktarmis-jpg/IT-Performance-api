CREATE OR ALTER PROCEDURE usp_Kullanicilar_SonGirisGuncelle
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET SonGirisTarihi = GETDATE() WHERE Id = @Id;
END
