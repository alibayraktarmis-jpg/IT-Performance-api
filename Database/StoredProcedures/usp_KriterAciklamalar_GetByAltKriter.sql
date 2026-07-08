CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetByAltKriter
    @AltKriterId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId;
END
