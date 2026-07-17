# Veritabanı Kurulumu

IT Performans Değerlendirme Sistemi'nin veritabanını sıfırdan kurmak için bu klasördeki scriptler sırayla çalıştırılır.

## Gereksinimler

- MS SQL Server (Express yeterli)
- `sqlcmd` veya SSMS

## Kurulum

Aşağıdaki komutları sırasıyla çalıştırın (sunucu adını kendi ortamınıza göre değiştirin):

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID(N'ITPerformansDB') IS NULL CREATE DATABASE ITPerformansDB"
sqlcmd -S "localhost\SQLEXPRESS" -E -d ITPerformansDB -i 01_Tablolar.sql
sqlcmd -S "localhost\SQLEXPRESS" -E -d ITPerformansDB -i 02_StoredProceduresKur.sql
sqlcmd -S "localhost\SQLEXPRESS" -E -d ITPerformansDB -i 03_OrnekVeri.sql
```

SSMS kullanıyorsanız: `ITPerformansDB` adında bir veritabanı oluşturup üç scripti aynı sırayla açıp çalıştırmanız yeterli.

## Scriptler

| Script | İçerik |
|---|---|
| `01_Tablolar.sql` | 8 tablo: PK/FK'ler, cascade silme kuralları, unique kısıtlar (Email, CalisanId+Donem, Token), default değerler. Tekrar çalıştırılabilir (var olan tabloya dokunmaz). |
| `02_StoredProceduresKur.sql` | 62 stored procedure (tek dosyada, `StoredProcedures/` klasöründeki `usp_*.sql` dosyalarından üretilmiştir). `CREATE OR ALTER` kullandığı için tekrar çalıştırılabilir. |
| `03_OrnekVeri.sql` | İlk giriş için Admin kullanıcısı + örnek kriter seti (4 ana başlık, 7 alt kriter, 21 rol açıklaması). Yalnızca boş veritabanına veri ekler. |

## İlk Giriş

| Alan | Değer |
|---|---|
| Email | `admin@itperformans.com` |
| Şifre | `Admin123!` |

> İlk girişten sonra Profil sayfasından şifreyi değiştirmeniz önerilir.

## Klasör Notları

- `StoredProcedures/usp_*.sql` — her SP'nin tekil kaynak dosyası. Bir SP değiştiğinde önce burada güncelleyin, sonra `02_StoredProceduresKur.sql`'i yeniden üretin (dosyaları alfabetik sırayla `GO` ile birleştirin).
- `StoredProcedures/alter_*.sql` — geliştirme sürecinde mevcut veritabanına uygulanan değişikliklerin geçmişi. Etkileri `01_Tablolar.sql`'e işlenmiştir; **sıfırdan kurulumda çalıştırılmazlar**.
- `er-diyagrami.dbml` — ER diyagramının kaynak kodu ([dbdiagram.io](https://dbdiagram.io) formatında).

## Uygulama Bağlantısı

API'nin `appsettings.json` dosyasındaki bağlantı dizesi varsayılan olarak şudur:

```
Server=localhost\SQLEXPRESS;Database=ITPerformansDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true
```

Farklı bir sunucu/instance kullanıyorsanız yalnızca `Server=` kısmını değiştirmeniz yeterlidir.
