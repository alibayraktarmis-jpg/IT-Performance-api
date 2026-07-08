-- Agirlik yuzdesi 0-100 araligi disinda olamaz; aktif ana kriterlerin toplami %100'u gecemez.
CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Create
    @Baslik NVARCHAR(200),
    @AgirlikYuzdesi INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @AgirlikYuzdesi < 0 OR @AgirlikYuzdesi > 100
    BEGIN
        RAISERROR('Agirlik yuzdesi 0 ile 100 arasinda olmalidir.', 16, 1);
        RETURN;
    END

    IF @AktifMi = 1
    BEGIN
        DECLARE @DigerAktifToplam INT;
        SELECT @DigerAktifToplam = ISNULL(SUM(AgirlikYuzdesi), 0) FROM AnaBasliklar WHERE AktifMi = 1;

        IF @DigerAktifToplam + @AgirlikYuzdesi > 100
        BEGIN
            RAISERROR('Aktif ana kriterlerin toplam agirligi yuzde 100''u gecemez.', 16, 1);
            RETURN;
        END
    END

    INSERT INTO AnaBasliklar (Baslik, AgirlikYuzdesi, AktifMi)
    VALUES (@Baslik, @AgirlikYuzdesi, @AktifMi);
END
