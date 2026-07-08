CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM KriterAciklamalar WHERE Id = @Id;
END
