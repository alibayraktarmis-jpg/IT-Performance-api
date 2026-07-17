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
