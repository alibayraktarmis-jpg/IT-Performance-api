CREATE OR ALTER PROCEDURE usp_Degerlendirmeler_GetAll
    @Rol NVARCHAR(50),
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Admin'
        SELECT * FROM Degerlendirmeler;
    ELSE IF @Rol = 'Evaluator'
        SELECT * FROM Degerlendirmeler WHERE DegerlendiricId = @KullaniciId;
    ELSE
        SELECT * FROM Degerlendirmeler WHERE CalisanId = @KullaniciId;
END
