CREATE OR ALTER PROCEDURE usp_Hedefler_Create
    @CalisanId INT,
    @Aciklama NVARCHAR(1000),
    @BitisTarihi DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Hedefler (CalisanId, Aciklama, BitisTarihi, TamamlandiMi)
    OUTPUT INSERTED.Id
    VALUES (@CalisanId, @Aciklama, @BitisTarihi, 0);
END
