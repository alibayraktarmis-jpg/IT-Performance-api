-- CalisanId gercekten Employee rolunde bir kullanici olmali (Evaluator/Admin'e hedef atanamaz).
CREATE OR ALTER PROCEDURE usp_Hedefler_Create
    @CalisanId INT,
    @Aciklama NVARCHAR(1000),
    @BitisTarihi DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE Id = @CalisanId AND Rol = 'Employee')
    BEGIN
        RAISERROR('Hedef sadece Employee rolundeki kullanicilara atanabilir.', 16, 1);
        RETURN;
    END

    INSERT INTO Hedefler (CalisanId, Aciklama, BitisTarihi, TamamlandiMi)
    OUTPUT INSERTED.Id
    VALUES (@CalisanId, @Aciklama, @BitisTarihi, 0);
END
