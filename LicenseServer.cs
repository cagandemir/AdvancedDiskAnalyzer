// ═══════════════════════════════════════════════════════════════════
// DOSYA: LicenseServer.cs
// ═══════════════════════════════════════════════════════════════════
// Proje  : Advanced Disk Analyzer v2.0 – Lisans Sunucusu
// Amaç   : Kullanıcı kayıt, oturum açma, lisans satın alma ve
//           lisans aktivasyonu işlemlerini yürüten bağımsız HTTP
//           mikro-sunucu.
// Mimari : HttpListener tabanlı HTTP dinleyici + XML-backed DataSet
//           veritabanı.  Her istek ThreadPool üzerinde eşzamansız
//           olarak işlenir.
// Güvenlik: Parolalar PBKDF2-HMACSHA1 (120 000 iterasyon) ile
//           türetilir; lisans anahtarları RSA-SHA256 ile dijital
//           olarak imzalanır.
// Derleme: csc.exe /target:exe /out:LicenseServer.exe
//                   /reference:System.Data.dll LicenseServer.cs
// ═══════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AdvancedDiskAnalyzer.Licensing
{
    public class LicenseServerProgram
    {
        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: SUNUCU YAPILANDIRMASI VE DURUM DEĞİŞKENLERİ
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : HTTP dinleyici adresi, RSA özel anahtarı ve dosya
        //          sistemindeki veri dizin yollarını tanımlar.
        // Yöntemi: Sabitler derleme zamanında, statik alanlar ise
        //          Main() başlangıcında atanır.
        // Notlar : DbLock nesnesi tüm veritabanı erişimlerini
        //          seri hale getirerek eşzamanlılık güvenliği sağlar.
        // ═══════════════════════════════════════════════════════════════

        private const string Prefix = "http://localhost:8765/";           // HTTP dinleyici bağlantı noktası
        private const string PrivateKeyXml =                               // Gömülü RSA özel anahtar (yalnızca geliştirme ortamı için)
            "<RSAKeyValue><Modulus>rJdXjF5+pUzOTHWnIdekvB85+OJ5TvhLygVyCXV1EllnmTYsHHLLmo9f9OfFTMZMyQG/Lo6IHUkYoXP1uq5P8Vc3Gd9GFcnj0HaLHaYJueNlDIjj3YFxJrA99ntyYdi+cM36++t057tTBujqEDncP4zgX1T029IgNewt7+F5bTWGzSJU/hKBHFxmTh/0LbiwQRxf/qj5qKUX4O0bZDYiFKgU1noPCIynDDzxnOoIDS0i9FysYBHkJeFwz0nMA81YmOtacCp9Rlg5M3+aCFqAk4j1di+WUgjAb2m3WMn07+Y58qqHRR7Cs5OHVAyed6QXvo7QFtTm2RNiCGQarM4W0Q==</Modulus><Exponent>AQAB</Exponent><P>3KFxOAXjej8jBDwgVNSrRpqb6vMo9IEFBq9P3/JJdg04UUbWgOwhfTHzVlQx40bkupRvUO0dY89W+yl/K3VHZkZg/5OaSvCViRt7LugdjJAi5O5HMfa5tzbCFgEtNX5W7v048wTICtXl6omV0nxBt8uOM4PBhIEZDoRxHm1K8/8=</P><Q>yEJjLTpZp16rF/2r0COgOgRgiY3N9okAT1vuJKfiHkCtN4hnHq5/++Kv9mWRxcjGOJzsEt/lx0Dwkc2o8xTwojENokzlasaucwW5ZJ9UjoTahGEifBPalr3X68UXOXlukTWQW/7uBueo/1AgQDdvbDCiBPT5++5+Y/Rny8J4tS8=</Q><DP>OXZ+03WNKrC5AQhb71w4g7oO8+GDADN+SKBucEhdY7bLvTdy9L8Ldd0FoK1rFOPI/ONeHrizF+TPpbjIG1x/TR71cntSC0Sf3cbfjXb9AzgjLnb06gl1k0daw3po+O6/25zuMTVEmLXfHPfaoqikQSduEPK2+zjYetR51c/nXqc=</DP><DQ>fbKt7cWiYJvbaMOhBJRYDhKRRcXsccKsnyNk5z05gSO3lhPYJjoBu2keadp3FV5gjUhyJabD472vQtWEJpAOOr/vfuAlYFA1T51YMQCRYqOhRVZy/s63dcbTsPmVCk5eSGcGpbfuUAc43Ii+tZAWMCKWj+X0w11/l93hCNSYT6E=</DQ><InverseQ>jy6nSif6eqDpvF986cc4mM7MaV6ehoxEJAGaF2GEBZZ18wdtcK+Y+ew5IOVxiIQUpDxAMsass+9+epLtR2vsbqhwfsBSi/pzFAc9O9RfDKeIyXHjjE5ODN/asYv2XK8e7Sx5LDxUzrkmF1WAwecOEXOmnOLIT7II64Inz6iXAK0=</InverseQ><D>IlMvHEGm2mU+GkLD7J0grDFhrDgOfEAxgoo2td7gW7fgPL5jY30JNUISiXiW6r/9gXSRe0bplzl0ZpfTZT9Jsuvvj3uySp8OaeVJoanmAUxSSn3nI6Scxl8C08SVaRRcO78bjYK0i2ncB2HHO1bmkNUJwqv4zscplM/WCwFCzw8kfOv9JDoNdfQCk6xfZ3bBpHLIuQTyl2WafU8HL0OOEtuaUc4ZMzk0uSubeXmmJrD/or7cmjKxaLRkdhWslKO3fhF5+0ixy6jwxHSbZIPTu6kxRHzTx3JHWll4+R0JGeIYTysRryCNIUCluTEBNwVYqW16pfzTO2Nkr521FQk/6Q==</D></RSAKeyValue>";

        private static readonly object DbLock = new object();              // Veritabanı eşzamanlılık kilidi
        private static string dataDir;                                     // Sunucu veri dizini (%LOCALAPPDATA%)
        private static string dbPath;                                      // XML veritabanı dosya yolu
        private static string adminTokenPath;                              // Yönetici jeton dosya yolu
        private static string adminToken;                                  // Aktif yönetici jetonu
        private static string privateKeyPath;                              // Harici RSA özel anahtar dosyası
        private static string privateKeyXml;                               // Çalışma zamanı RSA özel anahtarı (XML)

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: ANA GİRİŞ NOKTASI – SUNUCU BAŞLATMA
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Yapılandırma dosyalarını yükler, XML veritabanını
        //          hazırlar ve HTTP dinleyiciyi başlatır.
        // Yöntemi: HttpListener belirtilen prefix üzerinde dinlemeye
        //          geçer; her gelen istek ThreadPool'a devredilir.
        // Notlar : Sonsuz döngü ile çalışır – Ctrl+C ile durdurulur.
        // ═══════════════════════════════════════════════════════════════
        public static void Main(string[] args)
        {
            dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AdvancedDiskAnalyzer", "LicenseServer");
            dbPath = Path.Combine(dataDir, "license-db.xml");
            adminTokenPath = Path.Combine(dataDir, "admin-token.txt");
            privateKeyPath = Path.Combine(dataDir, "private-key.xml");
            Directory.CreateDirectory(dataDir);
            adminToken = LoadOrCreateAdminToken();
            privateKeyXml = LoadPrivateKeyXml();
            EnsureDatabase();

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(Prefix);
            listener.Start();

            Console.WriteLine("Advanced Disk Analyzer Lisans Sunucusu");
            Console.WriteLine("Dinleme adresi: " + Prefix);
            Console.WriteLine("Veritabanı     : " + dbPath);
            Console.WriteLine("Yönetici token : " + adminTokenPath);
            Console.WriteLine("Özel anahtar   : " + (File.Exists(privateKeyPath) ? privateKeyPath : "gömülü geliştirme yedeği"));
            Console.WriteLine("Uç noktalar    : /health, /register, /login, /checkout, /activate, /admin/export?adminToken=...");
            Console.WriteLine("Durdurmak için Ctrl+C kullanın.");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(delegate { Handle(context); });
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: HTTP İSTEK YÖNLENDİRİCİ (ROUTER)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Gelen HTTP isteğinin URL yolunu ayrıştırarak
        //          ilgili uç nokta yöntemine (endpoint) yönlendirir.
        // Yöntemi: AbsolutePath değeri normalize edilir ve basit
        //          if-else zinciri ile eşleştirilir.
        // Notlar : Bilinmeyen yollar 404, beklenmeyen hatalar 500
        //          durum kodu ile yanıtlanır.
        // ═══════════════════════════════════════════════════════════════
        private static void Handle(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url.AbsolutePath.Trim('/').ToLowerInvariant();
                if (path == "health")
                    Write(context, "OK\nMESSAGE=License server online");
                else if (path == "signup" || path == "register")
                    Register(context);
                else if (path == "login")
                    Login(context);
                else if (path == "checkout")
                    Checkout(context);
                else if (path == "activate")
                    Activate(context);
                else if (path == "admin/export")
                    Export(context);
                else
                    Write(context, "ERROR\nMESSAGE=Unknown endpoint", 404);
            }
            catch (Exception ex)
            {
                Write(context, "ERROR\nMESSAGE=" + ex.Message, 500);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: KULLANICI KAYIT UÇ NOKTASI (/register)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Yeni kullanıcı hesabı oluşturur ve oturum jetonu
        //          döndürür.
        // Yöntemi: E-posta tekrarlılık kontrolü → PBKDF2 ile parola
        //          türetme → hesap kaydı → oturum oluşturma.
        // Notlar : E-posta zaten varsa HTTP 409 (Conflict) döner.
        // ═══════════════════════════════════════════════════════════════
        private static void Register(HttpListenerContext context)
        {
            Dictionary<string, string> fields = RequestFields(context);
            string email = V(fields, "email");
            string company = V(fields, "company");
            string password = V(fields, "password");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Write(context, "ERROR\nMESSAGE=Email and password required", 400);
                return;
            }

            string token;
            lock (DbLock)
            {
                DataSet db = LoadDatabase();
                if (FindAccount(db, email) != null)
                {
                    Write(context, "ERROR\nMESSAGE=Account already exists", 409);
                    return;
                }
                CreateAccount(db, email, company, password);
                token = CreateSession(db, email);
                SaveDatabase(db);
            }
            Write(context, "OK\nMESSAGE=Account created\nTOKEN=" + token);
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: KİMLİK DOĞRULAMA UÇ NOKTASI (/login)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Mevcut kullanıcının e-posta ve parola çifti ile
        //          kimliğini doğrular, geçerli oturum jetonu üretir.
        // Yöntemi: Hesap kaydı bulunur → PBKDF2 hash doğrulaması →
        //          son giriş zaman damgası güncellenir → yeni oturum.
        // Notlar : Hatalı kimlik bilgisi HTTP 401 ile reddedilir.
        // ═══════════════════════════════════════════════════════════════
        private static void Login(HttpListenerContext context)
        {
            Dictionary<string, string> fields = RequestFields(context);
            string email = V(fields, "email");
            string password = V(fields, "password");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Write(context, "ERROR\nMESSAGE=Email and password required", 400);
                return;
            }

            string token;
            lock (DbLock)
            {
                DataSet db = LoadDatabase();
                DataRow account = FindAccount(db, email);
                if (account == null || !VerifyPassword(password, account["PasswordSalt"].ToString(), account["PasswordHash"].ToString()))
                {
                    Write(context, "ERROR\nMESSAGE=Invalid email or password", 401);
                    return;
                }
                account["LastLoginAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                token = CreateSession(db, email);
                SaveDatabase(db);
            }
            Write(context, "OK\nMESSAGE=Login successful\nTOKEN=" + token);
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: LİSANS SATIN ALMA UÇ NOKTASI (/checkout)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Oturum doğrulaması sonrası seçilen plana göre
        //          lisans anahtarı üretir ve sipariş kaydı oluşturur.
        // Yöntemi: Oturum jetonu kontrolü → şirket bilgisi güncelleme
        //          → sipariş satırı ekleme → RSA imzalı lisans üretme.
        // Notlar : email, token, plan ve hardwareId zorunlu alanlardır.
        // ═══════════════════════════════════════════════════════════════
        private static void Checkout(HttpListenerContext context)
        {
            Dictionary<string, string> fields = RequestFields(context);
            string email = V(fields, "email");
            string company = V(fields, "company");
            string plan = NormalizePlan(V(fields, "plan"));
            string hardwareId = V(fields, "hardwareId");
            string token = V(fields, "token");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(hardwareId) || string.IsNullOrEmpty(plan) || string.IsNullOrEmpty(token))
            {
                Write(context, "ERROR\nMESSAGE=email, token, plan and hardwareId required", 400);
                return;
            }

            string licenseKey;
            lock (DbLock)
            {
                DataSet db = LoadDatabase();
                if (!ValidateSession(db, email, token))
                {
                    Write(context, "ERROR\nMESSAGE=Login required", 401);
                    return;
                }
                UpdateCompany(db, email, company);
                AddOrder(db, email, plan, "Paid-Demo");
                licenseKey = AddLicense(db, email, company, plan, hardwareId);
                SaveDatabase(db);
            }

            Write(context, "OK\nMESSAGE=Checkout completed\nLICENSE=" + licenseKey);
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: LİSANS AKTİVASYONU UÇ NOKTASI (/activate)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Kullanıcıya ait en uygun lisans kaydını bularak
        //          lisans anahtarını döndürür.
        // Yöntemi: Oturum doğrulaması → Licenses tablosu e-posta ve
        //          donanım kimliği (hardwareId) ile filtrelenir.
        //          Joker "*" donanım kimliği her cihazla eşleşir.
        // Notlar : Eşleşen lisans bulunamazsa HTTP 404 döner.
        // ═══════════════════════════════════════════════════════════════
        private static void Activate(HttpListenerContext context)
        {
            Dictionary<string, string> fields = RequestFields(context);
            string email = V(fields, "email");
            string hardwareId = V(fields, "hardwareId");
            string token = V(fields, "token");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(hardwareId) || string.IsNullOrEmpty(token))
            {
                Write(context, "ERROR\nMESSAGE=email, token and hardwareId required", 400);
                return;
            }

            lock (DbLock)
            {
                DataSet db = LoadDatabase();
                if (!ValidateSession(db, email, token))
                {
                    Write(context, "ERROR\nMESSAGE=Login required", 401);
                    return;
                }
                DataTable licenses = db.Tables["Licenses"];
                DataRow best = null;
                foreach (DataRow row in licenses.Rows)
                {
                    if (!Same(row["Email"].ToString(), email)) continue;
                    string hw = row["HardwareId"].ToString();
                    if (hw != "*" && !Same(hw, hardwareId)) continue;
                    best = row;
                }

                if (best == null)
                {
                    Write(context, "ERROR\nMESSAGE=No license found", 404);
                    return;
                }

                Write(context, "OK\nMESSAGE=Activation found\nLICENSE=" + best["Key"]);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: YÖNETİCİ VERİ DIŞA AKTARIMI (/admin/export)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Yönetici jetonunu doğruladıktan sonra veritabanı
        //          tablolarının tamamını düz metin biçiminde döndürür.
        // Yöntemi: adminToken eşleşmesi → tüm DataTable satırları
        //          sekme-ayraçlı (TSV) biçimde serileştirilir.
        // Notlar : Bu uç nokta hassas veri içerir; yalnızca
        //          geçerli yönetici jetonu ile erişilebilir.
        // ═══════════════════════════════════════════════════════════════
        private static void Export(HttpListenerContext context)
        {
            Dictionary<string, string> fields = RequestFields(context);
            if (!Same(V(fields, "adminToken"), adminToken))
            {
                Write(context, "ERROR\nMESSAGE=Admin token required", 401);
                return;
            }

            lock (DbLock)
            {
                DataSet db = LoadDatabase();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("OK");
                foreach (DataTable table in db.Tables)
                {
                    sb.AppendLine("[" + table.TableName + "]");
                    foreach (DataRow row in table.Rows)
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            if (i > 0) sb.Append("\t");
                            sb.Append(row[i]);
                        }
                        sb.AppendLine();
                    }
                }
                Write(context, sb.ToString());
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: LİSANS ANAHTARI ÜRETİMİ
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Verilen plan ve kullanıcı bilgilerine göre
        //          RSA-SHA256 ile imzalanmış lisans anahtarı üretir.
        // Yöntemi: Payload formatı "ADA1|PLAN|Şirket|E-posta|
        //          SonKullanma|KoltukSayısı|DonanımId" şeklindedir.
        //          İmza payload sonuna eklenerek tek parça anahtar
        //          dizgisi oluşturulur.
        // Notlar : PRO planı 1 yıl sürelidir; ENTERPRISE planı
        //          250 koltuk destekler.
        // ═══════════════════════════════════════════════════════════════
        private static string AddLicense(DataSet db, string email, string company, string plan, string hardwareId)
        {
            string expires = plan == "PRO" ? DateTime.Now.AddYears(1).ToString("yyyy-MM-dd") : "PERPETUAL";
            string seats = plan == "ENTERPRISE" ? "250" : "1";
            string payload = "ADA1|" + plan + "|" + Clean(company) + "|" + Clean(email) + "|" + expires + "|" + seats + "|" + hardwareId;
            string key = payload + "|" + Sign(payload);

            DataTable table = db.Tables["Licenses"];
            DataRow row = table.NewRow();
            row["Id"] = Guid.NewGuid().ToString("N");
            row["Email"] = email;
            row["Company"] = company;
            row["Plan"] = plan;
            row["HardwareId"] = hardwareId;
            row["Key"] = key;
            row["Expires"] = expires;
            row["CreatedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            table.Rows.Add(row);
            return key;
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: RSA-SHA256 DİJİTAL İMZALAMA
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Verilen payload metnini RSA özel anahtarı ile
        //          SHA-256 özet algoritması kullanarak imzalar.
        // Yöntemi: RSACryptoServiceProvider ile SignData çağrısı →
        //          imza bayt dizisi Base64 kodlanarak döndürülür.
        // Notlar : Doğrulama istemci tarafında eşleşen RSA açık
        //          anahtarı ile yapılır.
        // ═══════════════════════════════════════════════════════════════
        private static string Sign(string payload)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKeyXml);
                byte[] data = Encoding.UTF8.GetBytes(payload);
                byte[] sig = rsa.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(sig);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: HESAP YARDIMCI YÖNTEMLERİ
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Accounts tablosu üzerinde CRUD işlemleri sağlar:
        //          FindAccount  – e-posta ile hesap arama
        //          CreateAccount – yeni hesap oluşturma
        //          UpdateCompany – şirket bilgisi güncelleme
        // Yöntemi: DataTable.Rows üzerinde doğrusal tarama; büyük/
        //          küçük harf duyarsız karşılaştırma (Same yöntemi).
        // Notlar : Parola doğrudan saklanmaz – salt + hash çifti
        //          PBKDF2 ile türetilir.
        // ═══════════════════════════════════════════════════════════════
        private static DataRow FindAccount(DataSet db, string email)
        {
            DataTable table = db.Tables["Accounts"];
            foreach (DataRow row in table.Rows)
            {
                if (Same(row["Email"].ToString(), email))
                    return row;
            }
            return null;
        }

        private static void CreateAccount(DataSet db, string email, string company, string password)
        {
            DataTable table = db.Tables["Accounts"];
            string salt;
            string hash = HashPassword(password, out salt);

            DataRow newRow = table.NewRow();
            newRow["Id"] = Guid.NewGuid().ToString("N");
            newRow["Email"] = email;
            newRow["Company"] = company;
            newRow["PasswordSalt"] = salt;
            newRow["PasswordHash"] = hash;
            newRow["CreatedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["LastLoginAt"] = "";
            table.Rows.Add(newRow);
        }

        private static void UpdateCompany(DataSet db, string email, string company)
        {
            DataRow account = FindAccount(db, email);
            if (account != null && !string.IsNullOrEmpty(company))
                account["Company"] = company;
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: OTURUM YÖNETİMİ
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Kullanıcı oturumlarının oluşturulması ve
        //          doğrulanmasını yönetir.
        // Yöntemi: CreateSession  – 256-bit rastgele jeton üretir,
        //          30 günlük geçerlilik süresi ile Sessions tablosuna
        //          yazar.
        //          ValidateSession – jeton + e-posta eşleşmesi ve
        //          süre sonu kontrolü yapar.
        // Notlar : Oturum jetonları URL-güvenli Base64 formatındadır.
        // ═══════════════════════════════════════════════════════════════
        private static string CreateSession(DataSet db, string email)
        {
            string token = CreateSecretToken();

            DataRow row = db.Tables["Sessions"].NewRow();
            row["Token"] = token;
            row["Email"] = email;
            row["CreatedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["ExpiresAt"] = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss");
            db.Tables["Sessions"].Rows.Add(row);
            return token;
        }

        private static bool ValidateSession(DataSet db, string email, string token)
        {
            DateTime now = DateTime.Now;
            foreach (DataRow row in db.Tables["Sessions"].Rows)
            {
                if (!Same(row["Email"].ToString(), email)) continue;
                if (row["Token"].ToString() != token) continue;
                DateTime expires;
                if (!DateTime.TryParse(row["ExpiresAt"].ToString(), out expires)) return false;
                return expires > now;
            }
            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: PAROLA KARMA (PBKDF2 ANAHTAR TÜRETMESİ)
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : Kullanıcı parolalarını güvenli biçimde saklamak
        //          ve doğrulamak için kriptografik hash üretir.
        // Yöntemi: HashPassword   – 16 bayt rastgele tuz oluşturur,
        //          Rfc2898DeriveBytes (PBKDF2-HMACSHA1) ile 120 000
        //          iterasyon uygulayarak 32 baytlık anahtar türetir.
        //          VerifyPassword – aynı tuz ile yeniden türetilen
        //          hash, saklanan değerle sabit-zamanlı XOR
        //          karşılaştırması ile doğrulanır (zamanlama saldırısı
        //          koruması).
        // Notlar : Iterasyon sayısı (120 000) kaba kuvvet saldırılarına
        //          karşı yeterli gecikme sağlar.
        // ═══════════════════════════════════════════════════════════════
        private static string HashPassword(string password, out string saltText)
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);
            saltText = Convert.ToBase64String(salt);
            using (Rfc2898DeriveBytes pbkdf = new Rfc2898DeriveBytes(password, salt, 120000))
                return Convert.ToBase64String(pbkdf.GetBytes(32));
        }

        private static bool VerifyPassword(string password, string saltText, string expectedHash)
        {
            try
            {
                byte[] salt = Convert.FromBase64String(saltText);
                using (Rfc2898DeriveBytes pbkdf = new Rfc2898DeriveBytes(password, salt, 120000))
                {
                    byte[] actual = pbkdf.GetBytes(32);
                    byte[] expected = Convert.FromBase64String(expectedHash);
                    if (actual.Length != expected.Length) return false;
                    int diff = 0;
                    for (int i = 0; i < actual.Length; i++)
                        diff |= actual[i] ^ expected[i];
                    return diff == 0;
                }
            }
            catch { return false; }
        }

        // Sipariş kaydı – Orders tablosuna yeni satır ekler
        private static void AddOrder(DataSet db, string email, string plan, string status)
        {
            DataRow row = db.Tables["Orders"].NewRow();
            row["Id"] = Guid.NewGuid().ToString("N");
            row["Email"] = email;
            row["Plan"] = plan;
            row["Status"] = status;
            row["CreatedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.Tables["Orders"].Rows.Add(row);
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: XML VERİTABANI KATMANI
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : DataSet tabanlı kalıcı depolama işlemlerini
        //          yönetir: oluşturma, yükleme, kaydetme.
        // Yöntemi: EnsureDatabase  – dosya yoksa oluşturur, varsa
        //          yükler ve şema tutarlılığını doğrular.
        //          LoadDatabase    – XML dosyasını ReadSchema ile
        //          okuyarak DataSet'e yükler.
        //          SaveDatabase    – DataSet'i WriteSchema ile diske
        //          XML olarak serileştirir.
        //          EnsureTables    – Accounts, Orders, Licenses,
        //          Sessions tablolarının varlığını garanti eder.
        // Notlar : Tüm veritabanı erişimleri DbLock altında yapılır.
        // ═══════════════════════════════════════════════════════════════
        private static void EnsureDatabase()
        {
            lock (DbLock)
            {
                DataSet db = File.Exists(dbPath) ? LoadDatabase() : CreateDatabase();
                SaveDatabase(db);
            }
        }

        private static DataSet LoadDatabase()
        {
            if (!File.Exists(dbPath)) return CreateDatabase();
            DataSet db = new DataSet("AdvancedDiskAnalyzerLicensing");
            db.ReadXml(dbPath, XmlReadMode.ReadSchema);
            EnsureTables(db);
            return db;
        }

        private static void SaveDatabase(DataSet db)
        {
            db.WriteXml(dbPath, XmlWriteMode.WriteSchema);
        }

        private static DataSet CreateDatabase()
        {
            DataSet db = new DataSet("AdvancedDiskAnalyzerLicensing");
            EnsureTables(db);
            return db;
        }

        private static void EnsureTables(DataSet db)
        {
            EnsureTable(db, "Accounts", new string[] { "Id", "Email", "Company", "PasswordSalt", "PasswordHash", "CreatedAt", "LastLoginAt" });
            EnsureTable(db, "Orders", new string[] { "Id", "Email", "Plan", "Status", "CreatedAt" });
            EnsureTable(db, "Licenses", new string[] { "Id", "Email", "Company", "Plan", "HardwareId", "Key", "Expires", "CreatedAt" });
            EnsureTable(db, "Sessions", new string[] { "Token", "Email", "CreatedAt", "ExpiresAt" });
        }

        private static void EnsureTable(DataSet db, string name, string[] columns)
        {
            if (!db.Tables.Contains(name))
                db.Tables.Add(name);
            DataTable table = db.Tables[name];
            foreach (string column in columns)
                if (!table.Columns.Contains(column))
                    table.Columns.Add(column, typeof(string));
        }

        // ═══════════════════════════════════════════════════════════════
        // BÖLÜM: YARDIMCI ARAÇLAR VE İSTEK AYRIŞTIRMA
        // ═══════════════════════════════════════════════════════════════
        // Amacı  : HTTP istek parametrelerini ayrıştırır, güvenlik
        //          jetonları üretir ve yanıt yazma işlemlerini
        //          merkezileştirir.
        // Yöntemi: RequestFields     – QueryString + form-encoded
        //          gövdeyi birleştirerek alan sözlüğü oluşturur.
        //          ParseFormEncoded  – application/x-www-form-urlencoded
        //          gövdeyi ayrıştırır.
        //          CreateSecretToken – 256-bit CSPRNG ile URL-güvenli
        //          rastgele jeton üretir.
        //          Write             – UTF-8 düz metin HTTP yanıtı
        //          döndürür.
        // Notlar : V() boş alan için güvenli varsayılan değer sağlar.
        // ═══════════════════════════════════════════════════════════════
        private static Dictionary<string, string> RequestFields(HttpListenerContext context)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in context.Request.QueryString.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                    fields[key] = context.Request.QueryString[key] ?? "";
            }

            if (context.Request.HasEntityBody)
            {
                try
                {
                    Encoding encoding = context.Request.ContentEncoding ?? Encoding.UTF8;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream, encoding))
                    {
                        ParseFormEncoded(reader.ReadToEnd(), fields);
                    }
                }
                catch { }
            }

            return fields;
        }

        private static void ParseFormEncoded(string body, Dictionary<string, string> fields)
        {
            if (string.IsNullOrEmpty(body)) return;
            string[] parts = body.Split('&');
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                int equals = part.IndexOf('=');
                string rawKey = equals >= 0 ? part.Substring(0, equals) : part;
                string rawValue = equals >= 0 ? part.Substring(equals + 1) : "";
                string key = Uri.UnescapeDataString(rawKey.Replace("+", " "));
                if (string.IsNullOrEmpty(key)) continue;
                fields[key] = Uri.UnescapeDataString(rawValue.Replace("+", " "));
            }
        }

        private static string V(Dictionary<string, string> fields, string key)
        {
            string value;
            if (fields != null && fields.TryGetValue(key, out value))
                return value ?? "";
            return "";
        }

        private static string LoadOrCreateAdminToken()
        {
            string env = Environment.GetEnvironmentVariable("ADA_LICENSE_ADMIN_TOKEN");
            if (!string.IsNullOrEmpty(env))
                return env.Trim();

            try
            {
                if (File.Exists(adminTokenPath))
                    return File.ReadAllText(adminTokenPath, Encoding.UTF8).Trim();
            }
            catch { }

            string token = CreateSecretToken();
            try { File.WriteAllText(adminTokenPath, token, Encoding.UTF8); }
            catch { }
            return token;
        }

        private static string LoadPrivateKeyXml()
        {
            string env = Environment.GetEnvironmentVariable("ADA_LICENSE_PRIVATE_KEY_XML");
            if (!string.IsNullOrEmpty(env))
                return env.Trim();

            try
            {
                if (File.Exists(privateKeyPath))
                    return File.ReadAllText(privateKeyPath, Encoding.UTF8).Trim();
            }
            catch { }

            // Gelistirme kolayligi icin geri uyumluluk; canli kurulumda ADA_LICENSE_PRIVATE_KEY_XML
            // veya private-key.xml kullanilmali, exe icindeki fallback anahtar ile dagitim yapilmamali.
            return PrivateKeyXml;
        }

        private static string CreateSecretToken()
        {
            byte[] random = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(random);
            return Convert.ToBase64String(random).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string NormalizePlan(string plan)
        {
            if (string.Equals(plan, "PRO", StringComparison.OrdinalIgnoreCase)) return "PRO";
            if (string.Equals(plan, "ENTERPRISE", StringComparison.OrdinalIgnoreCase)) return "ENTERPRISE";
            return "";
        }

        private static bool Same(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string Clean(string value)
        {
            return (value ?? "").Replace("|", " ").Trim();
        }

        private static void Write(HttpListenerContext context, string text)
        {
            Write(context, text, 200);
        }

        private static void Write(HttpListenerContext context, string text, int statusCode)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }
    }
}
