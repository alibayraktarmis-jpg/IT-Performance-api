-- Agirlik dogrulamasinin ardindan gunceller. Ana kriter pasife alinirsa
-- altindaki tum alt kriterler de pasife alinir; aktiflestirmede ise
-- alt kriterlerin (bilerek pasif birakilmis olabilecek) durumuna dokunulmaz.
CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Update
    @Id INT,
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
        SELECT @DigerAktifToplam = ISNULL(SUM(AgirlikYuzdesi), 0)
        FROM AnaBasliklar WHERE AktifMi = 1 AND Id <> @Id;

        IF @DigerAktifToplam + @AgirlikYuzdesi > 100
        BEGIN
            RAISERROR('Aktif ana kriterlerin toplam agirligi yuzde 100''u gecemez.', 16, 1);
            RETURN;
        END
    END

    UPDATE AnaBasliklar
    SET Baslik = @Baslik, AgirlikYuzdesi = @AgirlikYuzdesi, AktifMi = @AktifMi
    WHERE Id = @Id;

    IF @AktifMi = 0
    BEGIN
        UPDATE AltKriterler SET AktifMi = 0 WHERE AnaBaslikId = @Id;
    END
END
