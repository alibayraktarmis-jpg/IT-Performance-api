CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_KategoriDetay
    @CalisanId INT,
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ab.Baslik AS baslik,
        ab.AgirlikYuzdesi AS agirlikYuzdesi,
        AVG(CAST(dd.Puan AS FLOAT)) AS ortalamaPuan
    FROM DegerlendirmeDetaylar dd
    INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
    INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
    INNER JOIN Degerlendirmeler d ON dd.DegerlendirmeId = d.Id
    WHERE d.CalisanId = @CalisanId
      AND (@Donem IS NULL OR d.Donem = @Donem)
    GROUP BY ab.Id, ab.Baslik, ab.AgirlikYuzdesi;
END
