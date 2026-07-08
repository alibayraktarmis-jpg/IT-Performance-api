CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Degerlendirmeler WHERE Id = @Id;
END
