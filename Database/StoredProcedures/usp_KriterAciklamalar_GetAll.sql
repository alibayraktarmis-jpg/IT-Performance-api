CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar;
END
