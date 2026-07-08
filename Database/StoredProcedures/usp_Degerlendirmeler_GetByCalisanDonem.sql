-- Iki sonuc kumesi doner: 1) o donemin degerlendirmesi (varsa), 2) onun detaylari.
CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetByCalisanDonem
    @CalisanId INT,
    @Donem NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DegerlendirmeId INT;
    SELECT TOP 1 @DegerlendirmeId = Id
    FROM Degerlendirmeler
    WHERE CalisanId = @CalisanId AND Donem = @Donem
    ORDER BY Id DESC;

    SELECT TOP 1 * FROM Degerlendirmeler
    WHERE CalisanId = @CalisanId AND Donem = @Donem
    ORDER BY Id DESC;

    SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId;
END
