-- Gecmis degerlendirmelerde kullanilmis veya rol aciklamasi olan bir alt kriter
-- silinemez, pasife alinmalidir.
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

    IF EXISTS (SELECT 1 FROM KriterAciklamalar WHERE AltKriterId = @Id)
    BEGIN
        RAISERROR('Bu alt kriterin rol aciklamalari var. Once onlari silin.', 16, 1);
        RETURN;
    END

    DELETE FROM AltKriterler WHERE Id = @Id;
END
