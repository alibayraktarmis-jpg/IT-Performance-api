CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_SkorHesapla
    @DegerlendirmeId INT,
    @Skor FLOAT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @Skor = ISNULL(SUM((ab.AgirlikYuzdesi / 100.0) * (Kategori.OrtalamaPuan / 5.0) * 100.0), 0)
    FROM (
        SELECT ak.AnaBaslikId, AVG(CAST(dd.Puan AS FLOAT)) AS OrtalamaPuan
        FROM DegerlendirmeDetaylar dd
        INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
        WHERE dd.DegerlendirmeId = @DegerlendirmeId
        GROUP BY ak.AnaBaslikId
    ) AS Kategori
    INNER JOIN AnaBasliklar ab ON ab.Id = Kategori.AnaBaslikId;

    SET @Skor = ROUND(ISNULL(@Skor, 0), 2);
END
