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
