CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_AktifKategoriler
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Baslik FROM AnaBasliklar WHERE AktifMi = 1 ORDER BY AgirlikYuzdesi DESC;
END
