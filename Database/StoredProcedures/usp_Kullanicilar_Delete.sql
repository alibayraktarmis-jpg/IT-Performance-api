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
