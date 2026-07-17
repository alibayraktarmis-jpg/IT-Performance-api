CREATE OR ALTER PROCEDURE usp_AltKriterler_Create
    @AnaBaslikId INT,
    @KriterAdi NVARCHAR(200),
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AltKriterler (AnaBaslikId, KriterAdi, AktifMi)
    OUTPUT INSERTED.Id
    VALUES (@AnaBaslikId, @KriterAdi, @AktifMi);
END
GO

CREATE OR ALTER PROCEDURE usp_AltKriterler_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM DegerlendirmeDetaylar WHERE AltKriterId = @Id)
    BEGIN
        RAISERROR('Bu alt kriter gecmis degerlendirmelerde kullanilmis, silinemez. Bunun yerine pasife alabilirsiniz.', 16, 1);
        RETURN;
    END

    DELETE FROM AltKriterler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_AltKriterler_GetAll
    @SadeceAktif BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @SadeceAktif = 1
        SELECT * FROM AltKriterler WHERE AktifMi = 1;
    ELSE
        SELECT * FROM AltKriterler;
END
GO

CREATE OR ALTER PROCEDURE usp_AltKriterler_GetByAnaBaslik
    @AnaBaslikId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AltKriterler WHERE AnaBaslikId = @AnaBaslikId;
END
GO

CREATE OR ALTER PROCEDURE usp_AltKriterler_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AltKriterler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_AltKriterler_Update
    @Id INT,
    @AnaBaslikId INT,
    @KriterAdi NVARCHAR(200),
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AltKriterler
    SET AnaBaslikId = @AnaBaslikId, KriterAdi = @KriterAdi, AktifMi = @AktifMi
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Create
    @Baslik NVARCHAR(200),
    @AgirlikYuzdesi INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @AgirlikYuzdesi < 0 OR @AgirlikYuzdesi > 100
    BEGIN
        RAISERROR('Agirlik yuzdesi 0 ile 100 arasinda olmalidir.', 16, 1);
        RETURN;
    END

    IF @AktifMi = 1
    BEGIN
        DECLARE @DigerAktifToplam INT;
        SELECT @DigerAktifToplam = ISNULL(SUM(AgirlikYuzdesi), 0) FROM AnaBasliklar WHERE AktifMi = 1;

        IF @DigerAktifToplam + @AgirlikYuzdesi > 100
        BEGIN
            RAISERROR('Aktif ana kriterlerin toplam agirligi yuzde 100''u gecemez.', 16, 1);
            RETURN;
        END
    END

    INSERT INTO AnaBasliklar (Baslik, AgirlikYuzdesi, AktifMi)
    VALUES (@Baslik, @AgirlikYuzdesi, @AktifMi);
END
GO

CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM AltKriterler WHERE AnaBaslikId = @Id)
    BEGIN
        RAISERROR('Bu ana kriterin altinda hala alt kriterler var. Once onlari silin veya baska bir ana kritere tasiyin.', 16, 1);
        RETURN;
    END

    DELETE FROM AnaBasliklar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_AnaBasliklar_GetAll
    @SadeceAktif BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @SadeceAktif = 1
        SELECT * FROM AnaBasliklar WHERE AktifMi = 1;
    ELSE
        SELECT * FROM AnaBasliklar;
END
GO

CREATE OR ALTER PROCEDURE usp_AnaBasliklar_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AnaBasliklar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Update
    @Id INT,
    @Baslik NVARCHAR(200),
    @AgirlikYuzdesi INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @AgirlikYuzdesi < 0 OR @AgirlikYuzdesi > 100
    BEGIN
        RAISERROR('Agirlik yuzdesi 0 ile 100 arasinda olmalidir.', 16, 1);
        RETURN;
    END

    IF @AktifMi = 1
    BEGIN
        DECLARE @DigerAktifToplam INT;
        SELECT @DigerAktifToplam = ISNULL(SUM(AgirlikYuzdesi), 0)
        FROM AnaBasliklar WHERE AktifMi = 1 AND Id <> @Id;

        IF @DigerAktifToplam + @AgirlikYuzdesi > 100
        BEGIN
            RAISERROR('Aktif ana kriterlerin toplam agirligi yuzde 100''u gecemez.', 16, 1);
            RETURN;
        END
    END

    UPDATE AnaBasliklar
    SET Baslik = @Baslik, AgirlikYuzdesi = @AgirlikYuzdesi, AktifMi = @AktifMi
    WHERE Id = @Id;

    IF @AktifMi = 0
    BEGIN
        UPDATE AltKriterler SET AktifMi = 0 WHERE AnaBaslikId = @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_Create
    @DegerlendirmeId INT,
    @AltKriterId INT,
    @Puan INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan)
    VALUES (@DegerlendirmeId, @AltKriterId, @Puan);
