CREATE OR ALTER PROCEDURE usp_DegerlendirmeDetaylar_Create
    @DegerlendirmeId INT,
    @AltKriterId INT,
    @Puan INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DegerlendirmeDetaylar (DegerlendirmeId, AltKriterId, Puan)
    VALUES (@DegerlendirmeId, @AltKriterId, @Puan);
END
