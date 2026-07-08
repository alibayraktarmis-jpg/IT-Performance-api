-- Ayni calisan+donem icin degerlendirme zaten varsa eklemez, mevcut Id'yi
-- @MevcutId ciktisinda doner (cagiran taraf bunu 409 Conflict olarak yorumlar).
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
