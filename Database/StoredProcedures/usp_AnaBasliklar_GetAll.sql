CREATE OR ALTER PROCEDURE usp_AnaBasliklar_GetAll
    @SadeceAktif BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @SadeceAktif = 1
        SELECT * FROM AnaBasliklar WHERE AktifMi = 1;
    ELSE
        SELECT * FROM AnaBasliklar;
END
