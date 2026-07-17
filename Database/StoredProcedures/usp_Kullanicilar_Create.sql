CREATE OR ALTER PROCEDURE usp_Kullanicilar_Create
    @Ad NVARCHAR(100),
    @Soyad NVARCHAR(100),
    @Email NVARCHAR(200),
    @Sifre NVARCHAR(300),
    @Rol NVARCHAR(50),
    @Departman NVARCHAR(100),
    @EvaluatorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Kullanicilar WHERE Email = @Email)
    BEGIN
        RAISERROR('Bu email adresi zaten kullaniliyor.', 16, 1);
        RETURN;
    END

    IF @Rol = 'Employee' AND @EvaluatorId IS NOT NULL
    BEGIN
        DECLARE @EvaluatorDepartman NVARCHAR(100);
        DECLARE @EvaluatorAktifMi BIT;

        SELECT @EvaluatorDepartman = Departman, @EvaluatorAktifMi = AktifMi
        FROM Kullanicilar
        WHERE Id = @EvaluatorId AND Rol = 'Evaluator';

        IF @EvaluatorDepartman IS NULL
        BEGIN
            RAISERROR('Secilen degerlendirici bulunamadi veya Evaluator rolunde degil.', 16, 1);
            RETURN;
        END

        IF @EvaluatorAktifMi = 0
        BEGIN
            RAISERROR('Secilen degerlendirici pasif durumda, calisan atanamaz.', 16, 1);
            RETURN;
        END

        IF @EvaluatorDepartman <> @Departman
        BEGIN
            RAISERROR('Degerlendiricinin departmani (%s) calisanin departmaniyla (%s) uyusmuyor.', 16, 1, @EvaluatorDepartman, @Departman);
            RETURN;
        END
    END

    INSERT INTO Kullanicilar (Ad, Soyad, Email, Sifre, Rol, Departman, EvaluatorId)
    VALUES (@Ad, @Soyad, @Email, @Sifre, @Rol, @Departman, @EvaluatorId);
END
