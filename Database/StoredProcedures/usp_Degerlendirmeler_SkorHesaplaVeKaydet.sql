-- Skoru hesaplar ve Degerlendirmeler.ToplamSkor kolonuna yazar.
CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_SkorHesaplaVeKaydet
    @DegerlendirmeId INT,
    @Skor FLOAT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC usp_Degerlendirmeler_SkorHesapla @DegerlendirmeId, @Skor OUTPUT;

    UPDATE Degerlendirmeler SET ToplamSkor = @Skor WHERE Id = @DegerlendirmeId;
END
