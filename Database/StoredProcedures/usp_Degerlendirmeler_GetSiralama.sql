CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetSiralama
    @Rol NVARCHAR(50),
    @KullaniciId INT,
    @Donem NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Admin'
    BEGIN
        SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
               AVG(d.ToplamSkor) AS ortalamaToplamSkor
        FROM Kullanicilar k
        LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
        GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
        ORDER BY ortalamaToplamSkor DESC;
    END
    ELSE IF @Rol = 'Evaluator'
    BEGIN
        SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
               AVG(d.ToplamSkor) AS ortalamaToplamSkor
        FROM Kullanicilar k
        LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
        WHERE k.Rol = 'Employee' AND k.EvaluatorId = @KullaniciId
        GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
        ORDER BY ortalamaToplamSkor DESC;
    END
    ELSE
    BEGIN
        SELECT k.Id AS id, k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman, k.Rol AS rol,
               AVG(d.ToplamSkor) AS ortalamaToplamSkor
        FROM Kullanicilar k
        LEFT JOIN Degerlendirmeler d ON k.Id = d.CalisanId AND (@Donem IS NULL OR d.Donem = @Donem)
        WHERE k.Id = @KullaniciId
        GROUP BY k.Id, k.Ad, k.Soyad, k.Departman, k.Rol
        ORDER BY ortalamaToplamSkor DESC;
    END
END
