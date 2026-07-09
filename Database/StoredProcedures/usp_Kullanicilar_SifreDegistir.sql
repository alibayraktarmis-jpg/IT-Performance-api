CREATE OR ALTER PROCEDURE usp_Kullanicilar_SifreDegistir
    @Id INT,
    @YeniSifre NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET Sifre = @YeniSifre WHERE Id = @Id;
END
