CREATE OR ALTER PROCEDURE usp_Hedefler_Update
    @Id INT,
    @Aciklama NVARCHAR(1000),
    @BitisTarihi DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Hedefler SET Aciklama = @Aciklama, BitisTarihi = @BitisTarihi WHERE Id = @Id;
END
