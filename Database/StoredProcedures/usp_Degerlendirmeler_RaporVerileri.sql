CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_RaporVerileri
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT k.Id, k.Ad, k.Soyad, k.Departman,
           AVG(d.ToplamSkor) AS OrtalamaToplamSkor
    FROM Kullanicilar k
    LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
    WHERE k.Rol = 'Employee'
    GROUP BY k.Id, k.Ad, k.Soyad, k.Departman
    ORDER BY OrtalamaToplamSkor DESC;
END
