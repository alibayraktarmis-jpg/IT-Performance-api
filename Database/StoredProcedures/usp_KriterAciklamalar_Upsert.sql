CREATE OR ALTER PROCEDURE usp_KriterAciklamalar_Upsert
    @AltKriterId INT,
    @Rol NVARCHAR(50),
    @Aciklama NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM KriterAciklamalar WHERE AltKriterId = @AltKriterId AND Rol = @Rol)
    BEGIN
        UPDATE KriterAciklamalar SET Aciklama = @Aciklama
        WHERE AltKriterId = @AltKriterId AND Rol = @Rol;
    END
    ELSE IF LTRIM(RTRIM(ISNULL(@Aciklama, ''))) <> ''
    BEGIN
        INSERT INTO KriterAciklamalar (AltKriterId, Rol, Aciklama)
        VALUES (@AltKriterId, @Rol, @Aciklama);
    END
END
