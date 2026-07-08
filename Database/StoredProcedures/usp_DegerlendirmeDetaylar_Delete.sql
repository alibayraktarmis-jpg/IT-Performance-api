CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DegerlendirmeDetaylar WHERE Id = @Id;
END
