// Advanced Disk Analyzer License Server - minimal self-hosted issuer
// Build: csc.exe /target:exe /out:LicenseServer.exe /reference:System.Data.dll LicenseServer.cs

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
        private const string Prefix = "http://localhost:8765/";
        private const string PrivateKeyXml =
            "<RSAKeyValue><Modulus>rJdXjF5+pUzOTHWnIdekvB85+OJ5TvhLygVyCXV1EllnmTYsHHLLmo9f9OfFTMZMyQG/Lo6IHUkYoXP1uq5P8Vc3Gd9GFcnj0HaLHaYJueNlDIjj3YFxJrA99ntyYdi+cM36++t057tTBujqEDncP4zgX1T029IgNewt7+F5bTWGzSJU/hKBHFxmTh/0LbiwQRxf/qj5qKUX4O0bZDYiFKgU1noPCIynDDzxnOoIDS0i9FysYBHkJeFwz0nMA81YmOtacCp9Rlg5M3+aCFqAk4j1di+WUgjAb2m3WMn07+Y58qqHRR7Cs5OHVAyed6QXvo7QFtTm2RNiCGQarM4W0Q==</Modulus><Exponent>AQAB</Exponent><P>3KFxOAXjej8jBDwgVNSrRpqb6vMo9IEFBq9P3/JJdg04UUbWgOwhfTHzVlQx40bkupRvUO0dY89W+yl/K3VHZkZg/5OaSvCViRt7LugdjJAi5O5HMfa5tzbCFgEtNX5W7v048wTICtXl6omV0nxBt8uOM4PBhIEZDoRxHm1K8/8=</P><Q>yEJjLTpZp16rF/2r0COgOgRgiY3N9okAT1vuJKfiHkCtN4hnHq5/++Kv9mWRxcjGOJzsEt/lx0Dwkc2o8xTwojENokzlasaucwW5ZJ9UjoTahGEifBPalr3X68UXOXlukTWQW/7uBueo/1AgQDdvbDCiBPT5++5+Y/Rny8J4tS8=</Q><DP>OXZ+03WNKrC5AQhb71w4g7oO8+GDADN+SKBucEhdY7bLvTdy9L8Ldd0FoK1rFOPI/ONeHrizF+TPpbjIG1x/TR71cntSC0Sf3cbfjXb9AzgjLnb06gl1k0daw3po+O6/25zuMTVEmLXfHPfaoqikQSduEPK2+zjYetR51c/nXqc=</DP><DQ>fbKt7cWiYJvbaMOhBJRYDhKRRcXsccKsnyNk5z05gSO3lhPYJjoBu2keadp3FV5gjUhyJabD472vQtWEJpAOOr/vfuAlYFA1T51YMQCRYqOhRVZy/s63dcbTsPmVCk5eSGcGpbfuUAc43Ii+tZAWMCKWj+X0w11/l93hCNSYT6E=</DQ><InverseQ>jy6nSif6eqDpvF986cc4mM7MaV6ehoxEJAGaF2GEBZZ18wdtcK+Y+ew5IOVxiIQUpDxAMsass+9+epLtR2vsbqhwfsBSi/pzFAc9O9RfDKeIyXHjjE5ODN/asYv2XK8e7Sx5LDxUzrkmF1WAwecOEXOmnOLIT7II64Inz6iXAK0=</InverseQ><D>IlMvHEGm2mU+GkLD7J0grDFhrDgOfEAxgoo2td7gW7fgPL5jY30JNUISiXiW6r/9gXSRe0bplzl0ZpfTZT9Jsuvvj3uySp8OaeVJoanmAUxSSn3nI6Scxl8C08SVaRRcO78bjYK0i2ncB2HHO1bmkNUJwqv4zscplM/WCwFCzw8kfOv9JDoNdfQCk6xfZ3bBpHLIuQTyl2WafU8HL0OOEtuaUc4ZMzk0uSubeXmmJrD/or7cmjKxaLRkdhWslKO3fhF5+0ixy6jwxHSbZIPTu6kxRHzTx3JHWll4+R0JGeIYTysRryCNIUCluTEBNwVYqW16pfzTO2Nkr521FQk/6Q==</D></RSAKeyValue>";

        private static readonly object DbLock = new object();
        private static string dataDir;
        private static string dbPath;
        private static string adminTokenPath;
        private static string adminToken;
        private static string privateKeyPath;
        private static string privateKeyXml;

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

            Console.WriteLine("Advanced Disk Analyzer License Server");
            Console.WriteLine("Listening: " + Prefix);
            Console.WriteLine("Database : " + dbPath);
            Console.WriteLine("Admin token: " + adminTokenPath);
            Console.WriteLine("Private key: " + (File.Exists(privateKeyPath) ? privateKeyPath : "embedded development fallback"));
            Console.WriteLine("Endpoints: /health, /register, /login, /checkout, /activate, /admin/export?adminToken=...");
            Console.WriteLine("Press Ctrl+C to stop.");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(delegate { Handle(context); });
            }
        }

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
