CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_DepartmanOzet
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Departman AS departman,
           COUNT(OrtalamaToplamSkor) AS calisanSayisi,
           AVG(OrtalamaToplamSkor) AS ortalamaSkor
    FROM (
        SELECT k.Id, k.Departman, AVG(d.ToplamSkor) AS OrtalamaToplamSkor
        FROM Kullanicilar k
        LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
        WHERE k.Rol = 'Employee'
        GROUP BY k.Id, k.Departman
    ) CalisanOrtalamalari
    GROUP BY Departman
    ORDER BY ortalamaSkor DESC;
END
