# Advanced Disk Analyzer v2.0

**Gelişmiş disk alanı analiz ve temizlik aracı — Windows için tek dosyalı, bağımlılıksız çözüm.**

Advanced Disk Analyzer, disk kullanımını derinlemesine analiz eden, gereksiz dosyaları skor bazlı yöntemle tespit eden ve güvenli toplu temizlik imkânı sunan bir Windows masaüstü uygulamasıdır. Tamamı tek bir C# kaynak dosyasından derlenir; harici kütüphane veya NuGet paketi gerektirmez.

---

## Özellikler

### Tarama Motorları
- **NTFS Turbo Tarama (MFT):** Ham MFT kayıtlarını doğrudan okuyarak ultra hızlı dosya envanteri çıkarır. Yönetici yetkisi gerektirir. SSD'lerde milyonlarca dosyayı saniyeler içinde tarar.
- **Hızlı WinAPI Tarama:** `FindFirstFile` / `FindNextFile` API'leri ile paralel özyinelemeli dizin taraması yapar. Yönetici yetkisi gerektirmez, standart kullanıcı izinleriyle çalışır.
- **Normal Tarama:** Tam dosya listesini çıkarır, tarama sırasında canlı güncelleme sağlar.
- **Canlı Tarama:** Tarama devam ederken dosyalar anlık olarak ListView bileşenine eklenir; kullanıcı bekleme süresi boyunca sonuçları inceleyebilir.

### Görselleştirme
- **Pasta Grafik:** Klasör bazında boyut dağılımını grafiksel olarak gösterir. Fare ile üzerine gelindiğinde detay bilgisi sunar.
- **Treemap:** Alan bazlı görsel harita oluşturur. Her blok bir dosya veya klasörü temsil eder; tıklanarak alt dizinlere inilebilir.

### Analiz ve Temizlik
- **Skor Bazlı Analiz:** Her dosyaya 0–100 arasında bir gereksizlik skoru atar. Ağırlıklar: boyut %34, yaş %24, temizlik ipucu %24, konum %18. Kritik sistem dosyaları için negatif koruma cezası uygulanır.
- **Temizlik Kuralları Motoru:** Dosyaları güvenli ve riskli olarak sınıflandırır. Otomatik temizlik önerileri sunar; bilinen geçici dosya desenleri, tarayıcı önbellek dizinleri ve günlük dosyaları için önceden tanımlı kurallar içerir.
- **Toplu Silme:** Seçili dosyaları Geri Dönüşüm Kutusu'na göndererek güvenli toplu silme işlemi yapar.
- **Kopya Dosya Tespiti:** Aynı isim ve boyuta sahip dosyaları gruplar, olası kopyaları tespit eder.

### Raporlama
- **PDF Rapor:** Harici kütüphane kullanmadan ham PDF formatında rapor üretir. Bağımlılıksız PDF oluşturucu sayesinde ek kurulum gerektirmez.
- **CSV Dışa Aktarım:** Tarama sonuçlarını CSV formatında dışa aktarır.

