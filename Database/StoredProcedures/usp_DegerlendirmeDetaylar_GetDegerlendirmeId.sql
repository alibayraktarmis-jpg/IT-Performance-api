CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_GetDegerlendirmeId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DegerlendirmeId FROM DegerlendirmeDetaylar WHERE Id = @Id;
END
