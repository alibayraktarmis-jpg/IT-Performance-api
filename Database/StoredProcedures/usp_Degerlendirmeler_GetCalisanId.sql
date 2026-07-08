CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetCalisanId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CalisanId FROM Degerlendirmeler WHERE Id = @Id;
END
