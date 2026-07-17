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

    -- Alt kriter adi ve ana baslik adi da eklendi; Employee'nin Gecmis sayfasinda
    -- donem satirini actiginda kriter bazli kirilim gosterebilmesi icin.
    SELECT dd.Id, dd.DegerlendirmeId, dd.AltKriterId, dd.Puan,
           ak.KriterAdi, ab.Baslik AS AnaBaslikAdi
    FROM DegerlendirmeDetaylar dd
    INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
    INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
    WHERE dd.DegerlendirmeId = @DegerlendirmeId
    ORDER BY ab.Id, ak.Id;
END
