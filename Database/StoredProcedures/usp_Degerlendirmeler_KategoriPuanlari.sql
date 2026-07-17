CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_KategoriPuanlari
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT d.CalisanId AS CalisanId, ab.Id AS AnaBaslikId, AVG(CAST(dd.Puan AS FLOAT)) AS OrtalamaPuan
    FROM DegerlendirmeDetaylar dd
    INNER JOIN AltKriterler ak ON dd.AltKriterId = ak.Id
    INNER JOIN AnaBasliklar ab ON ak.AnaBaslikId = ab.Id
    INNER JOIN Degerlendirmeler d ON dd.DegerlendirmeId = d.Id
    INNER JOIN Kullanicilar k ON d.CalisanId = k.Id
    WHERE k.Rol = 'Employee' AND (@Donem IS NULL OR d.Donem = @Donem)
    GROUP BY d.CalisanId, ab.Id;
END
