CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Create
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama)
    VALUES (@AltKriterId, @Rol, @Aciklama);
END