END
GO

CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DegerlendirmeDetaylar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_GetByDegerlendirme
    @DegerlendirmeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId;
END
GO

CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_GetDegerlendirmeId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DegerlendirmeId FROM DegerlendirmeDetaylar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_AktifKategoriler
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Baslik FROM AnaBasliklar WHERE AktifMi = 1 ORDER BY AgirlikYuzdesi DESC;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_Create
    @DegerlendiricId INT,
    @CalisanId INT,
    @Tarih DATETIME,
    @Donem NVARCHAR(50),
    @Yorum NVARCHAR(MAX),
    @YeniId INT OUTPUT,
    @MevcutId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE Id = @CalisanId AND Rol = 'Employee')
    BEGIN
        RAISERROR('Degerlendirme sadece Employee rolundeki kullanicilar icin olusturulabilir.', 16, 1);
        RETURN;
    END

    SELECT @MevcutId = Id FROM Degerlendirmeler WHERE CalisanId = @CalisanId AND Donem = @Donem;
    IF @MevcutId IS NOT NULL
    BEGIN
        SET @YeniId = NULL;
        RETURN;
    END

    INSERT INTO Degerlendirmeler (DegerlendiricId, CalisanId, Tarih, Donem, Yorum, ToplamSkor)
    VALUES (@DegerlendiricId, @CalisanId, @Tarih, @Donem, @Yorum, 0);

    SET @YeniId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Degerlendirmeler WHERE Id = @Id;
END
GO

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
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_DetaylarSil
    @DegerlendirmeId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetAll
    @Rol NVARCHAR(50),
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Admin'
        SELECT * FROM Degerlendirmeler;
    ELSE IF @Rol = 'Evaluator'
        SELECT * FROM Degerlendirmeler WHERE DegerlendiricId = @KullaniciId;
    ELSE
        SELECT * FROM Degerlendirmeler WHERE CalisanId = @KullaniciId;
END
GO

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
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetByCalisanId
    @CalisanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Degerlendirmeler WHERE CalisanId = @CalisanId ORDER BY Id ASC;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Degerlendirmeler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetCalisanId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CalisanId FROM Degerlendirmeler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetDonemler
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Donem FROM Degerlendirmeler
    WHERE Donem IS NOT NULL AND Donem != ''
    ORDER BY Donem DESC;
END
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_SkorHesaplaVeKaydet
    @DegerlendirmeId INT,
    @Skor FLOAT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC usp_Degerlendirmeler_SkorHesapla @DegerlendirmeId, @Skor OUTPUT;

    UPDATE Degerlendirmeler SET ToplamSkor = @Skor WHERE Id = @DegerlendirmeId;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_Update
    @Id INT,
    @DegerlendiricId INT,
    @CalisanId INT,
    @Tarih DATETIME,
    @Donem NVARCHAR(50),
    @Yorum NVARCHAR(MAX),
    @ToplamSkor FLOAT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Degerlendirmeler WHERE CalisanId = @CalisanId AND Donem = @Donem AND Id <> @Id)
    BEGIN
        RAISERROR('Bu calisan icin bu doneme ait baska bir degerlendirme zaten mevcut.', 16, 1);
        RETURN;
    END

    UPDATE Degerlendirmeler
    SET DegerlendiricId = @DegerlendiricId, CalisanId = @CalisanId, Tarih = @Tarih,
        Donem = @Donem, Yorum = @Yorum, ToplamSkor = @ToplamSkor
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_UpdateYorumSkor
    @Id INT,
    @Yorum NVARCHAR(MAX),
    @ToplamSkor FLOAT,
    @Tarih DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Degerlendirmeler
    SET Yorum = @Yorum, ToplamSkor = @ToplamSkor, Tarih = @Tarih
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_Create
    @CalisanId INT,
    @Aciklama NVARCHAR(1000),
    @BitisTarihi DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE Id = @CalisanId AND Rol = 'Employee')
    BEGIN
        RAISERROR('Hedef sadece Employee rolundeki kullanicilara atanabilir.', 16, 1);
        RETURN;
    END

    INSERT INTO Hedefler (CalisanId, Aciklama, BitisTarihi, TamamlandiMi)
    OUTPUT INSERTED.Id
    VALUES (@CalisanId, @Aciklama, @BitisTarihi, 0);
