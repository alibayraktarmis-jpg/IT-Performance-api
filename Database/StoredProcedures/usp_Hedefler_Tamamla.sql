CREATE OR ALTER PROCEDURE usp_Hedefler_Tamamla
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET TamamlandiMi = 1 WHERE Id = @Id;
END
