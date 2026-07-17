IF OBJECT_ID(N'dbo.Kullanicilar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Kullanicilar (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kullanicilar PRIMARY KEY,
        Ad NVARCHAR(50) NULL,
        Soyad NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        Rol NVARCHAR(50) NULL,
        Departman NVARCHAR(50) NULL,
        Sifre NVARCHAR(255) NULL,
        AktifMi BIT NOT NULL CONSTRAINT DF_Kullanicilar_AktifMi DEFAULT (1),
        EvaluatorId INT NULL,
        KayitTarihi DATETIME NOT NULL CONSTRAINT DF_Kullanicilar_KayitTarihi DEFAULT (GETDATE()),
        SonGirisTarihi DATETIME NULL,
        SonAktiflikZamani DATETIME NULL
    );

    CREATE UNIQUE INDEX UQ_Kullanicilar_Email ON dbo.Kullanicilar (Email);
END
GO

IF OBJECT_ID(N'dbo.AnaBasliklar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnaBasliklar (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AnaBasliklar PRIMARY KEY,
        Baslik NVARCHAR(100) NOT NULL,
        AgirlikYuzdesi INT NOT NULL,
        AktifMi BIT NULL CONSTRAINT DF_AnaBasliklar_AktifMi DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.AltKriterler', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AltKriterler (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AltKriterler PRIMARY KEY,
        AnaBaslikId INT NULL CONSTRAINT FK_AltKriterler_AnaBasliklar REFERENCES dbo.AnaBasliklar (Id),
        KriterAdi NVARCHAR(100) NOT NULL,
        AktifMi BIT NULL CONSTRAINT DF_AltKriterler_AktifMi DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.KriterAciklamalar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KriterAciklamalar (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KriterAciklamalar PRIMARY KEY,
        AltKriterId INT NULL CONSTRAINT FK_KriterAciklamalar_AltKriterler REFERENCES dbo.AltKriterler (Id) ON DELETE CASCADE,
        Rol NVARCHAR(50) NOT NULL,
        Aciklama NVARCHAR(500) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Degerlendirmeler', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Degerlendirmeler (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Degerlendirmeler PRIMARY KEY,
        DegerlendiricId INT NULL CONSTRAINT FK_Degerlendirmeler_Kullanicilar_DegerlendiricId REFERENCES dbo.Kullanicilar (Id),
        CalisanId INT NULL CONSTRAINT FK_Degerlendirmeler_Kullanicilar_CalisanId REFERENCES dbo.Kullanicilar (Id) ON DELETE CASCADE,
        Tarih DATE NOT NULL,
        Donem NVARCHAR(50) NULL,
        Yorum NVARCHAR(MAX) NULL,
        ToplamSkor FLOAT NULL
    );

    CREATE UNIQUE INDEX UQ_Degerlendirmeler_CalisanDonem ON dbo.Degerlendirmeler (CalisanId, Donem);
END
GO

IF OBJECT_ID(N'dbo.DegerlendirmeDetaylar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DegerlendirmeDetaylar (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DegerlendirmeDetaylar PRIMARY KEY,
        DegerlendirmeId INT NULL CONSTRAINT FK_DegerlendirmeDetaylar_Degerlendirmeler REFERENCES dbo.Degerlendirmeler (Id) ON DELETE CASCADE,
        AltKriterId INT NULL CONSTRAINT FK_DegerlendirmeDetaylar_AltKriterler REFERENCES dbo.AltKriterler (Id),
        Puan INT NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Hedefler', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hedefler (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Hedefler PRIMARY KEY,
        CalisanId INT NULL CONSTRAINT FK_Hedefler_Kullanicilar_CalisanId REFERENCES dbo.Kullanicilar (Id) ON DELETE CASCADE,
        Aciklama NVARCHAR(500) NULL,
        BitisTarihi DATE NULL,
        TamamlandiMi BIT NULL CONSTRAINT DF_Hedefler_TamamlandiMi DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.SifreSifirlamaTokenlari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SifreSifirlamaTokenlari (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SifreSifirlamaTokenlari PRIMARY KEY,
        KullaniciId INT NOT NULL CONSTRAINT FK_SifreSifirlamaTokenlari_Kullanicilar REFERENCES dbo.Kullanicilar (Id) ON DELETE CASCADE,
        Token NVARCHAR(100) NOT NULL,
        OlusturmaZamani DATETIME NOT NULL CONSTRAINT DF_SifreSifirlamaTokenlari_OlusturmaZamani DEFAULT (GETDATE()),
        SonKullanmaTarihi DATETIME NOT NULL,
        KullanildiMi BIT NOT NULL CONSTRAINT DF_SifreSifirlamaTokenlari_KullanildiMi DEFAULT (0)
    );

    CREATE UNIQUE INDEX UQ_SifreSifirlamaTokenlari_Token ON dbo.SifreSifirlamaTokenlari (Token);
END
GO
