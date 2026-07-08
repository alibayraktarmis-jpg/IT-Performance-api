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
    UPDATE Degerlendirmeler
    SET DegerlendiricId = @DegerlendiricId, CalisanId = @CalisanId, Tarih = @Tarih,
        Donem = @Donem, Yorum = @Yorum, ToplamSkor = @ToplamSkor
    WHERE Id = @Id;
END
