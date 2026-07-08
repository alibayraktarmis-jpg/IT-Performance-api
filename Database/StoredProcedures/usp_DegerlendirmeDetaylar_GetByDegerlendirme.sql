CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_GetByDegerlendirme
    @DegerlendirmeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId;
END
