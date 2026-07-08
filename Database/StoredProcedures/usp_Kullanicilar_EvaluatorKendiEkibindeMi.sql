-- Bir calisanin, belirtilen degerlendiricinin ekibinde olup olmadigini kontrol eder.
CREATE OR ALTER PROCEDURE usp_Kullanicilar_EvaluatorKendiEkibindeMi
    @CalisanId INT,
    @EvaluatorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id FROM Kullanicilar WHERE Id = @CalisanId AND EvaluatorId = @EvaluatorId;
END
