-- Bir Employee silindiginde ona ait Hedefler ve Degerlendirmeler (ve degerlendirmelerin
-- DegerlendirmeDetaylar satirlari) otomatik silinsin diye CASCADE'e cevrilir.
-- Degerlendirmeler.DegerlendiricId NO_ACTION olarak kalir (SQL Server ayni tabloya
-- Kullanicilar -> Degerlendirmeler icin iki cascade yolu tanimlamaya izin vermiyor).
-- Bir Evaluator silindiginde onun yaptigi degerlendirmelerin DegerlendiricId'si
-- usp_Kullanicilar_Delete icinde elle NULL'lanir, boylece degerlendirme (puan/yorum/
-- donem/calisan) korunur, sadece "kim degerlendirdi" bilgisi bosalir.

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
