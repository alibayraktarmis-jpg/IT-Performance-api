CREATE OR ALTER PROCEDURE usp_Hedefler_GetCalisanId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CalisanId FROM Hedefler WHERE Id = @Id;
END
