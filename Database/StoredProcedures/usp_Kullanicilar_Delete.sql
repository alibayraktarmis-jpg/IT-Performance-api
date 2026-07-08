-- Kullaniciyi siler. Bir Evaluator'a hala bagli (EvaluatorId'si eslesen) calisan
-- varsa silme islemini engeller.
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

    DELETE FROM Kullanicilar WHERE Id = @Id;
END
