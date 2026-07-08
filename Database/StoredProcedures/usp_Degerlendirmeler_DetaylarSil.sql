CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_DetaylarSil
    @DegerlendirmeId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DegerlendirmeDetaylar WHERE DegerlendirmeId = @DegerlendirmeId;
END
