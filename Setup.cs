// Advanced Disk Analyzer Setup
// Build after DiskAnalyzer.exe and LicenseServer.exe:
// csc.exe /target:winexe /win32icon:app.ico /out:AdvancedDiskAnalyzerSetup.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /resource:DiskAnalyzer.exe,DiskAnalyzer.exe /resource:LicenseServer.exe,LicenseServer.exe Setup.cs

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AdvancedDiskAnalyzer.Setup
{
    static class SetupProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args != null && args.Length > 0 && string.Equals(args[0], "/uninstall", StringComparison.OrdinalIgnoreCase))
            {
                Application.Run(new UninstallForm());
                return;
            }

            Application.Run(new SetupForm());
        }
    }

    static class InstallerIconFactory
    {
        private static Icon cachedIcon;

        public static Icon GetIcon()
        {
            if (cachedIcon != null) return cachedIcon;
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                {
                    cachedIcon = icon;
                    return cachedIcon;
                }
            }
            catch { }

            cachedIcon = SystemIcons.Application;
            return cachedIcon;
        }
    }

    public class SetupForm : Form
    {
        private const string SetupProductName = "Advanced Disk Analyzer";
        private const string SetupProductVersion = "v1.9 Beta";
        private const string Publisher = "Advanced Disk Analyzer";
        private const string InstallFolderName = "Advanced Disk Analyzer";

        private Panel headerPanel;
        private Panel contentPanel;
        private Panel footerPanel;
        private Label titleLabel;
        private Label subtitleLabel;
        private Label footerVersionLabel;
        private Button backButton;
        private Button nextButton;
        private Button cancelButton;

        private RichTextBox licenseBox;
        private RadioButton acceptRadio;
        private RadioButton declineRadio;
        private TextBox installPathBox;
        private CheckBox startMenuCheck;
        private CheckBox desktopCheck;
        private CheckBox runAfterCheck;
        private CheckBox licenseServerCheck;
        private ProgressBar progressBar;
        private Label progressLabel;

        private int pageIndex = 0;
        private bool installCompleted = false;

        private Color bg = Color.FromArgb(245, 246, 248);
        private Color surface = Color.White;
        private Color border = Color.FromArgb(205, 210, 218);
        private Color text = Color.FromArgb(22, 27, 35);
        private Color subText = Color.FromArgb(88, 96, 110);
        private Color accent = Color.FromArgb(47, 129, 247);

        public SetupForm()
        {
            this.Text = SetupProductName + " - Kurulum";
            this.Size = new Size(860, 640);
            this.MinimumSize = new Size(780, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = bg;
            this.Font = new Font("Segoe UI", 9);
            this.Icon = InstallerIconFactory.GetIcon();

            BuildChrome();
            ShowLicensePage();
        }

        private void BuildChrome()
        {
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 104;
            headerPanel.BackColor = surface;
            this.Controls.Add(headerPanel);

            titleLabel = new Label();
            titleLabel.Location = new Point(42, 22);
            titleLabel.Size = new Size(620, 28);
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = text;
            headerPanel.Controls.Add(titleLabel);

            subtitleLabel = new Label();
            subtitleLabel.Location = new Point(42, 52);
            subtitleLabel.Size = new Size(650, 26);
            subtitleLabel.Font = new Font("Segoe UI", 10);
            subtitleLabel.ForeColor = text;
            headerPanel.Controls.Add(subtitleLabel);

            InstallArtPanel art = new InstallArtPanel();
            art.Location = new Point(742, 16);
            art.Size = new Size(72, 72);
            headerPanel.Controls.Add(art);

            Panel headerLine = new Panel();
            headerLine.Dock = DockStyle.Bottom;
            headerLine.Height = 1;
            headerLine.BackColor = border;
            headerPanel.Controls.Add(headerLine);

            footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 80;
            footerPanel.BackColor = bg;
            this.Controls.Add(footerPanel);

            Panel footerLine = new Panel();
            footerLine.Dock = DockStyle.Top;
            footerLine.Height = 1;
            footerLine.BackColor = border;
            footerPanel.Controls.Add(footerLine);

            footerVersionLabel = new Label();
            footerVersionLabel.Text = SetupProductName + " " + SetupProductVersion;
            footerVersionLabel.Location = new Point(30, 28);
            footerVersionLabel.Size = new Size(310, 24);
            footerVersionLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            footerVersionLabel.ForeColor = text;
            footerPanel.Controls.Add(footerVersionLabel);

            backButton = new Button();
            backButton.Text = "< Geri";
            backButton.Size = new Size(120, 36);
            backButton.Location = new Point(470, 22);
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.FlatAppearance.BorderColor = border;
            backButton.Click += delegate { Back(); };
            footerPanel.Controls.Add(backButton);

            nextButton = new Button();
            nextButton.Text = "Ileri >";
            nextButton.Size = new Size(120, 36);
            nextButton.Location = new Point(602, 22);
            nextButton.FlatStyle = FlatStyle.Flat;
            nextButton.FlatAppearance.BorderColor = accent;
            nextButton.Click += delegate { Next(); };
            footerPanel.Controls.Add(nextButton);

            cancelButton = new Button();
            cancelButton.Text = "Iptal";
            cancelButton.Size = new Size(120, 36);
            cancelButton.Location = new Point(734, 22);
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderColor = border;
            cancelButton.Click += delegate { CloseSetup(); };
            footerPanel.Controls.Add(cancelButton);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = bg;
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();
        }

        private void ShowLicensePage()
        {
            pageIndex = 0;
            titleLabel.Text = "Lisans Anlasmasi";
            subtitleLabel.Text = "Devam etmeden once asagidaki onemli bilgileri okuyun.";
            ResetContent();

            Label intro = new Label();
            intro.Text = "Kuruluma devam edebilmek icin bu anlasmayi kabul etmelisiniz.";
            intro.Location = new Point(66, 28);
            intro.Size = new Size(720, 40);
            intro.Font = new Font("Segoe UI", 10);
            intro.ForeColor = text;
            contentPanel.Controls.Add(intro);

            licenseBox = new RichTextBox();
            licenseBox.Location = new Point(66, 76);
            licenseBox.Size = new Size(720, 250);
            licenseBox.ReadOnly = true;
            licenseBox.BorderStyle = BorderStyle.FixedSingle;
            licenseBox.BackColor = Color.White;
            licenseBox.ForeColor = Color.Black;
            licenseBox.Font = new Font("Segoe UI", 9);
            licenseBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            licenseBox.Text = BuildLicenseText();
            contentPanel.Controls.Add(licenseBox);

            acceptRadio = new RadioButton();
            acceptRadio.Text = "Anlasmayi kabul ediyorum.";
            acceptRadio.Location = new Point(66, 344);
            acceptRadio.Size = new Size(300, 26);
            acceptRadio.Font = new Font("Segoe UI", 10);
            acceptRadio.ForeColor = text;
            acceptRadio.CheckedChanged += delegate { UpdateButtons(); };
            contentPanel.Controls.Add(acceptRadio);

            declineRadio = new RadioButton();
            declineRadio.Text = "Anlasmayi kabul etmiyorum.";
            declineRadio.Location = new Point(66, 374);
            declineRadio.Size = new Size(330, 26);
            declineRadio.Font = new Font("Segoe UI", 10);
            declineRadio.ForeColor = text;
            declineRadio.Checked = true;
            declineRadio.CheckedChanged += delegate { UpdateButtons(); };
            contentPanel.Controls.Add(declineRadio);

            UpdateButtons();
        }

        private void ShowOptionsPage()
        {
            pageIndex = 1;
            titleLabel.Text = "Kurulum Ayarlari";
            subtitleLabel.Text = "Uygulamanin nereye kurulacagini ve hangi kisayollarin olusturulacagini secin.";
            ResetContent();

            Label pathLabel = new Label();
            pathLabel.Text = "Kurulum klasoru";
            pathLabel.Location = new Point(66, 34);
            pathLabel.Size = new Size(240, 24);
            pathLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            pathLabel.ForeColor = text;
            contentPanel.Controls.Add(pathLabel);

            installPathBox = new TextBox();
            installPathBox.Location = new Point(66, 64);
            installPathBox.Size = new Size(600, 24);
            installPathBox.Text = DefaultInstallPath();
            contentPanel.Controls.Add(installPathBox);

            Button browseButton = new Button();
            browseButton.Text = "Gozat...";
            browseButton.Location = new Point(676, 62);
            browseButton.Size = new Size(110, 28);
            browseButton.FlatStyle = FlatStyle.Flat;
            browseButton.FlatAppearance.BorderColor = border;
            browseButton.Click += BrowseInstallPath;
            contentPanel.Controls.Add(browseButton);

            startMenuCheck = OptionCheck("Baslat Menusu kisayolu olustur", 66, 122, true);
            desktopCheck = OptionCheck("Masaustune kisayol olustur", 66, 154, true);
            runAfterCheck = OptionCheck("Kurulum bitince uygulamayi ac", 66, 186, true);
            licenseServerCheck = OptionCheck("Kurumsal lisans sunucusu aracini da kur", 66, 218, true);

            Label note = new Label();
            note.Text = "Kurulum kullanici profilinize yapilir; yonetici izni gerekmez. Lisans ve tarama verileri dis sunucuya gonderilmez.";
            note.Location = new Point(66, 272);
            note.Size = new Size(720, 52);
            note.ForeColor = subText;
            note.Font = new Font("Segoe UI", 9);
            contentPanel.Controls.Add(note);

            UpdateButtons();
        }

        private CheckBox OptionCheck(string textValue, int x, int y, bool value)
        {
            CheckBox cb = new CheckBox();
            cb.Text = textValue;
            cb.Location = new Point(x, y);
            cb.Size = new Size(430, 24);
            cb.Checked = value;
            cb.ForeColor = text;
            cb.Font = new Font("Segoe UI", 10);
            contentPanel.Controls.Add(cb);
            return cb;
        }

        private void ShowInstallPage()
        {
            pageIndex = 2;
            titleLabel.Text = "Kurulum";
            subtitleLabel.Text = "Dosyalar kopyalaniyor ve kisayollar hazirlaniyor.";
            ResetContent();

            progressLabel = new Label();
            progressLabel.Text = "Kurulum baslatiliyor...";
            progressLabel.Location = new Point(66, 84);
            progressLabel.Size = new Size(720, 32);
            progressLabel.Font = new Font("Segoe UI", 10);
            progressLabel.ForeColor = text;
            contentPanel.Controls.Add(progressLabel);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(66, 126);
            progressBar.Size = new Size(720, 22);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            contentPanel.Controls.Add(progressBar);

            UpdateButtons();
            BeginInvoke(new MethodInvoker(InstallNow));
        }

        private void ShowDonePage()
        {
            pageIndex = 3;
            installCompleted = true;
            titleLabel.Text = "Kurulum Tamamlandi";
            subtitleLabel.Text = SetupProductName + " bilgisayariniza kuruldu.";
            ResetContent();

            Label done = new Label();
            done.Text = "Kurulum basariyla tamamlandi.\n\nUygulamayi Baslat Menusu veya masaustu kisayolundan acabilirsiniz.";
            done.Location = new Point(66, 82);
            done.Size = new Size(720, 90);
            done.Font = new Font("Segoe UI", 11);
            done.ForeColor = text;
            contentPanel.Controls.Add(done);

            UpdateButtons();
        }

        private void InstallNow()
        {
            try
            {
                string installDir = installPathBox.Text.Trim();
                if (string.IsNullOrEmpty(installDir))
                    installDir = DefaultInstallPath();

                SetProgress(10, "Kurulum klasoru hazirlaniyor...");
                Directory.CreateDirectory(installDir);

                string appPath = Path.Combine(installDir, "DiskAnalyzer.exe");
                string serverPath = Path.Combine(installDir, "LicenseServer.exe");
                string setupCopy = Path.Combine(installDir, "AdvancedDiskAnalyzerSetup.exe");

                SetProgress(30, "Uygulama dosyalari kopyalaniyor...");
                ExtractResource("DiskAnalyzer.exe", appPath);
                if (licenseServerCheck.Checked)
                    ExtractResource("LicenseServer.exe", serverPath);

                SetProgress(58, "Kaldirma bilgisi hazirlaniyor...");
                try { File.Copy(Application.ExecutablePath, setupCopy, true); }
                catch { }
                WriteUninstallInfo(installDir, appPath, setupCopy);

                SetProgress(74, "Kisayollar olusturuluyor...");
                if (startMenuCheck.Checked) CreateStartMenuShortcut(appPath);
                if (desktopCheck.Checked) CreateDesktopShortcut(appPath);

                SetProgress(100, "Kurulum tamamlandi.");
                if (runAfterCheck.Checked)
                {
                    try { Process.Start(appPath); }
                    catch { }
                }
                ShowDonePage();
            }
            catch (Exception ex)
            {
                progressLabel.Text = "Kurulum tamamlanamadi: " + ex.Message;
                progressBar.Value = 0;
                MessageBox.Show("Kurulum sirasinda hata olustu:\n\n" + ex.Message, "Kurulum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pageIndex = 1;
                UpdateButtons();
            }
        }

        private void SetProgress(int value, string message)
        {
            progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(progressBar.Maximum, value));
            progressLabel.Text = message;
            progressLabel.Refresh();
            progressBar.Refresh();
        }

        private void ExtractResource(string resourceName, string targetPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream input = assembly.GetManifestResourceStream(resourceName))
            {
                if (input == null)
                    throw new InvalidOperationException(resourceName + " setup icinde bulunamadi.");

                using (FileStream output = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, read);
                }
            }
        }

        private void WriteUninstallInfo(string installDir, string appPath, string setupPath)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AdvancedDiskAnalyzer"))
                {
                    if (key == null) return;
                    key.SetValue("DisplayName", SetupProductName);
                    key.SetValue("DisplayVersion", SetupProductVersion);
                    key.SetValue("Publisher", Publisher);
                    key.SetValue("InstallLocation", installDir);
                    key.SetValue("DisplayIcon", appPath);
                    key.SetValue("UninstallString", "\"" + setupPath + "\" /uninstall");
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        private void CreateStartMenuShortcut(string appPath)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", SetupProductName);
            Directory.CreateDirectory(folder);
            CreateShortcut(Path.Combine(folder, SetupProductName + ".lnk"), appPath, Path.GetDirectoryName(appPath));
        }

        private void CreateDesktopShortcut(string appPath)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            CreateShortcut(Path.Combine(desktop, SetupProductName + ".lnk"), appPath, Path.GetDirectoryName(appPath));
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
                shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath + ",0" });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            catch { }
        }

        private void BrowseInstallPath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Kurulum klasoru secin";
                dialog.SelectedPath = installPathBox.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    installPathBox.Text = Path.Combine(dialog.SelectedPath, InstallFolderName);
            }
        }

        private void Back()
        {
            if (pageIndex == 1) ShowLicensePage();
        }

        private void Next()
        {
            if (pageIndex == 0)
            {
                if (!acceptRadio.Checked)
                {
                    MessageBox.Show("Kuruluma devam etmek icin lisans anlasmasini kabul etmelisiniz.", "Lisans Anlasmasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ShowOptionsPage();
                return;
            }
            if (pageIndex == 1)
            {
                ShowInstallPage();
                return;
            }
            if (pageIndex == 3)
                Close();
        }

        private void CloseSetup()
        {
            if (installCompleted)
            {
                Close();
                return;
            }

            if (MessageBox.Show("Kurulumdan cikmak istiyor musunuz?", "Kurulum", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Close();
        }

        private void ResetContent()
        {
            contentPanel.Controls.Clear();
        }

        private void UpdateButtons()
        {
            backButton.Enabled = pageIndex == 1;
            cancelButton.Enabled = pageIndex != 2;
            if (pageIndex == 0)
            {
                nextButton.Text = "Ileri >";
                nextButton.Enabled = acceptRadio != null && acceptRadio.Checked;
            }
            else if (pageIndex == 1)
            {
                nextButton.Text = "Kur";
                nextButton.Enabled = true;
            }
            else if (pageIndex == 2)
            {
                nextButton.Text = "Kuruluyor";
                nextButton.Enabled = false;
                backButton.Enabled = false;
            }
            else
            {
                nextButton.Text = "Bitir";
                nextButton.Enabled = true;
                backButton.Enabled = false;
                cancelButton.Enabled = false;
            }
        }

        private string DefaultInstallPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", InstallFolderName);
        }

        private string BuildLicenseText()
        {
            return
@"ADVANCED DISK ANALYZER LICENSE AGREEMENT

Version: " + SetupProductVersion + @"
Date: 2026-05-10

Please read this agreement before installing or using Advanced Disk Analyzer.
By selecting ""Anlasmayi kabul ediyorum"" and continuing the setup, you accept
the terms below.

1. Offline and privacy guarantee
This software is designed to run locally on the user's Windows computer.
File names, file sizes, folder paths, scan results, and file contents are not
sent to external servers by the desktop analyzer.

2. License scope
The Free edition may be used for personal evaluation and basic local disk
analysis. Pro and Enterprise features require a valid license key. License
features can include PDF reports, network drive scanning, bulk delete tools,
and enterprise customization.

3. User responsibility
Disk cleanup decisions are made by the user. The software can recommend files
that may be safe to remove, but the user must review results before deletion.
The developer is not responsible for accidental deletion, data loss, indirect
damage, business interruption, or losses caused by incorrect use.

4. No warranty
The software is provided ""as is"" without warranties of merchantability,
fitness for a particular purpose, uninterrupted operation, or error-free
results. Storage devices, network drives, permissions, and antivirus products
can affect scan speed and accuracy.

5. Enterprise use
Organizations should test the application in their own environment before
wide deployment. Enterprise administrators are responsible for license
distribution, internal policy compliance, backups, and user permissions.

6. Updates and support
Beta versions may change behavior before final release. Future updates may
include additional license checks, purchase flows, installer improvements, or
enterprise deployment tooling.

Advanced Disk Analyzer
Copyright 2026
";
        }
    }

    public class UninstallForm : Form
    {
        private const string SetupProductName = "Advanced Disk Analyzer";
        private Button uninstallButton;
        private Button cancelButton;

        public UninstallForm()
        {
            this.Text = SetupProductName + " - Kaldir";
            this.Size = new Size(520, 240);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Icon = InstallerIconFactory.GetIcon();

            Label title = new Label();
            title.Text = SetupProductName + " kaldirilsin mi?";
            title.Location = new Point(28, 26);
            title.Size = new Size(450, 28);
            title.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            this.Controls.Add(title);

            Label note = new Label();
            note.Text = "Uygulama dosyalari ve kisayollar kaldirilir. Lisans ve kullanici verileri korunur.";
            note.Location = new Point(30, 70);
            note.Size = new Size(440, 48);
            note.Font = new Font("Segoe UI", 9);
            this.Controls.Add(note);

            uninstallButton = new Button();
            uninstallButton.Text = "Kaldir";
            uninstallButton.Location = new Point(244, 142);
            uninstallButton.Size = new Size(110, 34);
            uninstallButton.Click += delegate { Uninstall(); };
            this.Controls.Add(uninstallButton);

            cancelButton = new Button();
            cancelButton.Text = "Iptal";
            cancelButton.Location = new Point(366, 142);
            cancelButton.Size = new Size(110, 34);
            cancelButton.Click += delegate { Close(); };
            this.Controls.Add(cancelButton);
        }

        private void Uninstall()
        {
            try
            {
                string installDir = GetInstallLocation();
                DeleteShortcuts();
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AdvancedDiskAnalyzer", false); }
                catch { }

                if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
                {
                    foreach (string file in Directory.GetFiles(installDir))
                    {
                        try { File.Delete(file); }
                        catch { }
                    }
                }

                MessageBox.Show("Kaldirma islemi tamamlandi.", "Kaldir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaldirma sirasinda hata olustu:\n\n" + ex.Message, "Kaldir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetInstallLocation()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AdvancedDiskAnalyzer"))
                {
                    if (key == null) return "";
                    return (key.GetValue("InstallLocation") as string) ?? "";
                }
            }
            catch { return ""; }
        }

        private void DeleteShortcuts()
        {
            try
            {
                string desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), SetupProductName + ".lnk");
                if (File.Exists(desktop)) File.Delete(desktop);
            }
            catch { }

            try
            {
                string startFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", SetupProductName);
                string shortcut = Path.Combine(startFolder, SetupProductName + ".lnk");
                if (File.Exists(shortcut)) File.Delete(shortcut);
                if (Directory.Exists(startFolder) && Directory.GetFiles(startFolder).Length == 0)
                    Directory.Delete(startFolder);
            }
            catch { }
        }
    }

    public class InstallArtPanel : Panel
    {
        public InstallArtPanel()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(18, 24, 34));
            using (Pen p = new Pen(Color.FromArgb(47, 129, 247), 2))
            using (SolidBrush b = new SolidBrush(Color.White))
            {
                Rectangle monitor = new Rectangle(16, 14, 40, 30);
                g.FillRectangle(b, monitor);
                g.DrawRectangle(p, monitor);
                g.DrawLine(p, 26, 48, 46, 48);
                g.DrawLine(p, 36, 44, 36, 52);
                g.DrawArc(p, 14, 34, 40, 24, 20, 230);
            }
        }
    }
}
