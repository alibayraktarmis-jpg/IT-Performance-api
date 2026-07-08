CREATE OR ALTER PROCEDURE usp_AltKriterler_Update
    @Id INT,
    @AnaBaslikId INT,
    @KriterAdi NVARCHAR(200),
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AltKriterler
    SET AnaBaslikId = @AnaBaslikId, KriterAdi = @KriterAdi, AktifMi = @AktifMi
    WHERE Id = @Id;
END
