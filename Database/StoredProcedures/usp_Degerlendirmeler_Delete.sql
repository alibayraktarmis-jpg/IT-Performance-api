CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Degerlendirmeler WHERE Id = @Id;
END
