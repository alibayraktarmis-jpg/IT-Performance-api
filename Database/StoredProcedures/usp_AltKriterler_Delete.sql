CREATE OR ALTER PROCEDURE usp_AltKriterler_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM DegerlendirmeDetaylar WHERE AltKriterId = @Id)
    BEGIN
        RAISERROR('Bu alt kriter gecmis degerlendirmelerde kullanilmis, silinemez. Bunun yerine pasife alabilirsiniz.', 16, 1);
        RETURN;
    END

    DELETE FROM AltKriterler WHERE Id = @Id;
END
