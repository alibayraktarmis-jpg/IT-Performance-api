-- Rol bazli kullanici listesi: Evaluator sadece kendi ekibini, Employee sadece kendini,
-- Admin ise herkesi gorur.
CREATE OR ALTER PROCEDURE usp_Kullanicilar_GetAll
    @Rol NVARCHAR(50),
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Rol = 'Evaluator'
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi
        FROM Kullanicilar
        WHERE Rol = 'Employee' AND EvaluatorId = @KullaniciId;
    END
    ELSE IF @Rol = 'Employee'
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi
        FROM Kullanicilar
        WHERE Id = @KullaniciId;
    END
    ELSE
    BEGIN
        SELECT Id, Ad, Soyad, Email, Rol, Departman, AktifMi, EvaluatorId
        FROM Kullanicilar;
    END
END
