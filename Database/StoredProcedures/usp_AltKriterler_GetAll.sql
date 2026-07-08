CREATE OR ALTER PROCEDURE usp_AltKriterler_GetAll
    @SadeceAktif BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @SadeceAktif = 1
        SELECT * FROM AltKriterler WHERE AktifMi = 1;
    ELSE
        SELECT * FROM AltKriterler;
END
