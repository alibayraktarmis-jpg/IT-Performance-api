IF NOT EXISTS (SELECT 1 FROM dbo.Kullanicilar)
BEGIN
    INSERT INTO dbo.Kullanicilar (Ad, Soyad, Email, Rol, Departman, Sifre, AktifMi, EvaluatorId)
    VALUES (N'Sistem', N'Yöneticisi', 'admin@itperformans.com', 'Admin', N'Yönetim',
            '$2a$11$3o5OuULFNuzHXmb//T32zeVBS9uOxXIEsPtNu2u0k3HGmcXGkj8OC', 1, NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AnaBasliklar)
BEGIN
    SET IDENTITY_INSERT dbo.AnaBasliklar ON;
    INSERT INTO dbo.AnaBasliklar (Id, Baslik, AgirlikYuzdesi, AktifMi) VALUES
        (1, N'Teknik Yetkinlik', 40, 1),
        (2, N'Takım Çalışması', 30, 1),
        (3, N'Problem Çözme', 30, 1),
        (4, N'Liderlik & Mentorluk', 10, 0);
    SET IDENTITY_INSERT dbo.AnaBasliklar OFF;

    SET IDENTITY_INSERT dbo.AltKriterler ON;
    INSERT INTO dbo.AltKriterler (Id, AnaBaslikId, KriterAdi, AktifMi) VALUES
        (1, 1, N'Kod Kalitesi', 1),
        (2, 1, N'Test Yazımı', 1),
        (3, 2, N'İletişim', 1),
        (4, 2, N'Toplantı Katılımı', 1),
        (5, 3, N'Bug Fix Hızı', 1),
        (6, 3, N'Analiz Yeteneği', 1),
        (7, 4, N'Bilgi Paylaşımı & Rehberlik', 0);
    SET IDENTITY_INSERT dbo.AltKriterler OFF;

    INSERT INTO dbo.KriterAciklamalar (AltKriterId, Rol, Aciklama) VALUES
        (1, N'Analist', N'Hazırlanan analiz dokümanlarının net, tutarlı ve takip edilebilir olması'),
        (1, N'Yazılımcı', N'Yazılan kodun okunabilir, test edilebilir ve standartlara uygun olması'),
        (1, N'QA', N'Test senaryolarının kapsamlı, anlaşılır ve tekrarlanabilir şekilde yazılması'),
        (2, N'Analist', N'Kabul kriterlerini ve kullanıcı senaryolarını kapsayan test vakalarının oluşturulması'),
        (2, N'Yazılımcı', N'Unit ve entegrasyon testlerinin yazılması ve code coverage oranının yüksek tutulması'),
        (2, N'QA', N'Regresyon, performans ve kenar durum testlerinin eksiksiz hazırlanması'),
        (3, N'Analist', N'Paydaşlarla, geliştiricilerle ve yönetimle açık ve etkili iletişim kurabilme'),
        (3, N'Yazılımcı', N'Teknik konuları teknik olmayan paydaşlara açık ve anlaşılır aktarabilme'),
        (3, N'QA', N'Hata raporlarını net ve çözüm odaklı şekilde ilgili ekiplerle paylaşabilme'),
        (4, N'Analist', N'Gereksinimlerin belirlenmesi ve sprint planlama toplantılarına aktif katılım'),
        (4, N'Yazılımcı', N'Daily standup, sprint review ve retrospektif toplantılarına düzenli ve aktif katılım'),
        (4, N'QA', N'Test planlaması ve UAT toplantılarına katkı sağlayarak süreçlere aktif dahil olma'),
        (5, N'Analist', N'Gereksinim kaynaklı sorunları tespit ederek çözüm önerilerini hızlı sunabilme'),
        (5, N'Yazılımcı', N'Bildirilen hataları önceliklendirerek kısa sürede analiz edip düzeltebilme'),
        (5, N'QA', N'Kritik hataları erken aşamada tespit ederek geliştirme ekibine hızlı iletebilme'),
        (6, N'Analist', N'Karmaşık iş gereksinimlerini parçalara ayırarak çözüm alternatifi üretebilme'),
        (6, N'Yazılımcı', N'Teknik sorunların kök nedenini analiz ederek kalıcı çözüm geliştirebilme'),
        (6, N'QA', N'Test sonuçlarını analiz ederek sistemdeki örüntüleri ve riskleri raporlayabilme'),
        (7, N'Analist', N'Ekip içi bilgi paylaşımı ve dokümantasyon kültürünü destekleme'),
        (7, N'Yazılımcı', N'Code review kalitesi ve teknik bilginin ekiple paylaşılması'),
        (7, N'QA', N'Test süreçlerini ekibe aktarma ve kalite standartlarını yayma');
END
GO
