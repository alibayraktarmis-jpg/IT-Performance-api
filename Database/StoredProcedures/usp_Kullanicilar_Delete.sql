-- Kullaniciyi siler.
-- Bir Evaluator'a hala bagli (EvaluatorId'si eslesen) calisan varsa silme islemini engeller.
-- Bu kullanicinin gecmiste yaptigi degerlendirmeler (Degerlendirmeler.DegerlendiricId) silinmez;
-- SQL Server ayni tabloya (Kullanicilar -> Degerlendirmeler) iki cascade yolu (CalisanId=CASCADE,
-- DegerlendiricId=SET NULL) tanimlamaya izin vermedigi icin bu NULL'lama burada elle yapilir.
-- Boylece degerlendirilen calisanin puani/yorumu/donemi korunur, sadece "kim degerlendirdi" bilgisi bosalir.
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
