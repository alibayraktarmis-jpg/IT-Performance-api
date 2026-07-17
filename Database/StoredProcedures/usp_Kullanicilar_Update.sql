CREATE OR ALTER PROCEDURE usp_Kullanicilar_Update
    @Id INT,
    @Ad NVARCHAR(100),
    @Soyad NVARCHAR(100),
    @Email NVARCHAR(200),
    @Rol NVARCHAR(50),
    @Departman NVARCHAR(100),
    @EvaluatorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Kullanicilar WHERE Email = @Email AND Id <> @Id)
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

    UPDATE Kullanicilar
    SET Ad = @Ad, Soyad = @Soyad, Email = @Email, Rol = @Rol, Departman = @Departman, EvaluatorId = @EvaluatorId
    WHERE Id = @Id;
END
