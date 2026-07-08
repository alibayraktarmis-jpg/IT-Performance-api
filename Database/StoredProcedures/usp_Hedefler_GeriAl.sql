CREATE OR ALTER PROCEDURE usp_Hedefler_GeriAl
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET TamamlandiMi = 0 WHERE Id = @Id;
END