END
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Hedefler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_GeriAl
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET TamamlandiMi = 0 WHERE Id = @Id;
END
GO

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
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_GetCalisanId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CalisanId FROM Hedefler WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_Tamamla
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET TamamlandiMi = 1 WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Hedefler_Update
    @Id INT,
    @Aciklama NVARCHAR(1000),
    @BitisTarihi DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET Aciklama = @Aciklama, BitisTarihi = @BitisTarihi WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Create
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama)
    VALUES (@AltKriterId, @Rol, @Aciklama);
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM KriterAciklamalar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetByAltKriter
    @AltKriterId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetByAltKriterVeRol
    @AltKriterId INT,
    @Rol NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Update
    @Id INT,
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KriterAciklamalar
    SET AltKriterId = @AltKriterId, Rol = @Rol, Aciklama = @Aciklama
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Upsert
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol)
    BEGIN
        UPDATE KriterAciklamalar SET Aciklama = @Aciklama
        WHERE AltKriterId = @AltKriterId AND Rol = @Rol;
    END
    ELSE IF LTRIM(RTRIM(ISNULL(@Aciklama, ''))) <> ''
    BEGIN
        INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama)
        VALUES (@AltKriterId, @Rol, @Aciklama);
    END
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_AktifMiKontrol
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET SonAktiflikZamani = GETDATE() WHERE Id = @Id;
    SELECT AktifMi, Rol FROM Kullanicilar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_AktifPasifYap
    @Id INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @AktifMi = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM Kullanicilar WHERE EvaluatorId = @Id)
        BEGIN
            RAISERROR('Bu degerlendiriciye hala bagli calisanlar var. Once onlari baska bir degerlendiriciye atayin.', 16, 1);
            RETURN;
        END
    END

    UPDATE Kullanicilar SET AktifMi = @AktifMi WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_Create
    @Ad NVARCHAR(100),
    @Soyad NVARCHAR(100),
    @Email NVARCHAR(200),
    @Sifre NVARCHAR(300),
    @Rol NVARCHAR(50),
    @Departman NVARCHAR(100),
    @EvaluatorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Kullanicilar WHERE Email = @Email)
    BEGIN
        RAISERROR('Bu email adresi zaten kullaniliyor.', 16, 1);
        RETURN;
    END

    IF @Rol = 'Employee' AND @EvaluatorId IS NOT NULL
    BEGIN
        DECLARE @EvaluatorDepartman NVARCHAR(100);
        DECLARE @EvaluatorAktifMi BIT;

        SELECT @EvaluatorDepartman = Departman, @EvaluatorAktifMi = AktifMi
        FROM Kullanicilar
        WHERE Id = @EvaluatorId AND Rol = 'Evaluator';

        IF @EvaluatorDepartman IS NULL
        BEGIN
            RAISERROR('Secilen degerlendirici bulunamadi veya Evaluator rolunde degil.', 16, 1);
            RETURN;
        END

        IF @EvaluatorAktifMi = 0
        BEGIN
            RAISERROR('Secilen degerlendirici pasif durumda, calisan atanamaz.', 16, 1);
            RETURN;
        END

        IF @EvaluatorDepartman <> @Departman
        BEGIN
            RAISERROR('Degerlendiricinin departmani (%s) calisanin departmaniyla (%s) uyusmuyor.', 16, 1, @EvaluatorDepartman, @Departman);
            RETURN;
        END
    END

    INSERT INTO Kullanicilar (Ad, Soyad, Email, Sifre, Rol, Departman, EvaluatorId)
    VALUES (@Ad, @Soyad, @Email, @Sifre, @Rol, @Departman, @EvaluatorId);
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Kullanicilar WHERE EvaluatorId = @Id)
    BEGIN
        RAISERROR('Bu degerlendiriciye hala bagli calisanlar var. Once onlari baska bir degerlendiriciye atayin.', 16, 1);
        RETURN;
    END

    UPDATE Degerlendirmeler SET DegerlendiricId = NULL WHERE DegerlendiricId = @Id;

    DELETE FROM Kullanicilar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_EvaluatorKendiEkibindeMi
    @CalisanId INT,
    @EvaluatorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id FROM Kullanicilar WHERE Id = @CalisanId AND EvaluatorId = @EvaluatorId;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetAll
    @Rol NVARCHAR(50),
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Evaluator'
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi
        FROM Kullanicilar
        WHERE Rol = 'Employee' AND EvaluatorId = @KullaniciId;
    END
    ELSE IF @Rol = 'Employee'
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi
        FROM Kullanicilar
        WHERE Id = @KullaniciId;
    END
    ELSE
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi, EvaluatorId, SonGirisTarihi, SonAktiflikZamani
        FROM Kullanicilar;
    END
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetByEmail
    @Email NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Kullanicilar WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetSifreHash
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Sifre FROM Kullanicilar WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_SifreDegistir
    @Id INT,
    @YeniSifre NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET Sifre = @YeniSifre WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_SonGirisGuncelle
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Kullanicilar SET SonGirisTarihi = GETDATE() WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_Kullanicilar_Update
    @Id INT,
    @Ad NVARCHAR(100),
    @Soyad NVARCHAR(100),
    @Email NVARCHAR(200),
    @Rol NVARCHAR(50),
    @Departman NVARCHAR(100),
    @EvaluatorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Kullanicilar WHERE Email = @Email AND Id <> @Id)
    BEGIN
        RAISERROR('Bu email adresi zaten kullaniliyor.', 16, 1);
        RETURN;
    END

    IF @Rol = 'Employee' AND @EvaluatorId IS NOT NULL
    BEGIN
        DECLARE @EvaluatorDepartman NVARCHAR(100);
        DECLARE @EvaluatorAktifMi BIT;

        SELECT @EvaluatorDepartman = Departman, @EvaluatorAktifMi = AktifMi
        FROM Kullanicilar
        WHERE Id = @EvaluatorId AND Rol = 'Evaluator';

        IF @EvaluatorDepartman IS NULL
        BEGIN
            RAISERROR('Secilen degerlendirici bulunamadi veya Evaluator rolunde degil.', 16, 1);
            RETURN;
        END

        IF @EvaluatorAktifMi = 0
        BEGIN
            RAISERROR('Secilen degerlendirici pasif durumda, calisan atanamaz.', 16, 1);
            RETURN;
        END

        IF @EvaluatorDepartman <> @Departman
        BEGIN
            RAISERROR('Degerlendiricinin departmani (%s) calisanin departmaniyla (%s) uyusmuyor.', 16, 1, @EvaluatorDepartman, @Departman);
            RETURN;
        END
    END

    UPDATE Kullanicilar
    SET Ad = @Ad, Soyad = @Soyad, Email = @Email, Rol = @Rol, Departman = @Departman, EvaluatorId = @EvaluatorId
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE usp_SifreSifirlama_SifreyiSifirla
    @Token NVARCHAR(100),
    @YeniSifreHash NVARCHAR(300),
    @BasariliMi BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @BasariliMi = 0;

    BEGIN TRANSACTION;

    DECLARE @KullaniciId INT;
    SELECT @KullaniciId = KullaniciId
    FROM SifreSifirlamaTokenlari WITH (UPDLOCK, ROWLOCK)
    WHERE Token = @Token AND KullanildiMi = 0 AND SonKullanmaTarihi > GETDATE();

    IF @KullaniciId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE Kullanicilar SET Sifre = @YeniSifreHash WHERE Id = @KullaniciId;
    UPDATE SifreSifirlamaTokenlari SET KullanildiMi = 1 WHERE Token = @Token;

    COMMIT TRANSACTION;
    SET @BasariliMi = 1;
END
GO

CREATE OR ALTER PROCEDURE usp_SifreSifirlama_TokenOlustur
    @KullaniciId INT,
    @Token NVARCHAR(100),
    @GecerlilikDakika INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO SifreSifirlamaTokenlari (KullaniciId, Token, OlusturmaZamani, SonKullanmaTarihi, KullanildiMi)
    VALUES (@KullaniciId, @Token, GETDATE(), DATEADD(MINUTE, @GecerlilikDakika, GETDATE()), 0);
END
GO

