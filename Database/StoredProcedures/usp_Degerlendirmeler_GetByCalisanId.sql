CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetByCalisanId
    @CalisanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Degerlendirmeler WHERE CalisanId = @CalisanId ORDER BY Id ASC;
END
