CREATE OR ALTER PROCEDURE usp_AltKriterler_GetByAnaBaslik
    @AnaBaslikId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM AltKriterler WHERE AnaBaslikId = @AnaBaslikId;
END
