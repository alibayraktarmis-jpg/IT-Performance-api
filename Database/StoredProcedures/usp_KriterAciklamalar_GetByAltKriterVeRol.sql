CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetByAltKriterVeRol
    @AltKriterId INT,
    @Rol NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol;
END
