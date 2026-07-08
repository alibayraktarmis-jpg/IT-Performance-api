CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KriterAciklamalar WHERE Id = @Id;
END
