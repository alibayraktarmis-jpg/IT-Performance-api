CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_OrtalamaSkor
    @CalisanId INT,
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AVG(ToplamSkor)
    FROM Degerlendirmeler
    WHERE CalisanId = @CalisanId
      AND (@Donem IS NULL OR Donem = @Donem);
END
