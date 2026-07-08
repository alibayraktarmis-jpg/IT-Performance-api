-- Rol bazli hedef listesi: Admin herkesi, Evaluator sadece kendi ekibini,
-- Employee sadece kendi hedeflerini gorur.
CREATE OR ALTER PROCEDURE usp_Hedefler_GetAll
    @Rol NVARCHAR(50),
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Admin'
    BEGIN
        SELECT h.Id AS id, h.CalisanId AS calisanId, h.Aciklama AS aciklama, h.BitisTarihi AS bitisTarihi, h.TamamlandiMi AS tamamlandiMi,
               k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman
        FROM Hedefler h
        INNER JOIN Kullanicilar k ON h.CalisanId = k.Id
        ORDER BY h.TamamlandiMi ASC, h.BitisTarihi ASC;
    END
    ELSE IF @Rol = 'Evaluator'
    BEGIN
        SELECT h.Id AS id, h.CalisanId AS calisanId, h.Aciklama AS aciklama, h.BitisTarihi AS bitisTarihi, h.TamamlandiMi AS tamamlandiMi,
               k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman
        FROM Hedefler h
        INNER JOIN Kullanicilar k ON h.CalisanId = k.Id
        WHERE k.EvaluatorId = @KullaniciId
        ORDER BY h.TamamlandiMi ASC, h.BitisTarihi ASC;
    END
    ELSE
    BEGIN
        SELECT h.Id AS id, h.CalisanId AS calisanId, h.Aciklama AS aciklama, h.BitisTarihi AS bitisTarihi, h.TamamlandiMi AS tamamlandiMi,
               k.Ad AS ad, k.Soyad AS soyad, k.Departman AS departman
        FROM Hedefler h
        INNER JOIN Kullanicilar k ON h.CalisanId = k.Id
        WHERE h.CalisanId = @KullaniciId
        ORDER BY h.TamamlandiMi ASC, h.BitisTarihi ASC;
    END
END
