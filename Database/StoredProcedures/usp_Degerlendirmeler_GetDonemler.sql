CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetDonemler
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Donem FROM Degerlendirmeler
    WHERE Donem IS NOT NULL AND Donem != ''
    ORDER BY Donem DESC;
END
