-- Email adresi benzersiz olmali (uygulama seviyesindeki kontrole ek olarak DB seviyesinde de garanti).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Kullanicilar_Email')
BEGIN
    CREATE UNIQUE INDEX UQ_Kullanicilar_Email ON Kullanicilar(Email);
END
