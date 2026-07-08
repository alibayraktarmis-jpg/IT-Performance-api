CREATE OR ALTER PROCEDURE usp_AnaBasliklar_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM AltKriterler WHERE AnaBaslikId = @Id)
    BEGIN
        RAISERROR('Bu ana kriterin altinda hala alt kriterler var. Once onlari silin veya baska bir ana kritere tasiyin.', 16, 1);
        RETURN;
    END

    DELETE FROM AnaBasliklar WHERE Id = @Id;
END
