CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Update
    @Id INT,
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KriterAciklamalar
    SET AltKriterId = @AltKriterId, Rol = @Rol, Aciklama = @Aciklama
    WHERE Id = @Id;
END
