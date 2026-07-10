-- Kullanicinin "su an aktif mi" (canli/cevrimici) gostergesi icin son istek zamanini tutar.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Kullanicilar') AND name = 'SonAktiflikZamani')
BEGIN
    ALTER TABLE Kullanicilar ADD SonAktiflikZamani DATETIME NULL;
END
