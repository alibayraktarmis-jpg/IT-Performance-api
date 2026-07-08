CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetByEmail
    @Email NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Kullanicilar WHERE Email = @Email;
END
