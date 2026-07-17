IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Hedefler__Calisa__2CF2ADDF')
BEGIN
    ALTER TABLE Hedefler DROP CONSTRAINT [FK__Hedefler__Calisa__2CF2ADDF];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Hedefler_Kullanicilar_CalisanId')
BEGIN
    ALTER TABLE Hedefler ADD CONSTRAINT FK_Hedefler_Kullanicilar_CalisanId
        FOREIGN KEY (CalisanId) REFERENCES Kullanicilar(Id) ON DELETE CASCADE;
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Degerlend__Calis__2645B050')
BEGIN
    ALTER TABLE Degerlendirmeler DROP CONSTRAINT [FK__Degerlend__Calis__2645B050];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Degerlendirmeler_Kullanicilar_CalisanId')
BEGIN
    ALTER TABLE Degerlendirmeler ADD CONSTRAINT FK_Degerlendirmeler_Kullanicilar_CalisanId
        FOREIGN KEY (CalisanId) REFERENCES Kullanicilar(Id) ON DELETE CASCADE;
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Degerlend__Deger__29221CFB')
BEGIN
    ALTER TABLE DegerlendirmeDetaylar DROP CONSTRAINT [FK__Degerlend__Deger__29221CFB];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DegerlendirmeDetaylar_Degerlendirmeler_DegerlendirmeId')
BEGIN
    ALTER TABLE DegerlendirmeDetaylar ADD CONSTRAINT FK_DegerlendirmeDetaylar_Degerlendirmeler_DegerlendirmeId
        FOREIGN KEY (DegerlendirmeId) REFERENCES Degerlendirmeler(Id) ON DELETE CASCADE;
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Degerlend__Deger__25518C17')
BEGIN
    ALTER TABLE Degerlendirmeler DROP CONSTRAINT [FK__Degerlend__Deger__25518C17];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Degerlendirmeler_Kullanicilar_DegerlendiricId')
BEGIN
    ALTER TABLE Degerlendirmeler ADD CONSTRAINT FK_Degerlendirmeler_Kullanicilar_DegerlendiricId
        FOREIGN KEY (DegerlendiricId) REFERENCES Kullanicilar(Id) ON DELETE NO ACTION;
END
