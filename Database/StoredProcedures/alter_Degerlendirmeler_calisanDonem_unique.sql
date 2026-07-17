IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Degerlendirmeler_CalisanDonem')
BEGIN
    CREATE UNIQUE INDEX UQ_Degerlendirmeler_CalisanDonem ON Degerlendirmeler(CalisanId, Donem);
END
