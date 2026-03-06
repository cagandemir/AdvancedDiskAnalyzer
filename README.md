# Advanced Disk Analyzer v1.6

Disk kullanımını analiz eden, gereksiz dosyaları tespit eden ve temizleme önerileri sunan bir Windows masaüstü uygulaması.

## Özellikler

- **Hızlı Tarama** — M.2/SSD için tam paralel, HDD için optimize edilmiş tarama
- **Canlı Tarama** — Tarama sırasında dosyalar anlık olarak listelenir
- **Pasta Grafik** — Klasör boyut dağılımı, hover ile detay gösterimi
- **AI Önerileri** — Silinebilecek dosyalar, büyük & eski dosyalar, en büyük klasörler
- **Toplu Silme** — Geçici/log/bak dosyalarını tek tıkla temizle
- **Sütun Sıralama** — Dosya adı, boyut, skor, tarihe göre A→Z / Z→A sıralama
- **Sağ Tık Menüsü** — Explorer'da aç, yolu kopyala, sil
- **Koyu / Açık Tema** — Tek tıkla tema değiştirme

## Kurulum & Derleme

### Gereksinimler
- Windows 10 veya üzeri
- .NET Framework 4.0+

### Derleme (CMD)
```cmd
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
  /target:winexe ^
  /out:DiskAnalyzer.exe ^
  /reference:System.Windows.Forms.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Management.dll ^
  Program.cs
```

Derleme sonrası oluşan `DiskAnalyzer.exe` dosyasını çalıştırın.

## Kullanım

1. **Tara** butonuna tıklayın
2. Taramak istediğiniz klasörü seçin
3. Tarama biterken dosyalar canlı olarak listelenir
4. Sağ panelde **AI Analiz** sekmesinden önerileri inceleyin
5. **Tümünü Sil** ile gereksiz dosyaları temizleyin

## Skor Sistemi

Her dosyaya 0-100 arası bir "gereksizlik skoru" verilir:

| Kriter | Ağırlık |
|--------|---------|
| Dosya boyutu (500MB baz) | %40 |
| Yaş (1 yıldan eski) | %40 |
| Geçici dosya tipi (.tmp .log .bak .old) | %20 |

- 🔴 **70+** — Büyük ihtimalle silinebilir
- 🟡 **40-69** — İncelemeye değer
- ⚪ **0-39** — Aktif kullanılan dosya

## Lisans

MIT License
