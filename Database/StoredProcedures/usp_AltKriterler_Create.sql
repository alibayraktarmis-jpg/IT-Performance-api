CREATE OR ALTER PROCEDURE usp_AltKriterler_Create
    @AnaBaslikId INT,
    @KriterAdi NVARCHAR(200),
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AltKriterler (AnaBaslikId, KriterAdi, AktifMi)
    OUTPUT INSERTED.Id
    VALUES (@AnaBaslikId, @KriterAdi, @AktifMi);
END
