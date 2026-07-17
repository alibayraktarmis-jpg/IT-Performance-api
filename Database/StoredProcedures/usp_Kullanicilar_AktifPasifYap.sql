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