### Disk ve Dosya Sistemi
- **Hard Link Çift Sayım Koruması:** `FileIdentity` (dosya kimliği) bilgisini kullanarak aynı fiziksel dosyanın birden fazla hard link üzerinden tekrar sayılmasını önler.
- **Diskte Kaplanan Alan (Allocated Size):** Cluster bazlı gerçek disk alanı hesabı yapar. Dosya boyutu ile diskte kaplanan alan arasındaki farkı gösterir.
- **Sıkıştırılmış / Sparse Dosya Desteği:** `GetCompressedFileSize` API'si ile NTFS sıkıştırılmış ve sparse dosyaların gerçek disk kullanımını doğru ölçer.
- **Uzun Yol Desteği:** `\\?\` extended path formatı ile 260 karakteri aşan dosya yollarını sorunsuz işler.
- **SSD / HDD Otomatik Algılama:** WMI sorguları ile sürücü tipini tespit eder ve paralel tarama iş parçacığı sayısını sürücü tipine göre otomatik ayarlar.
- **Ağ Sürücüsü Desteği:** UNC yolları ve eşlenmiş ağ sürücülerini tarama desteği sunar (lisanslı sürümde).

### Kullanıcı Arayüzü
- **Koyu / Açık Tema:** DWM dark title bar desteği dahil, sistem temasına uyumlu iki görünüm modu.
- **Modern ScrollBar:** Özel çizimli, ince scrollbar overlay bileşeni.
- **Sağ Tık Menüsü:** Dosyayı Explorer'da açma, yolu panoya kopyalama ve silme işlemleri için bağlam menüsü.

### Tarama Geçmişi
- **CSV Kayıt:** Her tarama sonucu otomatik olarak CSV formatında kaydedilir.
- **Snapshot Karşılaştırma:** Farklı zamanlarda alınan tarama sonuçlarını karşılaştırarak disk kullanım değişimlerini takip eder.

### Lisans ve Hesap
- **Lisans Sistemi:** RSA imzalı çevrimdışı doğrulama. Free, Pro ve Enterprise olmak üzere üç farklı plan sunar.
- **Online Hesap:** HTTP tabanlı kayıt, giriş ve satın alma işlemleri.
- **EULA / Gizlilik Onayı:** İlk çalıştırmada kullanıcı onayı alır, onay durumu Registry'de saklanır.

### Kurulum
- **Kurulum Sihirbazı:** Gömülü kaynakları çıkararak uygulama dosyalarını yükler, masaüstü ve Başlat menüsü kısayolları oluşturur.

---

## Mimari

Proje üç bağımsız C# kaynak dosyasından oluşur. Harici bağımlılık bulunmaz; tüm bileşenler .NET Framework 4.0 temel kütüphaneleri ile derlenir.

| Dosya | Açıklama | Yaklaşık Satır |
|---|---|---|
| `Program.cs` | Ana uygulama — kullanıcı arayüzü, tarama motoru, görselleştirme bileşenleri, skor analizi, lisans doğrulama, raporlama | ~7.800 |
| `LicenseServer.cs` | HTTP lisans sunucusu — `HttpListener` üzerinde çalışır, XML tabanlı `DataSet` ile lisans verilerini yönetir | ~800 |
| `Setup.cs` | Kurulum sihirbazı — derlenmiş çalıştırılabilir dosyaları gömülü kaynak olarak taşır ve hedef dizine çıkarır | ~500 |

---

## Skor Sistemi

Her dosyaya 0 ile 100 arasında bir **gereksizlik skoru** atanır. Skor ne kadar yüksekse, dosyanın gereksiz olma olasılığı o kadar fazladır.

### Ağırlık Tablosu

| Kriter | Ağırlık | Açıklama |
|---|---|---|
| **Boyut** | %34 | Büyük dosyalar daha yüksek skor alır. Logaritmik ölçekleme uygulanır. |
| **Yaş** | %24 | Uzun süredir erişilmemiş dosyalar daha yüksek skor alır. |
| **Temizlik İpucu** | %24 | Geçici dosya desenleri, önbellek dizinleri, günlük dosyaları gibi bilinen temizlik hedefleriyle eşleşen dosyalar ek puan alır. |
| **Konum** | %18 | Temp, önbellek ve indirme dizinlerindeki dosyalar konum puanı alır. |
| **Koruma Cezası** | -%28 | Sistem dosyaları, program dizinleri ve bilinen kritik dosyalar negatif puan alarak skor düşürülür. Bu mekanizma yanlışlıkla önemli dosyaların temizlik listesine girmesini önler. |

---

## Tarama Modları Karşılaştırması

| Özellik | NTFS Turbo (MFT) | Hızlı WinAPI | Normal |
|---|---|---|---|
| **Hız** | Çok yüksek | Yüksek | Orta |
| **Yöntem** | Ham MFT kayıt okuma | `FindFirstFile/FindNextFile` | Standart dizin yürüme |
| **Yönetici Yetkisi** | Gerekli | Gereksiz | Gereksiz |
| **Dosya Sistemi** | Yalnızca NTFS | Tüm dosya sistemleri | Tüm dosya sistemleri |
| **Paralel Tarama** | Evet | Evet | Hayır |
| **Canlı Güncelleme** | Evet | Evet | Evet |
| **Allocated Size** | Evet | Evet | Evet |

---

## Derleme

### Otomatik Derleme

```batch
build.bat
```

### Manuel Derleme

```batch
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

:: Ana uygulama
%CSC% /target:winexe /codepage:65001 /win32icon:app.ico /out:DiskAnalyzer.exe ^
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ^
  /reference:System.Management.dll Program.cs

:: Lisans sunucusu
%CSC% /target:exe /codepage:65001 /out:LicenseServer.exe ^
  /reference:System.Data.dll LicenseServer.cs

:: Kurulum sihirbazı (ana uygulama ve lisans sunucusunu gömülü kaynak olarak paketler)
%CSC% /target:winexe /codepage:65001 /win32icon:app.ico /out:AdvancedDiskAnalyzerSetup.exe ^
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ^
  /resource:DiskAnalyzer.exe,DiskAnalyzer.exe ^
  /resource:LicenseServer.exe,LicenseServer.exe Setup.cs
```

---

## Gereksinimler

| Bileşen | Minimum Sürüm |
|---|---|
| İşletim Sistemi | Windows 10 veya üzeri |
| .NET Framework | 4.0 veya üzeri |
| Yönetici Yetkisi | NTFS Turbo Tarama için gerekli |
| Disk Alanı | ~5 MB (derlenmiş uygulama) |

---

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

```
MIT License

Copyright (c) 2025 Advanced Disk Analyzer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
