CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetSifreHash
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Sifre FROM Kullanicilar WHERE Id = @Id;
END
