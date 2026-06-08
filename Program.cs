// ═══════════════════════════════════════════════════════════════════════════════
// ADVANCED DISK ANALYZER v2.0
// ═══════════════════════════════════════════════════════════════════════════════
// Dosya    : Program.cs (~7800 satır)
// Amaç     : Windows masaüstü disk analiz uygulamasının tüm kaynak kodu.
//            Tarama motorları, görselleştirme (pasta grafik, treemap), skor
//            bazlı analiz, lisans yönetimi ve kullanıcı arayüzünü içerir.
// Derleme  : .NET Framework 4.0, C# 5 (csc.exe ile tek dosya derleme)
// Bağımlılık: Harici kütüphane yok — tüm bileşenler bu dosyada tanımlı.
//            System.Windows.Forms, System.Drawing, System.Management referansları.
// Mimari   : Tek dosyalı monolitik yapı. Sınıflar mantıksal bölümlere ayrılmış:
//            Theme → MainForm → Tarama → Görselleştirme → Lisans → Veri Modelleri
// ═══════════════════════════════════════════════════════════════════════════════

// ── Dış Bağımlılıklar ──────────────────────────────────────────────────────────
// Standart .NET Framework 4.0 kütüphaneleri. Üçüncü parti paket kullanılmaz.
// System.Management: WMI sorguları ile SSD/HDD tespiti ve ağ sürücüsü bilgisi
// System.Runtime.InteropServices: Win32 API P/Invoke çağrıları (FindFirstFile vb.)
// System.Security.Cryptography: RSA lisans imzalama ve PBKDF2 parola hash
// ────────────────────────────────────────────────────────────────────────────────
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Globalization;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Concurrent;

namespace AdvancedDiskAnalyzer
{
    // ═══════════════════════════════════════════════════════════════════
    // BÖLÜM: TEMA SİSTEMİ (Theme + DarkMenuColorTable)
    // ═══════════════════════════════════════════════════════════════════
    // Amacı  : Koyu ve açık tema renk paletlerini merkezi olarak yönetir.
    //          Tüm UI bileşenleri renk değerlerini bu sınıftan alır.
    // Yöntemi: IsDark bayrağına göre koşullu property'ler (getter)
    //          ilgili paletten renk döndürür. Tema değişiminde tüm
    //          kontroller ApplyTheme() ile yeniden boyanır.
    // Notlar : Renk paleti el ile seçilmiş HSL değerlerinden oluşur.
    //          Arkaplan → Yüzey → Kart → Kenarlık → Metin şeklinde
    //          katmanlı hiyerarşi uygulanır (elevation pattern).
    // ═══════════════════════════════════════════════════════════════════
    public static class Theme
    {
        public static bool IsDark = true;  // Varsayılan: koyu tema

        public static Color DarkBg        = Color.FromArgb(12, 16, 23);
        public static Color DarkSurface   = Color.FromArgb(20, 25, 33);
        public static Color DarkCard      = Color.FromArgb(25, 31, 40);
        public static Color DarkBorder    = Color.FromArgb(48, 58, 72);
        public static Color DarkText      = Color.FromArgb(230, 236, 248);
        public static Color DarkSubText   = Color.FromArgb(145, 154, 170);
        public static Color DarkAccent    = Color.FromArgb(47, 129, 247);
        public static Color DarkAccent2   = Color.FromArgb(255, 122, 38);
        public static Color DarkSuccess   = Color.FromArgb(70, 222, 156);
        public static Color DarkDanger    = Color.FromArgb(255, 92, 110);
        public static Color DarkWarning   = Color.FromArgb(245, 181, 63);
        public static Color DarkHighlight = Color.FromArgb(30, 43, 63);

        public static Color LightBg        = Color.FromArgb(246, 247, 250);
        public static Color LightSurface   = Color.FromArgb(255, 255, 255);
        public static Color LightCard      = Color.FromArgb(240, 243, 248);
        public static Color LightBorder    = Color.FromArgb(210, 216, 226);
        public static Color LightText      = Color.FromArgb(18, 18, 38);
        public static Color LightSubText   = Color.FromArgb(70, 80, 95);      // Daha koyu, yüksek kontrastlı alt metin rengi
        public static Color LightAccent    = Color.FromArgb(20, 85, 170);     // Daha zengin ve okunabilir mavi accent
        public static Color LightAccent2   = Color.FromArgb(64, 78, 104);     // Daha koyu ikincil mavi/gri
        public static Color LightSuccess   = Color.FromArgb(16, 124, 65);     // Beyaz zemin üzerinde yüksek kontrastlı yeşil (AA standartlarına uygun)
        public static Color LightDanger    = Color.FromArgb(186, 12, 47);     // Beyaz zemin üzerinde yüksek kontrastlı kırmızı (AA standartlarına uygun)
        public static Color LightWarning   = Color.FromArgb(150, 80, 0);      // Beyaz zemin üzerinde yüksek kontrastlı turuncu/kahverengi (AA standartlarına uygun)
        public static Color LightHighlight = Color.FromArgb(208, 222, 255);

        public static Color Bg        { get { return IsDark ? DarkBg        : LightBg;        } }
        public static Color Surface   { get { return IsDark ? DarkSurface   : LightSurface;   } }
        public static Color Card      { get { return IsDark ? DarkCard      : LightCard;      } }
        public static Color Border    { get { return IsDark ? DarkBorder    : LightBorder;    } }
        public static Color Text      { get { return IsDark ? DarkText      : LightText;      } }
        public static Color SubText   { get { return IsDark ? DarkSubText   : LightSubText;   } }
        public static Color Accent    { get { return IsDark ? DarkAccent    : LightAccent;    } }
        public static Color Accent2   { get { return IsDark ? DarkAccent2   : LightAccent2;   } }
        public static Color Success   { get { return IsDark ? DarkSuccess   : LightSuccess;   } }
        public static Color Danger    { get { return IsDark ? DarkDanger    : LightDanger;    } }
        public static Color Warning   { get { return IsDark ? DarkWarning   : LightWarning;   } }
        public static Color Highlight { get { return IsDark ? DarkHighlight : LightHighlight; } }
    }

    public class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color MenuItemBorder { get { return Theme.Accent; } }
        public override Color MenuItemSelected { get { return Theme.Highlight; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.Highlight; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.Highlight; } }
        public override Color MenuItemPressedGradientBegin { get { return Theme.Highlight; } }
        public override Color MenuItemPressedGradientEnd { get { return Theme.Highlight; } }
        public override Color ImageMarginGradientBegin { get { return Theme.Card; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Card; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Card; } }
        public override Color SeparatorDark { get { return Theme.Border; } }
        public override Color SeparatorLight { get { return Theme.Border; } }
        public override Color ToolStripBorder { get { return Theme.Border; } }
        public override Color ToolStripGradientBegin { get { return Theme.Surface; } }
        public override Color ToolStripGradientMiddle { get { return Theme.Surface; } }
        public override Color ToolStripGradientEnd { get { return Theme.Surface; } }
    }

    // ═══════════════════════════════════════════════════════════════════
    // BÖLÜM: UYGULAMA İKONU FABRİKASI
    // ═══════════════════════════════════════════════════════════════════
    // Amacı  : EXE'den gömülü ikonu çıkartır ve önbelleğe alır.
    // Yöntemi: Icon.ExtractAssociatedIcon ile çalışan EXE'nin ikonunu
    //          okur. Başarısız olursa SystemIcons.Application kullanır.
    //          Sonuç cachedIcon'da tutulur — tekrar disk I/O yapılmaz.
    // ═══════════════════════════════════════════════════════════════════
    public static class AppIconFactory
    {
        private static Icon cachedIcon;

        public static Icon GetAppIcon()
        {
            if (cachedIcon != null) return cachedIcon;
            try
            {
                Icon extracted = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (extracted != null)
                {
                    cachedIcon = extracted;
                    return cachedIcon;
                }
            }
            catch { }

            cachedIcon = SystemIcons.Application;
            return cachedIcon;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // BÖLÜM: ANA FORM (MainForm)
    // ═══════════════════════════════════════════════════════════════════
    // Amacı  : Uygulamanın tek ana penceresi. Tüm kullanıcı etkileşimi,
    //          tarama kontrolü, görselleştirme ve veri yönetimi buradadır.
    // İçerik : ~4000 satır — UI oluşturma, tarama motoru, canlı liste,
    //          pasta grafik, treemap, skor analizi, CSV/PDF dışa aktarım,
    //          tema yönetimi, lisans kontrolü, sağ tık menüsü.
    // Yapı   : Metotlar mantıksal bölüm başlıkları ile gruplanmıştır.
    // ═══════════════════════════════════════════════════════════════════
    public class MainForm : Form
    {
        // ── Tarama Modu Seçenekleri ──
        // NtfsTurbo : Ham MFT okuma (en hızlı, yönetici yetkisi gerekir)
        // FastWinApi: Win32 FindFirstFile ile paralel tarama
        // Normal    : Sıralı tarama, tam canlı liste gösterimi
        private enum ScanMode
        {
            NtfsTurbo,
            FastWinApi,
            Normal
        }

        // ── UI Kontrolleri ──────────────────────────────────────────────
        // Ana pencere düzeni: Sol panel (TreeView) | Orta (ListView) | Sağ (TabControl)
        // Toolbar üstte, durum çubuğu altta, ilerleme çubuğu en altta
        private TreeView treeView;
        private ListView listView;
        private Panel treeHost;
        private Panel listHost;
        private Panel listBodyPanel;
        private ModernScrollBar treeScrollBar;
        private ModernScrollBar listScrollBar;
        private ModernScrollBar aiScrollBar;
        private Panel listNativeVerticalMask;
        private Panel listNativeHorizontalMask;
        private MenuStrip mainMenu;
        private ToolStripMenuItem ultraFastScanMenuItem;
        private ToolStripMenuItem ntfsTurboMenuItem;
        private ToolStripMenuItem allocatedSizeMenuItem;
        private ToolStripMenuItem hardLinkAccuracyMenuItem;
        private Panel toolbarPanel;
        private Panel piePanel;
        private Panel treemapPanel;
        private Panel aiHost;
        private Panel aiPanel;
        private TabControl tabControl;
        private ProgressBar progressBar;
        private Button scanButton;
        private Button scanOptionsButton;
        private ContextMenuStrip scanModeMenu;
        private Button themeButton;
        private Label filterLabel;
        private TextBox filterBox;
        private Button topFilesButton;
        private Button duplicatesButton;
        private Button csvButton;
        private Button accountButton;
        private Button licenseButton;
        private Label licenseLabel;
        private Label statusLabel;
        private Label liveCountLabel;
        private Label sidebarLicenseLabel;
        private Panel sidebarHeaderPanel;
        private Panel sidebarNavPanel;
        private Panel sidebarFooterPanel;
        private TableLayoutPanel dashboardStripPanel;
        private Label summaryTotalLabel;
        private Label summaryFilesLabel;
        private Label summaryTimeLabel;
        private Label summaryPathLabel;
        private ContextMenuStrip listContextMenu;
        private Panel statsBar;
        private Label statTotal, statFiles, statTime;

        // Ağ sürücüsü bilgisi
        private bool isNetworkDrive = false;
        private string networkDriveInfo = "";

        // Ağ sürücüsü bilgi banner'ı
        private Panel networkBanner;
        private Label networkBannerLabel;

        // ── Tarama Motoru Durum Değişkenleri ─────────────────────────────
        // scoringModel    : Dosya gereksizlik skorunu hesaplayan ağırlıklı formül
        // isSSD           : WMI ile tespit edilen sürücü tipi (parallelism ayarı için)
        // driveTypeCache  : Sürücü harfi → SSD/HDD eşlemesi (tekrar WMI sorgusu önlenir)
        // clusterSizeCache: Sürücü kökü → cluster boyutu (GetDiskFreeSpace sonucu)
        // countedHardLinks: Hard link FileID takibi — aynı dosyanın çift sayılmasını önler
        private AdaptiveScoringModel scoringModel = new AdaptiveScoringModel();
        private bool isSSD = false;
        private Dictionary<string, bool> driveTypeCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, long> clusterSizeCache = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private object clusterSizeLock = new object();
        private ConcurrentDictionary<string, byte> countedHardLinks = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private DirectoryNode rootNode = null;
        private int totalFilesFound = 0;
        private long totalBytesScanned = 0;
        private long totalAllocatedScanned = 0;
        private int duplicateHardLinksSkipped = 0;
        private System.Windows.Forms.Timer uiTimer;
        private System.Windows.Forms.Timer filterTimer;

        private readonly object queueLock = new object();
        private Queue<FileNode> pendingFiles = new Queue<FileNode>();
        private int liveQueued = 0;
        private volatile bool liveListLimited = false;
        private int liveUiTicks = 0;

        private int hoveredSlice = -1;
        private List<KeyValuePair<string, long>> currentPieSlices = new List<KeyValuePair<string, long>>();
        private List<float[]> sliceAngles = new List<float[]>();
        private int hoveredTreemapTile = -1;
        private List<TreemapTile> treemapTiles = new List<TreemapTile>();
        private DirectoryNode currentVisualNode = null;
        private DirectoryNode currentSelectedDirectory = null;
        private List<FileNode> currentSelectionFiles = new List<FileNode>();
        private int treeSelectionVersion = 0;
        private int sortColumn = -1;
        private bool sortAscending = true;
        private string selectedScanPath = "";
        private bool suppressFilterTextChanged = false;
        private LicenseState currentLicense;
        private CancellationTokenSource scanCancelSource = null;
        private bool scanInProgress = false;
        private ScanMode selectedScanMode = ScanMode.NtfsTurbo;
        private bool ultraFastMode = true;
        private bool ntfsTurboMode = true;
        private volatile string lastScanEngine = "WinAPI";
        private volatile string lastTurboMessage = "";
        private DateTime scoreOldFileThreshold = DateTime.Now.AddYears(-1);
        private bool allocatedSizeEnabled = true;
        private bool hardLinkAccuracyEnabled = true;

        private const string AppVersion = "v2.0";
        private const string EulaRegistryPath = @"Software\AdvancedDiskAnalyzer";
        private const string EulaStampDate = "2026-05-07";
        private const string PurchaseUrl = "https://advanced-disk-analyzer.com/pricing";
        private const string EnterpriseContactUrl = "mailto:sales@advanced-disk-analyzer.com?subject=Advanced%20Disk%20Analyzer%20Enterprise";
        private const int LiveListItemLimit = 4000;
        private const int UltraLiveListItemLimit = 1600;
        private const int DisplayFileLimit = 10000;
        private const long UltraFastHardLinkMinBytes = 16L * 1024L * 1024L;
        private const int TurboCompactRecordThreshold = 300000;
        private const long TurboMaterializeMinBytes = 16L * 1024L * 1024L;
        private readonly string[] listColumnTitles = new string[]
        {
            "Dosya Adı", "Boyut", "Diskte", "Skor", "Tarih", "Tür", "Konum"
        };

        public MainForm()
        {
            currentLicense = LicenseManager.Load();
            InitializeUI();
            RefreshLicenseUi();
            ApplyTheme();
            this.Shown += MainForm_Shown;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (scanCancelSource != null)
                    scanCancelSource.Cancel();
            }
            catch { }
        }

        private void LayoutToolbar()
        {
            if (toolbarPanel == null || filterBox == null || accountButton == null) return;

            int right = Math.Max(960, toolbarPanel.ClientSize.Width) - 24;
            int y = 18;

            licenseLabel.Width = 150;
            licenseLabel.Left = right - licenseLabel.Width;
            licenseLabel.Top = 27;

            licenseButton.Left = licenseLabel.Left - licenseButton.Width - 10;
            licenseButton.Top = y;
            accountButton.Left = licenseButton.Left - accountButton.Width - 10;
            accountButton.Top = y;
            themeButton.Left = accountButton.Left - themeButton.Width - 26;
            themeButton.Top = y;
            csvButton.Left = themeButton.Left - csvButton.Width - 10;
            csvButton.Top = y;
            duplicatesButton.Left = csvButton.Left - duplicatesButton.Width - 10;
            duplicatesButton.Top = y;
            topFilesButton.Left = duplicatesButton.Left - topFilesButton.Width - 10;
            topFilesButton.Top = y;

            filterLabel.Left = 24;
            filterLabel.Top = 26;
            filterBox.Left = 82;
            filterBox.Top = 20;
            filterBox.Width = Math.Max(180, topFilesButton.Left - filterBox.Left - 20);

            statusLabel.Left = 28;
            statusLabel.Top = 50;
            liveCountLabel.Left = Math.Max(520, accountButton.Left);
            liveCountLabel.Top = 50;
            liveCountLabel.Width = Math.Max(160, right - liveCountLabel.Left);
            statusLabel.Width = Math.Max(240, liveCountLabel.Left - statusLabel.Left - 18);
        }

        private MenuStrip BuildMainMenu()
        {
            MenuStrip menu = new MenuStrip();
            menu.Dock = DockStyle.Top;
            menu.Padding = new Padding(8, 2, 0, 2);
            menu.Font = new Font("Segoe UI", 9);
            menu.RenderMode = ToolStripRenderMode.Professional;
            menu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());

            ToolStripMenuItem file = MenuRoot("Dosya");
            file.DropDownItems.Add(MenuCommand("Klasör Tara...", ScanButton_Click, Keys.Control | Keys.O));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(MenuCommand("CSV Dışarı Aktar", CsvButton_Click, Keys.Control | Keys.E));
            file.DropDownItems.Add(MenuCommand("PDF Rapor Al", PdfReportBtn_Click, Keys.Control | Keys.P));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(MenuCommand("Çıkış", delegate { this.Close(); }, Keys.Alt | Keys.F4));

            ToolStripMenuItem view = MenuRoot("Görünüm");
            view.DropDownItems.Add(MenuCommand("Grafik Paneli", delegate { tabControl.SelectedIndex = 0; }, Keys.Control | Keys.D1));
            view.DropDownItems.Add(MenuCommand("Treemap Paneli", delegate { tabControl.SelectedIndex = 1; }, Keys.Control | Keys.D2));
            view.DropDownItems.Add(MenuCommand("AI Öneriler", delegate { tabControl.SelectedIndex = 2; }, Keys.Control | Keys.D3));
            view.DropDownItems.Add(new ToolStripSeparator());
            view.DropDownItems.Add(MenuCommand("Temayı Değiştir", ThemeButton_Click, Keys.Control | Keys.T));

            ToolStripMenuItem tools = MenuRoot("Araçlar");
            tools.DropDownItems.Add(MenuCommand("En Büyük 100 Dosya", TopFilesButton_Click, Keys.F6));
            tools.DropDownItems.Add(MenuCommand("Kopyaları Bul", DuplicatesButton_Click, Keys.F7));
            tools.DropDownItems.Add(MenuCommand("Listeyi Yenile", delegate { RefreshCurrentView(); }, Keys.F5));
            tools.DropDownItems.Add(new ToolStripSeparator());
            ultraFastScanMenuItem = MenuCommand("Ultra Hızlı Tarama", delegate { }, Keys.Control | Keys.U);
            ultraFastScanMenuItem.CheckOnClick = true;
            ultraFastScanMenuItem.Checked = ultraFastMode;
            ultraFastScanMenuItem.CheckedChanged += ToggleUltraFastScan_Click;
            tools.DropDownItems.Add(ultraFastScanMenuItem);
            ntfsTurboMenuItem = MenuCommand("NTFS Turbo (MFT Beta)", delegate { }, Keys.Control | Keys.M);
            ntfsTurboMenuItem.CheckOnClick = true;
            ntfsTurboMenuItem.Checked = ntfsTurboMode;
            ntfsTurboMenuItem.CheckedChanged += ToggleNtfsTurbo_Click;
            tools.DropDownItems.Add(ntfsTurboMenuItem);
            tools.DropDownItems.Add(MenuCommand("Yönetici Olarak Yeniden Başlat", RestartAsAdmin_Click, Keys.Control | Keys.Shift | Keys.A));
            allocatedSizeMenuItem = MenuCommand("Diskte Kaplanan Alanı Ölç", delegate { }, Keys.None);
            allocatedSizeMenuItem.CheckOnClick = true;
            allocatedSizeMenuItem.Checked = allocatedSizeEnabled;
            allocatedSizeMenuItem.CheckedChanged += ToggleAllocatedSize_Click;
            tools.DropDownItems.Add(allocatedSizeMenuItem);
            hardLinkAccuracyMenuItem = MenuCommand("Hard Link Çift Sayımı Önle", delegate { }, Keys.None);
            hardLinkAccuracyMenuItem.CheckOnClick = true;
            hardLinkAccuracyMenuItem.Checked = hardLinkAccuracyEnabled;
            hardLinkAccuracyMenuItem.CheckedChanged += ToggleHardLinkAccuracy_Click;
            tools.DropDownItems.Add(hardLinkAccuracyMenuItem);
            tools.DropDownItems.Add(new ToolStripSeparator());
            tools.DropDownItems.Add(MenuCommand("Tarama Geçmişini Aç", OpenScanHistory_Click, Keys.Control | Keys.G));
            tools.DropDownItems.Add(MenuCommand("Snapshot Geçmişini Aç", OpenSnapshotHistory_Click, Keys.Control | Keys.Shift | Keys.G));

            ToolStripMenuItem license = MenuRoot("Lisans");
            license.DropDownItems.Add(MenuCommand("Hesap...", AccountButton_Click, Keys.Control | Keys.H));
            license.DropDownItems.Add(MenuCommand("Plan ve Aktivasyon...", LicenseButton_Click, Keys.Control | Keys.L));

            ToolStripMenuItem help = MenuRoot("Yardım");
            help.DropDownItems.Add(MenuCommand("Gizlilik / EULA", delegate { ShowPrivacyInfo(); }, Keys.F1));
            help.DropDownItems.Add(MenuCommand("Kurumsal Satış", delegate { OpenExternalUrl(EnterpriseContactUrl); }, Keys.None));

            menu.Items.Add(file);
            menu.Items.Add(view);
            menu.Items.Add(tools);
            menu.Items.Add(license);
            menu.Items.Add(help);
            return menu;
        }

        private void ToggleUltraFastScan_Click(object sender, EventArgs e)
        {
            ultraFastMode = ultraFastScanMenuItem == null || ultraFastScanMenuItem.Checked;
            if (!ntfsTurboMode)
                selectedScanMode = ultraFastMode ? ScanMode.FastWinApi : ScanMode.Normal;
            UpdateScanModeMenuChecks();
            statusLabel.Text = ultraFastMode
                ? "Ultra hızlı tarama açık: UI akışı sınırlı, hard link kontrolü büyük dosyalarda adaptif."
                : "Standart tarama açık: daha fazla canlı liste gösterilir.";
        }

        private void ToggleNtfsTurbo_Click(object sender, EventArgs e)
        {
            ntfsTurboMode = ntfsTurboMenuItem == null || ntfsTurboMenuItem.Checked;
            selectedScanMode = ntfsTurboMode ? ScanMode.NtfsTurbo : (ultraFastMode ? ScanMode.FastWinApi : ScanMode.Normal);
            UpdateScanModeMenuChecks();
            statusLabel.Text = ntfsTurboMode
                ? "NTFS Turbo açık: yerel NTFS disklerde ham MFT okunur, destek yoksa güvenli taramaya düşer."
                : "NTFS Turbo kapalı: güvenli WinAPI tarama kullanılır.";
        }

        private void RestartAsAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsRunningAsAdministrator())
                {
                    MessageBox.Show("Uygulama zaten yönetici yetkisiyle çalışıyor.", "Yönetici Yetkisi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ProcessStartInfo info = new ProcessStartInfo(Application.ExecutablePath);
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yönetici olarak yeniden başlatılamadı:\n" + ex.Message,
                    "Yönetici Yetkisi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        private bool ShouldOfferAdminRestartForTurbo(string path)
        {
            if (!ntfsTurboMode || isNetworkDrive || IsRunningAsAdministrator())
                return false;
            try
            {
                string root = PathText.GetRoot(path);
                if (string.IsNullOrEmpty(root) || root.Length < 2 || root[1] != ':')
                    return false;
                DriveInfo drive = new DriveInfo(root);
                return string.Equals(drive.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void ToggleAllocatedSize_Click(object sender, EventArgs e)
        {
            allocatedSizeEnabled = allocatedSizeMenuItem == null || allocatedSizeMenuItem.Checked;
            statusLabel.Text = allocatedSizeEnabled
                ? "Diskte kaplanan alan hesabı açık."
                : "Diskte alan hesabı kapalı: tarama daha sade ve hızlı çalışır.";
        }

        private void ToggleHardLinkAccuracy_Click(object sender, EventArgs e)
        {
            hardLinkAccuracyEnabled = hardLinkAccuracyMenuItem == null || hardLinkAccuracyMenuItem.Checked;
            statusLabel.Text = hardLinkAccuracyEnabled
                ? "Hard link çift sayım koruması açık."
                : "Hard link koruması kapalı: en hızlı ham listeleme yapılır.";
        }

        private ToolStripMenuItem MenuRoot(string text)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.ForeColor = Theme.Text;
            item.BackColor = Theme.Surface;
            return item;
        }

        private ToolStripMenuItem MenuCommand(string text, EventHandler handler, Keys shortcut)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.ForeColor = Theme.Text;
            item.BackColor = Theme.Card;
            item.ShortcutKeys = shortcut;
            item.ShowShortcutKeys = shortcut != Keys.None;
            item.Click += handler;
            return item;
        }

        private ContextMenuStrip BuildScanModeMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.RenderMode = ToolStripRenderMode.Professional;
            menu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
            menu.Items.Add(CreateScanModeItem("NTFS Turbo Tarama (en hızlı)", ScanMode.NtfsTurbo));
            menu.Items.Add(CreateScanModeItem("Hızlı Tarama (yönetici gerekmez)", ScanMode.FastWinApi));
            menu.Items.Add(CreateScanModeItem("Normal Tarama (tam liste)", ScanMode.Normal));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(MenuCommand("Yönetici Olarak Yeniden Başlat", RestartAsAdmin_Click, Keys.None));
            menu.Opening += delegate { UpdateScanModeMenuChecks(); };
            return menu;
        }

        private ToolStripMenuItem CreateScanModeItem(string text, ScanMode mode)
        {
            ToolStripMenuItem item = MenuCommand(text, ScanModeMenuItem_Click, Keys.None);
            item.Tag = mode;
            item.CheckOnClick = false;
            return item;
        }

        private void ScanOptionsButton_Click(object sender, EventArgs e)
        {
            if (scanModeMenu == null || scanOptionsButton == null || scanInProgress) return;
            UpdateScanModeMenuChecks();
            scanModeMenu.Show(scanOptionsButton, new Point(0, scanOptionsButton.Height));
        }

        private void ScanModeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null || !(item.Tag is ScanMode)) return;
            selectedScanMode = (ScanMode)item.Tag;
            ApplySelectedScanMode();
            UpdateScanModeMenuChecks();
            if (!scanInProgress)
                ScanButton_Click(scanButton, EventArgs.Empty);
        }

        private void UpdateScanModeMenuChecks()
        {
            if (scanModeMenu == null) return;
            foreach (ToolStripItem raw in scanModeMenu.Items)
            {
                ToolStripMenuItem item = raw as ToolStripMenuItem;
                if (item == null || !(item.Tag is ScanMode)) continue;
                item.Checked = (ScanMode)item.Tag == selectedScanMode;
                item.BackColor = item.Checked ? Theme.Highlight : Theme.Card;
                item.ForeColor = item.Checked ? Theme.Text : Theme.SubText;
            }
        }

        private void ApplySelectedScanMode()
        {
            if (selectedScanMode == ScanMode.NtfsTurbo)
            {
                ntfsTurboMode = true;
                ultraFastMode = true;
            }
            else if (selectedScanMode == ScanMode.FastWinApi)
            {
                ntfsTurboMode = false;
                ultraFastMode = true;
            }
            else
            {
                ntfsTurboMode = false;
                ultraFastMode = false;
            }

            SyncAdvancedScanToggles();
            if (!scanInProgress && statusLabel != null)
                statusLabel.Text = "Tarama modu: " + GetScanModeLabel(selectedScanMode);
        }

        private void SyncAdvancedScanToggles()
        {
            if (ultraFastScanMenuItem != null && ultraFastScanMenuItem.Checked != ultraFastMode)
                ultraFastScanMenuItem.Checked = ultraFastMode;
            if (ntfsTurboMenuItem != null && ntfsTurboMenuItem.Checked != ntfsTurboMode)
                ntfsTurboMenuItem.Checked = ntfsTurboMode;
        }

        private string GetScanModeLabel(ScanMode mode)
        {
            if (mode == ScanMode.NtfsTurbo) return "NTFS Turbo (en hızlı)";
            if (mode == ScanMode.FastWinApi) return "Hızlı Tarama (WinAPI)";
            return "Normal Tarama";
        }

        private void ApplyMenuTheme()
        {
            if (mainMenu == null) return;
            mainMenu.BackColor = Theme.Surface;
            mainMenu.ForeColor = Theme.Text;
            mainMenu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
            foreach (ToolStripItem item in mainMenu.Items)
                ThemeMenuItem(item);
        }

        private void ThemeMenuItem(ToolStripItem item)
        {
            item.BackColor = Theme.Surface;
            item.ForeColor = Theme.Text;
            ToolStripMenuItem menuItem = item as ToolStripMenuItem;
            if (menuItem == null) return;
            foreach (ToolStripItem child in menuItem.DropDownItems)
            {
                child.BackColor = Theme.Card;
                child.ForeColor = Theme.Text;
                ThemeMenuItem(child);
            }
        }

        private void ApplyScanModeMenuTheme()
        {
            if (scanModeMenu == null) return;
            scanModeMenu.BackColor = Theme.Card;
            scanModeMenu.ForeColor = Theme.Text;
            scanModeMenu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
            foreach (ToolStripItem item in scanModeMenu.Items)
            {
                item.BackColor = Theme.Card;
                item.ForeColor = Theme.Text;
            }
            UpdateScanModeMenuChecks();
        }

        private async void RefreshCurrentView()
        {
            if (currentSelectedDirectory == null)
            {
                if (rootNode == null) return;
                currentSelectedDirectory = rootNode;
            }
            int version = Interlocked.Increment(ref treeSelectionVersion);
            DirectoryNode node = currentSelectedDirectory;
            statusLabel.Text = "Görünüm yenileniyor...";
            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                List<FileNode> result = new List<FileNode>();
                CollectAllFiles(node, result);
                return result.OrderByDescending(f => f.Score).ThenByDescending(f => f.Size).ToList();
            });
            if (version != treeSelectionVersion || this.IsDisposed) return;
            currentSelectionFiles = files;
            await PopulateListViewAsync(files, version);
            statusLabel.Text = string.Format("{0:N0} dosya listelendi", files.Count);
        }

        private void ShowPrivacyInfo()
        {
            using (PrivacyConsentForm form = new PrivacyConsentForm(AppVersion, EulaStampDate,
                LicenseManager.GetEnterpriseEulaText(currentLicense)))
                form.ShowDialog(this);
        }

        private void OpenExternalUrl(string url)
        {
            try { Process.Start(url); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Bağlantı", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: KULLANICI ARAYÜZÜ OLUŞTURMA (InitializeUI)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Tüm WinForms kontrollerini oluşturur ve konumlandırır.
        // Yapı   : Sol panel (sidebar: logo, navigasyon, tarama butonu)
        //          + Orta alan (dashboard kartları + ListView dosya listesi)
        //          + Sağ panel (TabControl: Grafik / Treemap / Analiz)
        //          + Üst menü çubuğu + alt durum çubuğu + ilerleme çubuğu
        // Notlar : Owner-draw ListView, özel ScrollBar ve MetricCard
        //          kontrollerinden oluşan modern arayüz. Her kontrol
        //          Theme sınıfından renk alır, tema değişiminde güncellenir.
        // ═══════════════════════════════════════════════════════════════════
        private void InitializeUI()
        {
            this.Text = "Advanced Disk Analyzer " + AppVersion;
            this.Size = new Size(1680, 980);
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.Icon = AppIconFactory.GetAppIcon();

            mainMenu = BuildMainMenu();
            this.MainMenuStrip = mainMenu;

            toolbarPanel = new Panel();
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 72;
            toolbarPanel.Padding = new Padding(24, 0, 24, 0);
            toolbarPanel.Resize += delegate { LayoutToolbar(); };

            Label titleLabel = new Label();
            titleLabel.Text = "Disk Analiz";
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.Location = new Point(22, 22);
            titleLabel.AutoSize = true;
            titleLabel.Tag = "brand-title";
            toolbarPanel.Controls.Add(titleLabel);

            Label versionLabel = new Label();
            versionLabel.Text = AppVersion;
            versionLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            versionLabel.Location = new Point(22, 48);
            versionLabel.AutoSize = true;
            versionLabel.Tag = "brand-version";
            toolbarPanel.Controls.Add(versionLabel);

            scanButton = new Button();
            scanButton.Text = "Yeni Tarama";
            scanButton.Location = new Point(18, 14);
            scanButton.Size = new Size(254, 42);
            scanButton.FlatStyle = FlatStyle.Flat;
            scanButton.FlatAppearance.BorderSize = 0;
            scanButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            scanButton.Cursor = Cursors.Hand;
            scanButton.Click += ScanButton_Click;
            toolbarPanel.Controls.Add(scanButton);

            scanOptionsButton = new Button();
            scanOptionsButton.Text = "▼";
            scanOptionsButton.Size = new Size(40, 42);
            scanOptionsButton.FlatStyle = FlatStyle.Flat;
            scanOptionsButton.FlatAppearance.BorderSize = 0;
            scanOptionsButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            scanOptionsButton.Cursor = Cursors.Hand;
            scanOptionsButton.Click += ScanOptionsButton_Click;
            scanModeMenu = BuildScanModeMenu();

            themeButton = new Button();
            themeButton.Text = "AYDINLIK";
            themeButton.Location = new Point(628, 18);
            themeButton.Size = new Size(98, 34);
            themeButton.FlatStyle = FlatStyle.Flat;
            themeButton.FlatAppearance.BorderSize = 1;
            themeButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            themeButton.Cursor = Cursors.Hand;
            themeButton.Click += ThemeButton_Click;
            toolbarPanel.Controls.Add(themeButton);

            filterLabel = new Label();
            filterLabel.Text = "🔍 ARA";
            filterLabel.Location = new Point(24, 26);
            filterLabel.Size = new Size(54, 18);
            filterLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            toolbarPanel.Controls.Add(filterLabel);

            filterBox = new TextBox();
            filterBox.Location = new Point(82, 20);
            filterBox.Size = new Size(265, 26);
            filterBox.BorderStyle = BorderStyle.FixedSingle;
            filterBox.Font = new Font("Segoe UI", 9.5f);
            filterBox.TextChanged += FilterBox_TextChanged;
            toolbarPanel.Controls.Add(filterBox);

            topFilesButton = new Button();
            topFilesButton.Text = "İLK 100";
            topFilesButton.Location = new Point(350, 18);
            topFilesButton.Size = new Size(84, 34);
            topFilesButton.FlatStyle = FlatStyle.Flat;
            topFilesButton.FlatAppearance.BorderSize = 1;
            topFilesButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            topFilesButton.Cursor = Cursors.Hand;
            topFilesButton.Click += TopFilesButton_Click;
            toolbarPanel.Controls.Add(topFilesButton);

            duplicatesButton = new Button();
            duplicatesButton.Text = "KOPYALAR";
            duplicatesButton.Location = new Point(442, 18);
            duplicatesButton.Size = new Size(96, 34);
            duplicatesButton.FlatStyle = FlatStyle.Flat;
            duplicatesButton.FlatAppearance.BorderSize = 1;
            duplicatesButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            duplicatesButton.Cursor = Cursors.Hand;
            duplicatesButton.Click += DuplicatesButton_Click;
            toolbarPanel.Controls.Add(duplicatesButton);

            csvButton = new Button();
            csvButton.Text = "CSV";
            csvButton.Location = new Point(546, 18);
            csvButton.Size = new Size(68, 34);
            csvButton.FlatStyle = FlatStyle.Flat;
            csvButton.FlatAppearance.BorderSize = 1;
            csvButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            csvButton.Cursor = Cursors.Hand;
            csvButton.Click += CsvButton_Click;
            toolbarPanel.Controls.Add(csvButton);

            accountButton = new Button();
            accountButton.Text = "HESAP";
            accountButton.Location = new Point(738, 18);
            accountButton.Size = new Size(82, 34);
            accountButton.FlatStyle = FlatStyle.Flat;
            accountButton.FlatAppearance.BorderSize = 1;
            accountButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            accountButton.Cursor = Cursors.Hand;
            accountButton.Click += AccountButton_Click;
            toolbarPanel.Controls.Add(accountButton);

            licenseButton = new Button();
            licenseButton.Text = "PLAN";
            licenseButton.Location = new Point(828, 18);
            licenseButton.Size = new Size(78, 34);
            licenseButton.FlatStyle = FlatStyle.Flat;
            licenseButton.FlatAppearance.BorderSize = 1;
            licenseButton.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            licenseButton.Cursor = Cursors.Hand;
            licenseButton.Click += LicenseButton_Click;
            toolbarPanel.Controls.Add(licenseButton);

            licenseLabel = new Label();
            licenseLabel.Text = "";
            licenseLabel.Location = new Point(918, 27);
            licenseLabel.Size = new Size(180, 18);
            licenseLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            toolbarPanel.Controls.Add(licenseLabel);

            statusLabel = new Label();
            statusLabel.Location = new Point(28, 50);
            statusLabel.Width = 760;
            statusLabel.Font = new Font("Segoe UI", 8);
            statusLabel.Text = "Klasör seçmek için Yeni Tarama butonuna basın.";
            toolbarPanel.Controls.Add(statusLabel);

            liveCountLabel = new Label();
            liveCountLabel.Location = new Point(800, 50);
            liveCountLabel.Width = 300;
            liveCountLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            liveCountLabel.Text = "";
            toolbarPanel.Controls.Add(liveCountLabel);
            LayoutToolbar();

            // --- Ağ Sürücüsü Banner (başta gizli) ---
            networkBanner = new Panel();
            networkBanner.Dock = DockStyle.Top;
            networkBanner.Height = 32;
            networkBanner.Visible = false;
            networkBanner.Cursor = Cursors.Default;

            networkBannerLabel = new Label();
            networkBannerLabel.Dock = DockStyle.Fill;
            networkBannerLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            networkBannerLabel.TextAlign = ContentAlignment.MiddleCenter;
            networkBanner.Controls.Add(networkBannerLabel);

            statsBar = new Panel();
            statsBar.Dock = DockStyle.Bottom;
            statsBar.Height = 28;

            statTotal = new Label();
            statTotal.Location = new Point(15, 7); statTotal.Width = 300;
            statTotal.Font = new Font("Courier New", 8);
            statsBar.Controls.Add(statTotal);

            statFiles = new Label();
            statFiles.Location = new Point(325, 7); statFiles.Width = 200;
            statFiles.Font = new Font("Courier New", 8);
            statsBar.Controls.Add(statFiles);

            statTime = new Label();
            statTime.Location = new Point(535, 7); statTime.Width = 200;
            statTime.Font = new Font("Courier New", 8);
            statsBar.Controls.Add(statTime);

            progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Bottom;
            progressBar.Height = 4;

            treeHost = new Panel();
            treeHost.Dock = DockStyle.Left;
            treeHost.Width = 300;
            treeHost.Padding = new Padding(0);

            sidebarHeaderPanel = new Panel();
            sidebarHeaderPanel.Dock = DockStyle.Top;
            sidebarHeaderPanel.Height = 92;
            Panel logoMark = new Panel();
            logoMark.Location = new Point(20, 22);
            logoMark.Size = new Size(34, 34);
            logoMark.Tag = "brand-logo";
            Label logoText = new Label();
            logoText.Text = "DA";
            logoText.Dock = DockStyle.Fill;
            logoText.TextAlign = ContentAlignment.MiddleCenter;
            logoText.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            logoText.Tag = "brand-logo-text";
            logoMark.Controls.Add(logoText);
            sidebarHeaderPanel.Controls.Add(logoMark);
            titleLabel.Location = new Point(68, 17);
            versionLabel.Location = new Point(70, 44);
            sidebarHeaderPanel.Controls.Add(titleLabel);
            sidebarHeaderPanel.Controls.Add(versionLabel);
            sidebarLicenseLabel = new Label();
            sidebarLicenseLabel.Text = "";
            sidebarLicenseLabel.Location = new Point(70, 64);
            sidebarLicenseLabel.Size = new Size(205, 18);
            sidebarLicenseLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            sidebarLicenseLabel.Tag = "brand-license";
            sidebarHeaderPanel.Controls.Add(sidebarLicenseLabel);

            sidebarNavPanel = new Panel();
            sidebarNavPanel.Dock = DockStyle.Top;
            sidebarNavPanel.Height = 178;
            Button navDashboard = CreateSidebarNavButton("Genel Bakış", 12, true);
            navDashboard.Click += delegate { 
                SetActiveSidebarButton(navDashboard);
                if (rootNode != null) { BuildPieData(rootNode); piePanel.Tag = rootNode; currentVisualNode = rootNode; tabControl.SelectedIndex = 0; piePanel.Invalidate(); } 
            };
            Button navFiles = CreateSidebarNavButton("Dosyalar", 56, false);
            navFiles.Click += delegate { 
                SetActiveSidebarButton(navFiles);
                treeView.Focus(); 
            };
            Button navAnalytics = CreateSidebarNavButton("Analizler", 100, false);
            navAnalytics.Click += delegate { 
                SetActiveSidebarButton(navAnalytics);
                tabControl.SelectedIndex = 2; 
            };
            Button navSettings = CreateSidebarNavButton("Hesap ve Plan", 144, false);
            navSettings.Click += delegate { 
                SetActiveSidebarButton(navSettings);
                AccountButton_Click(navSettings, EventArgs.Empty); 
            };
            sidebarNavPanel.Controls.Add(navDashboard);
            sidebarNavPanel.Controls.Add(navFiles);
            sidebarNavPanel.Controls.Add(navAnalytics);
            sidebarNavPanel.Controls.Add(navSettings);

            sidebarFooterPanel = new Panel();
            sidebarFooterPanel.Dock = DockStyle.Bottom;
            sidebarFooterPanel.Height = 126;
            scanButton.Location = new Point(20, 20);
            scanButton.Size = new Size(217, 44);
            scanOptionsButton.Location = new Point(238, 20);
            scanOptionsButton.Size = new Size(42, 44);
            sidebarFooterPanel.Controls.Add(scanButton);
            sidebarFooterPanel.Controls.Add(scanOptionsButton);
            Label footerHelp = new Label();
            footerHelp.Text = "Destek";
            footerHelp.Location = new Point(24, 78);
            footerHelp.Size = new Size(110, 20);
            footerHelp.Font = new Font("Segoe UI", 8);
            footerHelp.Tag = "sidebar-muted";
            sidebarFooterPanel.Controls.Add(footerHelp);
            Label footerProfile = new Label();
            footerProfile.Text = "Profil";
            footerProfile.Location = new Point(150, 78);
            footerProfile.Size = new Size(110, 20);
            footerProfile.Font = new Font("Segoe UI", 8);
            footerProfile.Tag = "sidebar-muted";
            sidebarFooterPanel.Controls.Add(footerProfile);

            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.Font = new Font("Segoe UI", 9.5f);
            treeView.BorderStyle = BorderStyle.None;
            treeView.ShowLines = false;
            treeView.FullRowSelect = true;
            treeView.HotTracking = true;
            treeView.ItemHeight = 26;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.BeforeExpand += TreeView_BeforeExpand;
            treeView.AfterExpand += delegate { RefreshModernScrollBars(); };
            treeView.AfterCollapse += delegate { RefreshModernScrollBars(); };

            // Modern Explorer tarzı (chevron okları, yumuşak hover efektleri vb.) görünüm uygula
            WindowThemeHelper.ApplyExplorerTheme(treeView);

            treeScrollBar = new ModernScrollBar();
            treeScrollBar.Attach(treeView);
            treeHost.Controls.Add(treeView);
            treeHost.Controls.Add(treeScrollBar);
            treeHost.Controls.Add(sidebarFooterPanel);
            treeHost.Controls.Add(sidebarNavPanel);
            treeHost.Controls.Add(sidebarHeaderPanel);
            treeScrollBar.BringToFront();

            tabControl = new ThemedTabControl();
            tabControl.Dock = DockStyle.Right;
            tabControl.Width = 460;
            tabControl.Font = new Font("Segoe UI", 9);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            TabPage pieTab = new TabPage("  Grafik  ");
            piePanel = new Panel();
            piePanel.Dock = DockStyle.Fill;
            piePanel.Paint += PiePanel_Paint;
            piePanel.MouseMove += PiePanel_MouseMove;
            piePanel.MouseLeave += PiePanel_MouseLeave;
            pieTab.Controls.Add(piePanel);

            TabPage treemapTab = new TabPage("  Treemap  ");
            treemapPanel = new Panel();
            treemapPanel.Dock = DockStyle.Fill;
            treemapPanel.Paint += TreemapPanel_Paint;
            treemapPanel.MouseMove += TreemapPanel_MouseMove;
            treemapPanel.MouseLeave += TreemapPanel_MouseLeave;
            treemapPanel.MouseClick += TreemapPanel_MouseClick;
            treemapTab.Controls.Add(treemapPanel);

            TabPage aiTab = new TabPage("  AI Öneriler  ");
            aiHost = new Panel();
            aiHost.Dock = DockStyle.Fill;
            aiPanel = new SmoothScrollPanel();
            aiPanel.Dock = DockStyle.Fill;
            aiPanel.AutoScroll = true;
            aiScrollBar = new ModernScrollBar();
            aiScrollBar.Attach(aiPanel);
            aiHost.Controls.Add(aiPanel);
            aiHost.Controls.Add(aiScrollBar);
            aiScrollBar.BringToFront();
            aiTab.Controls.Add(aiHost);

            tabControl.TabPages.Add(pieTab);
            tabControl.TabPages.Add(treemapTab);
            tabControl.TabPages.Add(aiTab);

            listHost = new Panel();
            listHost.Dock = DockStyle.Fill;
            listHost.Padding = new Padding(28, 24, 28, 24);

            dashboardStripPanel = new TableLayoutPanel();
            dashboardStripPanel.Dock = DockStyle.Top;
            dashboardStripPanel.Height = 92;
            dashboardStripPanel.ColumnCount = 4;
            dashboardStripPanel.RowCount = 1;
            dashboardStripPanel.Padding = new Padding(0);
            dashboardStripPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            dashboardStripPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            dashboardStripPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            dashboardStripPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            dashboardStripPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            dashboardStripPanel.Controls.Add(CreateMetricCard("Toplam Boyut", "Hazır", out summaryTotalLabel), 0, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Dosya Sayısı", "0", out summaryFilesLabel), 1, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Süre", "-", out summaryTimeLabel), 2, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Konum", "-", out summaryPathLabel), 3, 0);

            listBodyPanel = new Panel();
            listBodyPanel.Dock = DockStyle.Fill;
            listBodyPanel.Resize += delegate { UpdateListNativeMasks(); };

            listView = new SmoothListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = false;
            listView.OwnerDraw = true;
            listView.Font = new Font("Segoe UI", 9);
            listView.BorderStyle = BorderStyle.None;

            // Explorer teması (modern scrollbarlar ve header) uygula
            WindowThemeHelper.ApplyExplorerTheme(listView);
            listView.Columns.Add(listColumnTitles[0], 270);
            listView.Columns.Add(listColumnTitles[1], 90);
            listView.Columns.Add(listColumnTitles[2], 90);
            listView.Columns.Add(listColumnTitles[3], 55);
            listView.Columns.Add(listColumnTitles[4], 115);
            listView.Columns.Add(listColumnTitles[5], 65);
            listView.Columns.Add(listColumnTitles[6], 280);
            listView.ColumnClick += ListView_ColumnClick;
            listView.DrawColumnHeader += ListView_DrawColumnHeader;
            listView.DrawItem += ListView_DrawItem;
            listView.DrawSubItem += ListView_DrawSubItem;
            listView.SizeChanged += delegate { AdjustListColumns(); UpdateListNativeMasks(); };
            listView.ColumnWidthChanged += delegate { UpdateListNativeMasks(); };
            listNativeVerticalMask = CreateNativeScrollbarMask();
            listNativeHorizontalMask = CreateNativeScrollbarMask();
            AdjustListColumns();
            UpdateListNativeMasks();

            listScrollBar = new ModernScrollBar();
            listScrollBar.Attach(listView);
            listBodyPanel.Controls.Add(listView);
            listBodyPanel.Controls.Add(listNativeVerticalMask);
            listBodyPanel.Controls.Add(listNativeHorizontalMask);
            listBodyPanel.Controls.Add(listScrollBar);
            listHost.Controls.Add(listBodyPanel);
            listHost.Controls.Add(dashboardStripPanel);
            listScrollBar.BringToFront();
            UpdateListNativeMasks();

            listContextMenu = new ContextMenuStrip();
            ToolStripMenuItem openItem   = new ToolStripMenuItem("  Aç (Explorer)");
            ToolStripMenuItem copyItem   = new ToolStripMenuItem("  Yolu Kopyala");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("  Sil");
            openItem.Click   += ContextMenu_Open;
            copyItem.Click   += ContextMenu_CopyPath;
            deleteItem.Click += ContextMenu_Delete;
            listContextMenu.Items.Add(openItem);
            listContextMenu.Items.Add(copyItem);
            listContextMenu.Items.Add(new ToolStripSeparator());
            listContextMenu.Items.Add(deleteItem);
            listView.ContextMenuStrip = listContextMenu;

            // Ekleme sirasi onemli: banner toolbar'in altinda gorunmeli
            this.Controls.Add(listHost);
            this.Controls.Add(treeHost);
            this.Controls.Add(tabControl);
            this.Controls.Add(progressBar);
            this.Controls.Add(statsBar);
            this.Controls.Add(networkBanner);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(mainMenu);

            uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 450;
            uiTimer.Tick += UiTimer_Tick;

            filterTimer = new System.Windows.Forms.Timer();
            filterTimer.Interval = 260;
            filterTimer.Tick += FilterTimer_Tick;
        }

        private Button CreateSidebarNavButton(string text, int y, bool selected)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(12, y);
            button.Size = new Size(276, 38);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = selected ? 1 : 0;
            button.Font = new Font("Segoe UI", 9, selected ? FontStyle.Bold : FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(18, 0, 0, 0);
            button.Cursor = Cursors.Hand;
            button.Tag = selected ? "nav-active" : "nav";
            return button;
        }

        private Control CreateMetricCard(string title, string valueText, out Label valueLabel)
        {
            MetricCardPanel card = new MetricCardPanel();
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 12, 14);
            card.Padding = new Padding(18, 12, 18, 10);
            card.Tag = "metric-card";

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 22;
            titleLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            titleLabel.Tag = "metric-title";
            card.Controls.Add(titleLabel);

            valueLabel = new Label();
            valueLabel.Text = valueText;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            valueLabel.AutoEllipsis = true;
            valueLabel.Tag = "metric-value";
            card.Controls.Add(valueLabel);
            valueLabel.BringToFront();

            return card;
        }

        private void UpdateSummaryCards(string total, string files, string time, string path)
        {
            if (summaryTotalLabel != null && total != null) summaryTotalLabel.Text = total;
            if (summaryFilesLabel != null && files != null) summaryFilesLabel.Text = files;
            if (summaryTimeLabel != null && time != null) summaryTimeLabel.Text = time;
            if (summaryPathLabel != null && path != null) summaryPathLabel.Text = CompactPath(path, 58);
        }

        private string CompactPath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength) return path ?? "";
            string root = PathText.GetRoot(path);
            string name = PathText.GetFileName(path);
            if (!string.IsNullOrEmpty(root) && !string.IsNullOrEmpty(name))
            {
                string compact = root + "..." + Path.DirectorySeparatorChar + name;
                if (compact.Length <= maxLength) return compact;
            }
            return "..." + path.Substring(Math.Max(0, path.Length - maxLength + 3));
        }

        // =====================================================================
        // BÖLÜM: EULA / GİZLİLİK GÜVENCESİ
        // =====================================================================
        // Amacı  : İlk çalıştırmada kullanıcıdan gizlilik/EULA onayı alır.
        // Yöntemi: Registry'de (HKCU\Software\AdvancedDiskAnalyzer) "PrivacyAccepted"
        //          değeri kontrol edilir. Yoksa PrivacyConsentForm gösterilir.
        //          Onay verilirse tarih, versiyon ve damga kaydedilir.
        // Notlar : Kurumsal lisanslarda özel EULA metni desteklenir.
        //          Onay reddedilirse uygulama kapatılır.
        // =====================================================================

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (IsEulaAccepted()) return;

            using (PrivacyConsentForm form = new PrivacyConsentForm(AppVersion, EulaStampDate,
                LicenseManager.GetEnterpriseEulaText(currentLicense)))
            {
                DialogResult result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                    SaveEulaAccepted();
                else
                    this.Close();
            }
        }

        private bool IsEulaAccepted()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(EulaRegistryPath))
                {
                    if (key == null) return false;
                    object value = key.GetValue("PrivacyAccepted");
                    return value != null && value.ToString() == "1";
                }
            }
            catch { return false; }
        }

        private void SaveEulaAccepted()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(EulaRegistryPath))
                {
                    if (key == null) return;
                    key.SetValue("PrivacyAccepted", "1");
                    key.SetValue("AcceptedVersion", AppVersion);
                    key.SetValue("AcceptedStampDate", EulaStampDate);
                    key.SetValue("AcceptedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch
            {
                MessageBox.Show("Gizlilik onayı kayıt defterine yazılamadı. Uygulama bu oturumda devam edecek.",
                    "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        // BÖLÜM: AĞ SÜRÜCÜSÜ ALGILAMA
        // =====================================================================
        // Amacı  : Seçilen tarama yolunun ağ sürücüsü (UNC veya mapped drive)
        //          olup olmadığını tespit eder ve kullanıcıyı bilgilendirir.
        // Yöntemi: DriveInfo.DriveType == Network kontrolü + UNC path tespiti.
        //          Mapped drive ise WMI (Win32_NetworkConnection) sorgusuyla
        //          uzak hedef (\\server\share) bulunur.
        // Etki   : Ağ sürücüsünde parallelism kısıtlanır (2 thread),
        //          NTFS Turbo devre dışı bırakılır, banner gösterilir.
        // =====================================================================

        /// <summary>
        /// Verilen path'in ağ sürücüsü (mapped drive veya UNC) olup olmadığını döndürür.
        /// </summary>
        private bool IsNetworkPath(string path)
        {
            try
            {
                // UNC path kontrolu: \\server\share
                if (path.StartsWith(@"\\")) return true;

                // Surucü harfi varsa DriveInfo ile kontrol et
                string root = Path.GetPathRoot(path);
                if (!string.IsNullOrEmpty(root))
                {
                    DriveInfo drive = new DriveInfo(root);
                    return drive.DriveType == DriveType.Network;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Ağ sürücüsü ise UNC hedefini bulmaya çalışır, banner metnini hazırlar.
        /// </summary>
        private string GetNetworkDriveDetail(string path)
        {
            try
            {
                // UNC path dogrudan kullaniliyorsa
                if (path.StartsWith(@"\\"))
                    return path;

                // Mapped drive ise hedef UNC'yi bul
                string root = Path.GetPathRoot(path).TrimEnd('\\');
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_NetworkConnection WHERE LocalName = '" + root + "'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string remote = obj["RemoteName"] as string;
                    if (!string.IsNullOrEmpty(remote))
                        return root + "  →  " + remote;
                }
            }
            catch { }
            return path;
        }

        /// <summary>
        /// Banner'ı göster veya gizle, renk ve metin ayarla.
        /// </summary>
        private void ShowNetworkBanner(bool show, string message = "")
        {
            networkBanner.Visible = show;
            if (show)
            {
                networkBannerLabel.Text = message;
                networkBanner.BackColor = Color.FromArgb(Theme.IsDark ? 80 : 60,
                    Theme.Warning.R, Theme.Warning.G, Theme.Warning.B);
                networkBannerLabel.ForeColor = Theme.IsDark
                    ? Color.FromArgb(255, 220, 100)
                    : Color.FromArgb(100, 60, 0);
            }
        }

        // =====================================================================
        // BÖLÜM: TEMA MOTORU
        // =====================================================================
        // Amacı  : Koyu ↔ Açık tema geçişini yönetir. Tüm kontrollerin
        //          BackColor, ForeColor ve FlatAppearance değerlerini günceller.
        // Yöntemi: ApplyTheme() tüm kontrol ağacını dolaşarak Tag property'sine
        //          göre renk atar. DWM API ile başlık çubuğu da koyu yapılır.
        // Kapsam : MainMenu, Toolbar, Sidebar, TreeView, ListView,
        //          TabControl, PiePanel, AI Panel, ContextMenu, StatsBar
        // =====================================================================

        private void ThemeButton_Click(object sender, EventArgs e)
        {
            Theme.IsDark = !Theme.IsDark;
            themeButton.Text = Theme.IsDark ? "AYDINLIK" : "KARANLIK";
            ApplyTheme();
            piePanel.Invalidate();
            if (rootNode != null)
                GenerateAIRecommendations(currentSelectedDirectory ?? rootNode);
        }

        private void ApplyTheme()
        {
            this.BackColor = Theme.Bg;
            ApplyMenuTheme();

            toolbarPanel.BackColor = Theme.Bg;
            foreach (Control c in toolbarPanel.Controls)
            {
                c.BackColor = Theme.Bg;
                c.ForeColor = Theme.SubText;
            }

            scanButton.BackColor = Theme.Accent;
            scanButton.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            scanButton.FlatAppearance.BorderColor = Theme.Accent;
            scanButton.FlatAppearance.MouseOverBackColor = Theme.IsDark ? Color.FromArgb(65, 145, 255) : Color.FromArgb(35, 105, 195);
            scanButton.FlatAppearance.MouseDownBackColor = Theme.IsDark ? Color.FromArgb(35, 110, 220) : Color.FromArgb(15, 70, 150);
            if (scanOptionsButton != null)
            {
                scanOptionsButton.BackColor = Theme.Accent;
                scanOptionsButton.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
                scanOptionsButton.FlatAppearance.BorderColor = Theme.Accent;
                scanOptionsButton.FlatAppearance.MouseOverBackColor = Theme.IsDark ? Color.FromArgb(65, 145, 255) : Color.FromArgb(35, 105, 195);
                scanOptionsButton.FlatAppearance.MouseDownBackColor = Theme.IsDark ? Color.FromArgb(35, 110, 220) : Color.FromArgb(15, 70, 150);
            }
            ApplyScanModeMenuTheme();

            themeButton.BackColor = Theme.Card;
            themeButton.ForeColor = Theme.SubText;
            themeButton.FlatAppearance.BorderColor = Theme.Border;
            themeButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            themeButton.FlatAppearance.MouseDownBackColor = Theme.Border;

            filterBox.BackColor = Theme.Card;
            filterBox.ForeColor = Theme.Text;

            topFilesButton.BackColor = Theme.Card;
            topFilesButton.ForeColor = Theme.SubText;
            topFilesButton.FlatAppearance.BorderColor = Theme.Border;
            topFilesButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            topFilesButton.FlatAppearance.MouseDownBackColor = Theme.Border;

            duplicatesButton.BackColor = Theme.Card;
            duplicatesButton.ForeColor = Theme.SubText;
            duplicatesButton.FlatAppearance.BorderColor = Theme.Border;
            duplicatesButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            duplicatesButton.FlatAppearance.MouseDownBackColor = Theme.Border;

            csvButton.BackColor = Theme.Card;
            csvButton.ForeColor = Theme.SubText;
            csvButton.FlatAppearance.BorderColor = Theme.Border;
            csvButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            csvButton.FlatAppearance.MouseDownBackColor = Theme.Border;

            bool signedIn = !string.IsNullOrEmpty(OnlineLicenseClient.GetSavedToken());
            accountButton.BackColor = signedIn ? Theme.Card : Theme.Surface;
            accountButton.ForeColor = signedIn ? Theme.Success : Theme.SubText;
            accountButton.FlatAppearance.BorderColor = signedIn ? Theme.Success : Theme.Border;
            accountButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            accountButton.FlatAppearance.MouseDownBackColor = Theme.Border;

            licenseButton.BackColor = LicenseManager.HasPaidPlan(currentLicense) ? Theme.Success : Theme.Card;
            licenseButton.ForeColor = LicenseManager.HasPaidPlan(currentLicense)
                ? (Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White)
                : Theme.SubText;
            licenseButton.FlatAppearance.BorderColor = LicenseManager.HasPaidPlan(currentLicense) ? Theme.Success : Theme.Border;
            licenseButton.FlatAppearance.MouseOverBackColor = Theme.Highlight;
            licenseButton.FlatAppearance.MouseDownBackColor = Theme.Border;
            licenseLabel.BackColor = Theme.Bg;
            licenseLabel.ForeColor = LicenseManager.IsEnterprise(currentLicense) ? Theme.Success : Theme.SubText;

            statusLabel.ForeColor  = Theme.SubText;
            liveCountLabel.ForeColor = Theme.Success;
            ApplySidebarTheme();

            // Banner rengi guncelle
            if (networkBanner.Visible)
                ShowNetworkBanner(true, networkBannerLabel.Text);

            statsBar.BackColor  = Theme.Surface;
            statTotal.BackColor = Theme.Surface; statTotal.ForeColor = Theme.SubText;
            statFiles.BackColor = Theme.Surface; statFiles.ForeColor = Theme.SubText;
            statTime.BackColor  = Theme.Surface; statTime.ForeColor  = Theme.SubText;

            treeHost.BackColor = Theme.Surface;
            treeView.BackColor = Theme.Surface;
            treeView.ForeColor = Theme.Text;
            treeView.LineColor = Theme.Border;

            listHost.BackColor = Theme.Bg;
            if (listBodyPanel != null) listBodyPanel.BackColor = Theme.Bg;
            listView.BackColor = Theme.Surface;
            listView.ForeColor = Theme.Text;
            ApplyDashboardTheme();

            tabControl.BackColor = Theme.Bg;
            foreach (TabPage page in tabControl.TabPages)
                page.BackColor = Theme.Bg;
            piePanel.BackColor   = Theme.Bg;
            treemapPanel.BackColor = Theme.Bg;
            if (aiHost != null) aiHost.BackColor = Theme.Bg;
            aiPanel.BackColor    = Theme.Bg;

            listContextMenu.BackColor = Theme.Card;
            listContextMenu.ForeColor = Theme.Text;
            foreach (ToolStripItem item in listContextMenu.Items)
            {
                item.BackColor = Theme.Card;
                item.ForeColor = item.Text.Contains("Sil") ? Theme.Danger : Theme.Text;
            }

            tabControl.Invalidate();
            listView.Invalidate();
            UpdateListNativeMasks();
            if (treemapPanel != null) treemapPanel.Invalidate();
            RefreshModernScrollBars();
            DarkTitleBar.Apply(this.Handle, Theme.IsDark);
            this.Invalidate(true);
        }

        private void ApplyDashboardTheme()
        {
            if (dashboardStripPanel == null) return;
            dashboardStripPanel.BackColor = Theme.Bg;
            foreach (Control card in dashboardStripPanel.Controls)
            {
                card.BackColor = Theme.Card;
                card.ForeColor = Theme.Text;
                MetricCardPanel metric = card as MetricCardPanel;
                if (metric != null) metric.Invalidate();
                foreach (Control child in card.Controls)
                {
                    string tag = child.Tag as string;
                    child.BackColor = Theme.Card;
                    child.ForeColor = tag == "metric-title" ? Theme.SubText : Theme.Text;
                }
            }
        }

        private void ApplySidebarTheme()
        {
            if (treeHost == null) return;
            treeHost.BackColor = Theme.Surface;
            if (sidebarHeaderPanel != null) sidebarHeaderPanel.BackColor = Theme.Surface;
            if (sidebarNavPanel != null) sidebarNavPanel.BackColor = Theme.Surface;
            if (sidebarFooterPanel != null) sidebarFooterPanel.BackColor = Theme.Surface;
            ThemeSidebarChildren(treeHost);
        }

        private void ThemeSidebarChildren(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c == treeView || c == treeScrollBar) continue;

                c.BackColor = Theme.Surface;
                c.ForeColor = Theme.SubText;

                string tag = c.Tag as string;
                if (tag == "brand-title")
                {
                    c.ForeColor = Theme.Text;
                    c.BackColor = Theme.Surface;
                }
                else if (tag == "brand-version" || tag == "brand-license")
                {
                    c.ForeColor = tag == "brand-license" && LicenseManager.HasPaidPlan(currentLicense) ? Theme.Success : Theme.SubText;
                    c.BackColor = Theme.Surface;
                }
                else if (tag == "sidebar-muted")
                {
                    c.ForeColor = Theme.SubText;
                    c.BackColor = Theme.Surface;
                }
                else if (tag == "brand-logo")
                {
                    c.BackColor = Theme.Highlight;
                    c.ForeColor = Theme.Text;
                }
                else if (tag == "brand-logo-text")
                {
                    c.BackColor = Theme.Highlight;
                    c.ForeColor = Theme.Accent;
                }

                Button button = c as Button;
                if (button != null)
                {
                    if (button == scanButton)
                    {
                        button.BackColor = Theme.Accent;
                        button.ForeColor = Theme.IsDark ? Color.FromArgb(8, 12, 18) : Color.White;
                        button.FlatAppearance.BorderColor = Theme.Accent;
                        button.FlatAppearance.BorderSize = 0;
                        button.FlatAppearance.MouseOverBackColor = Theme.IsDark ? Color.FromArgb(65, 145, 255) : Color.FromArgb(35, 105, 195);
                        button.FlatAppearance.MouseDownBackColor = Theme.IsDark ? Color.FromArgb(35, 110, 220) : Color.FromArgb(15, 70, 150);
                    }
                    else if (button == scanOptionsButton)
                    {
                        button.BackColor = Theme.Accent;
                        button.ForeColor = Theme.IsDark ? Color.FromArgb(8, 12, 18) : Color.White;
                        button.FlatAppearance.BorderColor = Theme.Accent;
                        button.FlatAppearance.BorderSize = 0;
                        button.FlatAppearance.MouseOverBackColor = Theme.IsDark ? Color.FromArgb(65, 145, 255) : Color.FromArgb(35, 105, 195);
                        button.FlatAppearance.MouseDownBackColor = Theme.IsDark ? Color.FromArgb(35, 110, 220) : Color.FromArgb(15, 70, 150);
                    }
                    else if (tag == "nav-active")
                    {
                        button.BackColor = Theme.Highlight;
                        button.ForeColor = Theme.Text;
                        button.FlatAppearance.BorderColor = Theme.Accent;
                        button.FlatAppearance.BorderSize = 1;
                        button.FlatAppearance.MouseOverBackColor = Theme.Highlight;
                        button.FlatAppearance.MouseDownBackColor = Theme.Border;
                    }
                    else if (tag == "nav")
                    {
                        button.BackColor = Theme.Surface;
                        button.ForeColor = Theme.SubText;
                        button.FlatAppearance.BorderColor = Theme.Surface;
                        button.FlatAppearance.BorderSize = 0;
                        button.FlatAppearance.MouseOverBackColor = Theme.Highlight;
                        button.FlatAppearance.MouseDownBackColor = Theme.Border;
                    }
                }

                if (c.HasChildren)
                    ThemeSidebarChildren(c);
            }
        }

        private void RefreshModernScrollBars()
        {
            if (treeScrollBar != null) treeScrollBar.RefreshTheme();
            if (listScrollBar != null) listScrollBar.RefreshTheme();
            if (aiScrollBar != null) aiScrollBar.RefreshTheme();
        }

        private void RefreshLicenseUi()
        {
            if (currentLicense == null)
                currentLicense = LicenseManager.Load();

            if (licenseLabel != null)
                licenseLabel.Text = ToTurkishPlanName(currentLicense.PlanName).ToUpperInvariant();

            if (sidebarLicenseLabel != null)
                sidebarLicenseLabel.Text = "Plan: " + ToTurkishPlanName(currentLicense.PlanName);

            if (licenseButton != null)
                licenseButton.Text = "PLAN";

            if (this.IsHandleCreated)
                ApplyTheme();
        }

        private string ToTurkishPlanName(string planName)
        {
            if (string.Equals(planName, "Free", StringComparison.OrdinalIgnoreCase))
                return "Ücretsiz";
            if (string.Equals(planName, "Enterprise", StringComparison.OrdinalIgnoreCase))
                return "Kurumsal";
            if (string.Equals(planName, "Pro", StringComparison.OrdinalIgnoreCase))
                return "Pro";
            return string.IsNullOrEmpty(planName) ? "Ücretsiz" : planName;
        }

        private void LicenseButton_Click(object sender, EventArgs e)
        {
            using (LicenseForm form = new LicenseForm(currentLicense, PurchaseUrl, EnterpriseContactUrl))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    currentLicense = LicenseManager.Load();
                    RefreshLicenseUi();
                    statusLabel.Text = "Lisans durumu: " + ToTurkishPlanName(currentLicense.PlanName);
                    if (rootNode != null) GenerateAIRecommendations(rootNode);
                }
            }
        }

        private void AccountButton_Click(object sender, EventArgs e)
        {
            using (AccountForm form = new AccountForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshLicenseUi();
                    statusLabel.Text = string.IsNullOrEmpty(OnlineLicenseClient.GetSavedEmail())
                        ? "Hesap oturumu kapalı."
                        : "Hesap aktif: " + OnlineLicenseClient.GetSavedEmail();
                }
            }
        }

        private bool EnsureFeature(LicenseFeature feature, string title)
        {
            if (LicenseManager.HasFeature(currentLicense, feature))
                return true;

            using (UpgradeForm form = new UpgradeForm(currentLicense, feature, title, PurchaseUrl, EnterpriseContactUrl))
                form.ShowDialog(this);
            currentLicense = LicenseManager.Load();
            RefreshLicenseUi();
            return LicenseManager.HasFeature(currentLicense, feature);
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Theme.Card))
                e.Graphics.FillRectangle(brush, e.Bounds);
            using (Pen pen = new Pen(Theme.Border))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top + 4, e.Bounds.Right - 1, e.Bounds.Bottom - 4);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, listView.Font, textRect, Theme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // SubItem cizimi tum satiri kontrol ediyor.
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color alt = Theme.IsDark ? Color.FromArgb(17, 22, 30) : Color.FromArgb(250, 251, 253);
            Color bg = e.Item.Selected ? Theme.Highlight : (e.ItemIndex % 2 == 0 ? Theme.Surface : alt);
            using (SolidBrush brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.Bounds);

            // Seçili satırın soluna dikey mavi vurgu çizgisi çizimi (premium VS Code tarzı)
            if (e.Item.Selected && e.ColumnIndex == 0)
            {
                Rectangle highlightBar = new Rectangle(e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);
                using (SolidBrush accentBrush = new SolidBrush(Theme.Accent))
                    e.Graphics.FillRectangle(accentBrush, highlightBar);
            }

            using (Pen pen = new Pen(Color.FromArgb(Theme.IsDark ? 34 : 220, Theme.Border)))
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top + 3, e.Bounds.Right - 1, e.Bounds.Bottom - 3);

            Color fg = e.Item.Selected ? Theme.Text : e.Item.ForeColor;
            if (fg == Color.Empty) fg = Theme.Text;

            // İlk sütunda seçili satırın mavi çizgisinin üzerine yazı gelmemesi için metni kaydırıyoruz
            int leftOffset = (e.ColumnIndex == 0 && e.Item.Selected) ? 11 : 8;
            Rectangle textRect = new Rectangle(e.Bounds.X + leftOffset, e.Bounds.Y, e.Bounds.Width - (leftOffset + 4), e.Bounds.Height);

            if (e.ColumnIndex == 3)
            {
                int score;
                if (int.TryParse(e.SubItem.Text, out score))
                {
                    Color scoreColor = score >= 70 ? Theme.Danger : (score >= 40 ? Theme.Warning : Theme.Success);
                    Rectangle pill = new Rectangle(e.Bounds.X + 9, e.Bounds.Y + 5, Math.Max(34, e.Bounds.Width - 18), Math.Max(18, e.Bounds.Height - 10));
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(Theme.IsDark ? 48 : 28, scoreColor)))
                        e.Graphics.FillRectangle(brush, pill);
                    using (Pen pen = new Pen(scoreColor))
                        e.Graphics.DrawRectangle(pen, pill);
                    TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listView.Font, pill, scoreColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
                    return;
                }
            }

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listView.Font, textRect, fg,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void AdjustListColumns()
        {
            if (listView == null || listView.Columns.Count < 7) return;
            int chrome = Math.Max(14, SystemInformation.VerticalScrollBarWidth);
            int available = Math.Max(620, listView.ClientSize.Width - chrome - 6);

            int sizeW = 104;
            int allocatedW = 104;
            int scoreW = 56;
            int dateW = 126;
            int typeW = 74;
            int nameW = Math.Max(210, Math.Min(340, (int)(available * 0.30)));
            int locationW = available - (nameW + sizeW + allocatedW + scoreW + dateW + typeW);

            if (locationW < 220)
            {
                int need = 220 - locationW;
                nameW = Math.Max(180, nameW - need);
                locationW = available - (nameW + sizeW + allocatedW + scoreW + dateW + typeW);
            }
            locationW = Math.Max(180, locationW);

            SetColumnWidth(0, nameW);
            SetColumnWidth(1, sizeW);
            SetColumnWidth(2, allocatedW);
            SetColumnWidth(3, scoreW);
            SetColumnWidth(4, dateW);
            SetColumnWidth(5, typeW);
            SetColumnWidth(6, locationW);
            UpdateListNativeMasks();
            SmoothListView smooth = listView as SmoothListView;
            if (smooth != null) smooth.HideChrome();
        }

        private void SetColumnWidth(int index, int width)
        {
            if (listView.Columns[index].Width != width)
                listView.Columns[index].Width = width;
        }

        private Panel CreateNativeScrollbarMask()
        {
            Panel mask = new Panel();
            mask.BackColor = Theme.Surface;
            mask.Enabled = true;
            mask.Cursor = Cursors.Default;
            mask.MouseWheel += delegate(object sender, MouseEventArgs e)
            {
                if (listScrollBar != null) listScrollBar.ScrollWheelDelta(e.Delta);
            };
            return mask;
        }

        private void UpdateListNativeMasks()
        {
            if (listView == null || listNativeVerticalMask == null || listNativeHorizontalMask == null) return;
            int w = Math.Max(14, SystemInformation.VerticalScrollBarWidth);
            int h = Math.Max(14, SystemInformation.HorizontalScrollBarHeight);
            listNativeVerticalMask.BackColor = Theme.Surface;
            listNativeHorizontalMask.BackColor = Theme.Surface;
            Rectangle b = listView.Bounds;
            listNativeVerticalMask.Bounds = new Rectangle(Math.Max(0, b.Right - w), b.Top, w, Math.Max(0, b.Height));
            listNativeHorizontalMask.Bounds = new Rectangle(b.Left, Math.Max(0, b.Bottom - h), Math.Max(0, b.Width), h);
            listNativeVerticalMask.BringToFront();
            listNativeHorizontalMask.BringToFront();
            if (listScrollBar != null) listScrollBar.BringToFront();
            SmoothListView smooth = listView as SmoothListView;
            if (smooth != null) smooth.HideChrome();
        }

        private int GetCurrentLiveListLimit()
        {
            return ultraFastMode ? UltraLiveListItemLimit : LiveListItemLimit;
        }

        // =====================================================================
        // BÖLÜM: UI ZAMANLAYICI (Canlı Liste Güncelleme)
        // =====================================================================
        // Amacı  : Tarama sırasında arkaplandaki thread'lerden gelen dosyaları
        //          450ms aralıklarla ListView'e batch halinde ekler.
        // Yöntemi: pendingFiles kuyruğundan max 60 dosya alınır, WM_SETREDRAW
        //          kapatılıp ekleme yapılır, sonra tekrar açılır (flicker önleme).
        // Sınır  : Ultra hızlı modda 1600, standart modda 4000 öğeden sonra
        //          canlı liste durdurulur (UI donmasını önlemek için).
        // =====================================================================

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            int liveLimit = GetCurrentLiveListLimit();
            liveCountLabel.Text = liveListLimited
                ? string.Format("{0:N0} dosya  |  {1} canlı liste {2:N0}+ ile sınırlı", totalFilesFound, ultraFastMode ? "ultra" : "standart", liveLimit)
                : string.Format("{0:N0} dosya", totalFilesFound);
            if (summaryFilesLabel != null)
                summaryFilesLabel.Text = string.Format("{0:N0}", totalFilesFound);
            if (summaryTotalLabel != null)
            {
                long logical = Interlocked.Read(ref totalBytesScanned);
                long allocated = Interlocked.Read(ref totalAllocatedScanned);
                summaryTotalLabel.Text = FormatLogicalAllocated(logical, allocated);
            }

            List<FileNode> batch = new List<FileNode>();
            lock (queueLock)
            {
                while (pendingFiles.Count > 0 && batch.Count < 60)
                    batch.Add(pendingFiles.Dequeue());
            }
            if (batch.Count == 0) return;

            if (listView.Items.Count >= liveLimit)
            {
                liveListLimited = true;
                return;
            }

            ListViewItem topItem = null;
            try { topItem = listView.TopItem; } catch { }
            bool hadFocus = listView.Focused;

            NativeListViewPaint.SetRedraw(listView, false);
            try
            {
                List<ListViewItem> itemsToAdd = new List<ListViewItem>(batch.Count);
                int currentCount = listView.Items.Count;
                foreach (FileNode f in batch)
                {
                    if (currentCount + itemsToAdd.Count >= liveLimit)
                    {
                        liveListLimited = true;
                        break;
                    }
                    itemsToAdd.Add(CreateFileListItem(f));
                }
                
                if (itemsToAdd.Count > 0)
                {
                    listView.Items.AddRange(itemsToAdd.ToArray());
                }

                if (topItem != null && topItem.ListView == listView)
                {
                    try { listView.TopItem = topItem; } catch { }
                }
            }
            finally
            {
                NativeListViewPaint.SetRedraw(listView, true);
            }
            listView.Invalidate(new Rectangle(0, 0, listView.ClientSize.Width, Math.Min(listView.ClientSize.Height, Math.Max(40, batch.Count * 24 + 32))));
            if (hadFocus) listView.Focus();

            liveUiTicks++;
            if (listScrollBar != null && liveUiTicks % 4 == 0)
                listScrollBar.RefreshTheme();
        }

        // =====================================================================
        // BÖLÜM: TARAMA MOTORU
        // =====================================================================
        // Amacı  : Klasör seçim diyaloğu açar, taramayı başlatır,
        //          tamamlanınca ağaç/grafik/analiz panellerini doldurur.
        // Akış   : 1) Klasör seçimi → 2) Ağ sürücüsü kontrolü
        //          3) Yönetici yetkisi önerisi (NTFS Turbo için)
        //          4) ScanRoot() — önce MFT Turbo dener, başarısız olursa
        //             WinAPI FastScan'e düşer
        //          5) Sonuç ağacı oluşturulur, görselleştirme tetiklenir
        // İptal  : CancellationTokenSource ile tarama her an iptal edilebilir.
        //          İptal sonrası o ana kadar bulunan dosyalar listede kalır.
        // Geçmiş: Tarama sonucu CSV geçmişine yazılır, önceki snapshot ile
        //          karşılaştırılıp delta gösterilir.
        // =====================================================================

        private async void ScanButton_Click(object sender, EventArgs e)
        {
            if (scanInProgress)
            {
                try
                {
                    if (scanCancelSource != null)
                        scanCancelSource.Cancel();
                }
                catch { }
                scanButton.Enabled = false;
                scanButton.Text = "İPTAL...";
                statusLabel.Text = "Tarama iptal ediliyor...";
                return;
            }

            ApplySelectedScanMode();
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string selectedPath = dialog.SelectedPath;
            selectedScanPath = selectedPath;

            // --- AĞ SÜRÜCÜSÜ KONTROLÜ ---
            isNetworkDrive = IsNetworkPath(selectedPath);
            if (isNetworkDrive)
            {
                if (!EnsureFeature(LicenseFeature.NetworkScan, "Ağ sürücüsü taraması"))
                    return;

                networkDriveInfo = GetNetworkDriveDetail(selectedPath);
                string bannerMsg = "AĞ SÜRÜCÜSÜ  |  " + networkDriveInfo +
                                   "  |  Yavaş mod aktif - tarama daha uzun sürebilir";
                ShowNetworkBanner(true, bannerMsg);

                // Kullanıcıya bilgi ver, onay al
                DialogResult confirm = MessageBox.Show(
                    "Ağ sürücüsü seçildi:\n" + networkDriveInfo +
                    "\n\nAğ taraması lokal taramadan çok daha yavaş olabilir." +
                    "\nDevam etmek istiyor musunuz?",
                    "Ağ Sürücüsü Algılandı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (confirm != DialogResult.Yes)
                {
                    ShowNetworkBanner(false);
                    return;
                }

                // Ağ modunda SSD paralelliği kullanma
                isSSD = false;
            }
            else
            {
                ShowNetworkBanner(false);
                networkDriveInfo = "";
            }

            if (ShouldOfferAdminRestartForTurbo(selectedPath))
            {
                DialogResult adminChoice = MessageBox.Show(
                    "En hızlı NTFS Turbo tarama için uygulamanın yönetici yetkisiyle çalışması gerekiyor.\n\n" +
                    "Uygulamayı şimdi yönetici yetkileriyle yeniden başlatmak ister misiniz?\n\n" +
                    "(Hayır seçeneğini tıklarsanız, tarama iptal edilmez ve yönetici yetkisi gerektirmeyen 'Hızlı Tarama (WinAPI)' motoru ile devam edilir.)",
                    "Yönetici Yetkisi Gerekli (NTFS Turbo)",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (adminChoice == DialogResult.Yes)
                {
                    RestartAsAdmin_Click(this, EventArgs.Empty);
                    return;
                }
                else
                {
                    // Hayır denilirse, sessizce Hızlı Tarama (WinAPI) moduna geçerek devam et
                    ntfsTurboMode = false;
                    selectedScanMode = ScanMode.FastWinApi;
                    if (statusLabel != null)
                        statusLabel.Text = "NTFS Turbo reddedildi, Hızlı Tarama (WinAPI) motoruna geçildi.";
                }
            }

            CancellationTokenSource localCancel = new CancellationTokenSource();
            scanCancelSource = localCancel;
            CancellationToken token = localCancel.Token;
            Stopwatch sw = new Stopwatch();

            try
            {
                scanInProgress = true;
                scanButton.Enabled = true;
                if (scanOptionsButton != null) scanOptionsButton.Enabled = false;
                scanButton.Text = "İPTAL";

                // --- NORMAL TARAMA AKIŞI ---
                liveUiTicks = 0;
                treeView.Nodes.Clear();
                listView.Items.Clear();
                sortColumn = -1;
                sortAscending = true;
                UpdateSortArrow();
                RefreshModernScrollBars();
                aiPanel.Controls.Clear();
                totalFilesFound = 0;
                totalBytesScanned = 0;
                totalAllocatedScanned = 0;
                duplicateHardLinksSkipped = 0;
                countedHardLinks.Clear();
                liveQueued = 0;
                liveListLimited = false;
                scoreOldFileThreshold = DateTime.Now.AddYears(-1);
                currentSelectionFiles.Clear();
                currentSelectedDirectory = null;
                currentVisualNode = null;
                rootNode = null;
                ClearFilterBoxSilently();
                lock (queueLock) { pendingFiles.Clear(); }
                currentPieSlices.Clear(); sliceAngles.Clear(); hoveredSlice = -1;
                treemapTiles.Clear(); hoveredTreemapTile = -1;
                statTotal.Text = ""; statFiles.Text = ""; statTime.Text = "";
                UpdateSummaryCards("Taranıyor", "0", "-", selectedPath);

                progressBar.Style = ProgressBarStyle.Marquee;
                uiTimer.Start();

                // Ağ sürücüsü değilse sürücü tipini tespit et
                if (!isNetworkDrive)
                    DetectDriveType(selectedPath);
                else
                    statusLabel.Text = "Ağ sürücüsü - yavaş mod (2 iş parçacığı)";
                if (ultraFastMode && !isNetworkDrive)
                    statusLabel.Text += "  |  Ultra hızlı mod";
                if (ntfsTurboMode && !isNetworkDrive && !IsRunningAsAdministrator())
                    statusLabel.Text += "  |  NTFS Turbo için yönetici önerilir";

                sw.Start();
                DirectoryNode scannedRoot = await Task.Run(() => ScanRoot(selectedPath, token), token);
                token.ThrowIfCancellationRequested();
                sw.Stop();
                rootNode = scannedRoot;
                if (lastScanEngine == "NTFS Turbo")
                {
                    totalFilesFound = CountFiles(rootNode);
                    totalBytesScanned = rootNode.Size;
                    totalAllocatedScanned = rootNode.AllocatedSize;
                }

                uiTimer.Stop();
                UiTimer_Tick(null, null);

                if (this.IsDisposed) return;

                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                treeView.Nodes.Add(BuildTreeNode(rootNode));
                if (treeView.Nodes.Count > 0) treeView.Nodes[0].Expand();
                treeView.EndUpdate();
                RefreshModernScrollBars();

                BuildPieData(rootNode);
                piePanel.Tag = rootNode;
                piePanel.Invalidate();
                currentVisualNode = rootNode;
                treemapPanel.Invalidate();
                GenerateAIRecommendations(rootNode);

                if (listScrollBar != null) listScrollBar.RefreshTheme();
                liveCountLabel.Text  = string.Format("{0:N0} dosya", totalFilesFound);
                statusLabel.Text     = isNetworkDrive
                    ? "Tamamlandı  [Ağ Sürücüsü: " + networkDriveInfo + "]"
                    : "Tamamlandı  [" + lastScanEngine + "]";
                if (!string.IsNullOrEmpty(lastTurboMessage) && lastScanEngine != "NTFS Turbo")
                    statusLabel.Text += "  |  " + lastTurboMessage;
                string totalDisplay = FormatLogicalAllocated(rootNode);
                statTotal.Text       = "Toplam: " + totalDisplay;
                statFiles.Text       = string.Format("{0:N0} dosya", totalFilesFound);
                string timeDisplay = sw.Elapsed.TotalSeconds.ToString("F1") + " sn";
                if (!string.IsNullOrEmpty(lastScanEngine))
                    timeDisplay += " / " + lastScanEngine;
                statTime.Text        = "Süre: " + timeDisplay;
                UpdateSummaryCards(totalDisplay, string.Format("{0:N0}", totalFilesFound), timeDisplay, selectedPath);
                WriteScanHistory(selectedPath, rootNode.Size, rootNode.AllocatedSize, totalFilesFound, sw.Elapsed, isNetworkDrive, networkDriveInfo);
                string snapshotDelta = WriteScanSnapshot(rootNode, selectedPath, sw.Elapsed);
                if (!string.IsNullOrEmpty(snapshotDelta))
                    statusLabel.Text += "  |  " + snapshotDelta;
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                statusLabel.Text = "Tarama iptal edildi. Listede tarama anına kadar bulunan dosyalar kaldı.";
                statFiles.Text = string.Format("{0:N0} dosya bulundu", totalFilesFound);
                statTime.Text = "Süre: " + sw.Elapsed.TotalSeconds.ToString("F1") + "sn";
                UpdateSummaryCards(null, string.Format("{0:N0}", totalFilesFound), sw.Elapsed.TotalSeconds.ToString("F1") + " sn", selectedPath);
            }
            catch (Exception ex)
            {
                sw.Stop();
                statusLabel.Text = "Tarama durduruldu: " + ex.Message;
                MessageBox.Show("Tarama sırasında hata oluştu:\n\n" + ex.Message, "Tarama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                uiTimer.Stop();
                UiTimer_Tick(null, null);
                progressBar.Style = ProgressBarStyle.Blocks;
                scanButton.Text = "Yeni Tarama";
                scanButton.Enabled = true;
                if (scanOptionsButton != null) scanOptionsButton.Enabled = true;
                scanInProgress = false;
                if (scanCancelSource == localCancel)
                    scanCancelSource = null;
                localCancel.Dispose();
                RefreshModernScrollBars();
            }
        }

        private DirectoryNode FastScan(string path)
        {
            return FastScan(path, CancellationToken.None);
        }

        private DirectoryNode ScanRoot(string path, CancellationToken token)
        {
            lastScanEngine = "WinAPI";
            lastTurboMessage = "";

            if (ntfsTurboMode && !isNetworkDrive)
            {
                string message;
                DirectoryNode turboRoot = NtfsMftScanner.TryScan(path, token, scoringModel, scoreOldFileThreshold,
                    allocatedSizeEnabled, QueueTurboFile, out message);
                if (turboRoot != null)
                {
                    lastScanEngine = "NTFS Turbo";
                    lastTurboMessage = message;
                    return turboRoot;
                }
                lastTurboMessage = string.IsNullOrEmpty(message) ? "NTFS Turbo uygun değil, güvenli tarama kullanıldı." : message;
            }

            return FastScan(path, token);
        }

        private void QueueTurboFile(FileNode file)
        {
            if (file == null) return;
            QueueLiveFile(file);
        }

        private DirectoryNode FastScan(string path, CancellationToken token)
        {
            return FastScan(path, 0, token);
        }

        // ── Özyinelemeli WinAPI Tarama (FastScan) ────────────────────────────
        // Win32 FindFirstFile/FindNextFile API'leri ile dizin ağacını tarar.
        // .NET Directory.GetFiles() yerine doğrudan kernel'e iner —
        // yaklaşık 3-5x hız artışı sağlar. Her dosya için:
        // - Boyut, değiştirme tarihi, uzantı çıkartılır
        // - Allocated size: cluster hizalaması veya GetCompressedFileSize
        // - Hard link kontrolü: aynı inode'un çift sayılması engellenir
        // - Skor hesaplanır (boyut + yaş + uzantı + konum ağırlıklı formül)
        // Parallelism: SSD'de CPU sayısına göre, HDD'de 2 thread,
        //              ağ sürücüsünde max 2 (ağ tıkanmasını önler)
        // ──────────────────────────────────────────────────────────────────
        private DirectoryNode FastScan(string path, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            DirectoryNode node = new DirectoryNode();
            string nodeName = PathText.GetFileName(path);
            node.Name = string.IsNullOrEmpty(nodeName) ? path : nodeName;
            node.Path = path;
            try
            {
                List<FastFileEntry> files;
                List<FastFileEntry> subDirs;
                FastEnumerate(path, out files, out subDirs);
                long directoryClusterSize = allocatedSizeEnabled ? GetClusterSize(path) : 4096;

                foreach (FastFileEntry entry in files)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        FileNode f = new FileNode();
                        f.Name = entry.Name; f.FullPath = entry.FullPath;
                        f.DirectoryPath = path;
                        f.Size = entry.Size; f.LastModified = entry.LastWriteTime;
                        f.Extension = PathText.GetExtension(entry.Name).ToLowerInvariant();
                        f.AllocatedSize = allocatedSizeEnabled ? GetAllocatedSize(entry.FullPath, entry.Size, entry.Attributes, directoryClusterSize) : entry.Size;
                        if (f.AllocatedSize <= 0) f.AllocatedSize = f.Size;
                        f.CountedSize = f.Size;
                        f.CountedAllocatedSize = f.AllocatedSize;
                        ApplyHardLinkAccounting(f);
                        f.Score = scoringModel.Score(f.Size, f.LastModified, f.Extension, f.Name, f.FullPath, scoreOldFileThreshold);
                        node.Files.Add(f);
                        node.Size += f.CountedSize;
                        node.AllocatedSize += f.CountedAllocatedSize;
                        node.FileCount++;
                        Interlocked.Increment(ref totalFilesFound);
                        Interlocked.Add(ref totalBytesScanned, f.CountedSize);
                        Interlocked.Add(ref totalAllocatedScanned, f.CountedAllocatedSize);
                        QueueLiveFile(f);
                    }
                    catch { }
                }

                // Ağ sürücüsü: max 2 thread (ağ tıkanmaması için)
                // SSD: tam paralel (CPU sayısı kadar)
                // HDD: 2 thread (kafa çarpışması önleme)
                int deg = GetScanDegree(depth);
                bool useParallel = ShouldParallelize(subDirs.Count, depth);

                DirectoryNode[] children = new DirectoryNode[subDirs.Count];
                if (useParallel)
                {
                    Parallel.For(0, subDirs.Count, new ParallelOptions { MaxDegreeOfParallelism = deg, CancellationToken = token }, i =>
                    {
                        token.ThrowIfCancellationRequested();
                        children[i] = FastScan(subDirs[i].FullPath, depth + 1, token);
                    });
                }
                else
                {
                    for (int i = 0; i < subDirs.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        children[i] = FastScan(subDirs[i].FullPath, depth + 1, token);
                    }
                }

                foreach (DirectoryNode child in children)
                {
                    if (child == null) continue;
                    node.SubDirectories.Add(child);
                    node.Size += child.Size;
                    node.AllocatedSize += child.AllocatedSize;
                    node.FileCount += child.FileCount > 0 ? child.FileCount : CountFiles(child);
                    if (child.FilesArePartial) node.FilesArePartial = true;
                }
            }
            catch (OperationCanceledException) { throw; }
            catch { }
            return node;
        }

        private int GetScanDegree(int depth)
        {
            if (isNetworkDrive) return 2;
            if (isSSD)
            {
                int cpu = Math.Max(2, Environment.ProcessorCount);
                return ultraFastMode ? Math.Min(cpu * 2, 16) : Math.Min(cpu, 8);
            }
            return ultraFastMode && depth == 0 ? 3 : 2;
        }

        private bool ShouldParallelize(int subDirectoryCount, int depth)
        {
            if (subDirectoryCount <= 1) return false;
            if (isNetworkDrive) return depth == 0;
            if (isSSD) return ultraFastMode ? depth < 3 : depth < 2;
            return depth == 0;
        }

        private void FastEnumerate(string path, out List<FastFileEntry> files, out List<FastFileEntry> directories)
        {
            files = new List<FastFileEntry>();
            directories = new List<FastFileEntry>();
            string pattern;
            try { pattern = CombineChildPath(path, "*"); }
            catch { return; }

            Win32FindData data;
            IntPtr handle = NativeFileApi.FindFirstFile(NativeFileApi.ToExtendedPath(pattern), out data);
            if (handle == NativeFileApi.InvalidHandleValue)
                return;

            try
            {
                do
                {
                    string name = data.cFileName;
                    if (string.IsNullOrEmpty(name) || name == "." || name == "..")
                        continue;

                    FileAttributes attrs = (FileAttributes)data.dwFileAttributes;
                    bool isDir = (attrs & FileAttributes.Directory) == FileAttributes.Directory;
                    if (isDir && (attrs & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        continue;

                    string fullPath;
                    try { fullPath = CombineChildPath(path, name); }
                    catch { continue; }

                    FastFileEntry entry = new FastFileEntry();
                    entry.Name = name;
                    entry.FullPath = fullPath;
                    entry.IsDirectory = isDir;
                    entry.Attributes = attrs;
                    entry.Size = isDir ? 0 : (((long)data.nFileSizeHigh << 32) + data.nFileSizeLow);
                    entry.LastWriteTime = FileTimeToLocalDateTime(data.ftLastWriteTimeHigh, data.ftLastWriteTimeLow);

                    if (isDir)
                        directories.Add(entry);
                    else
                        files.Add(entry);
                }
                while (NativeFileApi.FindNextFile(handle, out data));
            }
            finally
            {
                NativeFileApi.FindClose(handle);
            }
        }

        private string CombineChildPath(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent)) return child;
            char last = parent[parent.Length - 1];
            if (last == Path.DirectorySeparatorChar || last == Path.AltDirectorySeparatorChar)
                return parent + child;
            return parent + Path.DirectorySeparatorChar + child;
        }

        private DateTime FileTimeToLocalDateTime(uint high, uint low)
        {
            try
            {
                long fileTime = ((long)high << 32) | low;
                if (fileTime <= 0) return DateTime.MinValue;
                return DateTime.FromFileTimeUtc(fileTime).ToLocalTime();
            }
            catch { return DateTime.MinValue; }
        }

        // ── Dosya Boyut Hesaplama: Gerçek Disk Alanı (Allocated Size) ─────
        // Mantıksal boyut (logical) ile diskte kaplanan alan (allocated) farklıdır.
        // Cluster hizalaması: ((boyut + cluster - 1) / cluster) * cluster
        // Sıkıştırılmış/sparse dosyalarda GetCompressedFileSize API'si kullanılır
        // çünkü gerçek disk tabanı cluster hesabından çok farklı olabilir.
        // ──────────────────────────────────────────────────────────────────
        private long GetAllocatedSize(string path, long logicalSize, FileAttributes attributes, long clusterSize)
        {
            if (logicalSize <= 0) return 0;

            bool compressedOrSparse =
                (attributes & FileAttributes.Compressed) == FileAttributes.Compressed ||
                (attributes & FileAttributes.SparseFile) == FileAttributes.SparseFile;

            if (compressedOrSparse && !isNetworkDrive)
            {
                try
                {
                    uint high;
                    uint low = NativeFileApi.GetCompressedFileSize(NativeFileApi.ToExtendedPath(path), out high);
                    int err = Marshal.GetLastWin32Error();
                    if (low != 0xFFFFFFFF || err == 0)
                    {
                        long value = ((long)high << 32) + low;
                        if (value >= 0) return value;
                    }
                }
                catch { }
            }

            if (clusterSize <= 0) clusterSize = 4096;
            return ((logicalSize + clusterSize - 1) / clusterSize) * clusterSize;
        }

        private long GetClusterSize(string path)
        {
            string root;
            try { root = Path.GetPathRoot(path); }
            catch { root = ""; }
            if (string.IsNullOrEmpty(root)) return 4096;

            lock (clusterSizeLock)
            {
                long cached;
                if (clusterSizeCache.TryGetValue(root, out cached)) return cached;
            }

            uint sectorsPerCluster, bytesPerSector, freeClusters, totalClusters;
            long size = 4096;
            try
            {
                if (NativeFileApi.GetDiskFreeSpace(root, out sectorsPerCluster, out bytesPerSector, out freeClusters, out totalClusters))
                {
                    long computed = (long)sectorsPerCluster * bytesPerSector;
                    if (computed > 0) size = computed;
                }
            }
            catch { }

            lock (clusterSizeLock)
                clusterSizeCache[root] = size;
            return size;
        }

        // ── Hard Link Çift Sayım Koruması ───────────────────────────────
        // NTFS'te hard link'ler aynı dosya verisini farklı dizin girişleriyle
        // gösterir. Toplam boyut hesabında aynı dosyanın birden fazla kez
        // sayılmasını önlemek için GetFileInformationByHandle ile dosyanın
        // benzersiz kimliği (VolumeSerial + FileIndex) okunur.
        // ConcurrentDictionary'de ilk görüleni say, sonrakileri sıfırla.
        // Performans için sadece Windows/ProgramFiles altında uygulanır.
        // ──────────────────────────────────────────────────────────────────
        private void ApplyHardLinkAccounting(FileNode file)
        {
            if (!hardLinkAccuracyEnabled || file == null || file.Size <= 0)
                return;
            if (ultraFastMode && file.Size < UltraFastHardLinkMinBytes)
                return;
            if (!ShouldReadHardLinkIdentity(file.FullPath))
                return;

            FileIdentity identity;
            if (!NativeFileApi.TryGetFileIdentity(file.FullPath, out identity))
                return;

            file.HardLinkCount = identity.LinkCount;
            file.FileIdKey = identity.Key;
            if (identity.LinkCount <= 1 || string.IsNullOrEmpty(identity.Key))
                return;

            if (!countedHardLinks.TryAdd(identity.Key, 1))
            {
                file.SharedHardLink = true;
                file.CountedSize = 0;
                file.CountedAllocatedSize = 0;
                Interlocked.Increment(ref duplicateHardLinksSkipped);
            }
        }

        private bool ShouldReadHardLinkIdentity(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            string path = fullPath.ToLowerInvariant();
            if (path.IndexOf("\\windows\\") >= 0 ||
                path.IndexOf("\\program files\\") >= 0 ||
                path.IndexOf("\\program files (x86)\\") >= 0 ||
                path.IndexOf("\\winsxs\\") >= 0)
                return true;
            return false;
        }

        private void QueueLiveFile(FileNode file)
        {
            if (liveListLimited)
                return;
            int queued = Interlocked.Increment(ref liveQueued);
            if (queued <= GetCurrentLiveListLimit())
            {
                lock (queueLock) pendingFiles.Enqueue(file);
            }
            else
            {
                liveListLimited = true;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: TREEVIEW VE LISTVIEW YARDIMCILARI (Gezinme ve Sıralama)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Dizin ağacını (TreeView) ve dosya listesini (ListView)
        //          yöneten yardımcı görsel ve mantıksal metotları içerir.
        // Yöntemi: TreeView düğümlerini dinamik (lazy-loading) yükler. 
        //          Kullanıcı düğümü genişletmeden (Expand) alt klasörler
        //          belleğe/ağaca eklenmez, böylece UI kilitlenmesi önlenir.
        //          ListView sütun tıklamalarında hızlı bellek içi sıralama
        //          (SortListView) ve boyut/tarih ayrıştırma yapar.
        // ═══════════════════════════════════════════════════════════════════
        private TreeNode BuildTreeNode(DirectoryNode node)
        {
            TreeNode tn = new TreeNode(node.Name + "  (" + FormatSize(node.Size) + ")");
            tn.Tag = node;
            if (node.SubDirectories.Count > 0)
                tn.Nodes.Add(CreateTreePlaceholder());
            return tn;
        }

        private TreeNode CreateTreePlaceholder()
        {
            TreeNode placeholder = new TreeNode("Yükleniyor...");
            placeholder.Tag = null;
            return placeholder;
        }

        private bool HasTreePlaceholder(TreeNode node)
        {
            return node != null && node.Nodes.Count == 1 && node.Nodes[0].Tag == null;
        }

        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e == null || e.Node == null || !HasTreePlaceholder(e.Node)) return;
            DirectoryNode node = e.Node.Tag as DirectoryNode;
            if (node == null) return;

            e.Node.Nodes.Clear();
            foreach (DirectoryNode sub in node.SubDirectories.OrderByDescending(s => s.Size))
                e.Node.Nodes.Add(BuildTreeNode(sub));
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (sortColumn == e.Column)
                sortAscending = !sortAscending;
            else
            {
                sortColumn = e.Column;
                sortAscending = true;
            }
            SortListView();
        }

        private void SortListView()
        {
            if (listView.Items.Count == 0)
            {
                UpdateSortArrow();
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in listView.Items)
                items.Add(item);

            items.Sort(delegate(ListViewItem a, ListViewItem b)
            {
                string va = a.SubItems.Count > sortColumn ? a.SubItems[sortColumn].Text : "";
                string vb = b.SubItems.Count > sortColumn ? b.SubItems[sortColumn].Text : "";
                int result;

                if (sortColumn == 1 || sortColumn == 2)
                    result = ParseSize(va).CompareTo(ParseSize(vb));
                else if (sortColumn == 3)
                {
                    int ia = 0, ib = 0;
                    int.TryParse(va, out ia);
                    int.TryParse(vb, out ib);
                    result = ia.CompareTo(ib);
                }
                else if (sortColumn == 4)
                {
                    DateTime da = DateTime.MinValue;
                    DateTime db = DateTime.MinValue;
                    DateTime.TryParseExact(va, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out da);
                    DateTime.TryParseExact(vb, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out db);
                    result = da.CompareTo(db);
                }
                else
                    result = string.Compare(va, vb, StringComparison.OrdinalIgnoreCase);

                return sortAscending ? result : -result;
            });

            listView.BeginUpdate();
            listView.Items.Clear();
            if (items.Count > 0)
                listView.Items.AddRange(items.ToArray());
            listView.EndUpdate();

            UpdateSortArrow();
            if (listScrollBar != null) listScrollBar.RefreshTheme();
        }

        private double ParseSize(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            string s = value.Trim();
            double factor = 1;

            if (s.EndsWith(" GB", StringComparison.OrdinalIgnoreCase))
            {
                factor = 1024.0 * 1024 * 1024;
                s = s.Substring(0, s.Length - 3).Trim();
            }
            else if (s.EndsWith(" MB", StringComparison.OrdinalIgnoreCase))
            {
                factor = 1024.0 * 1024;
                s = s.Substring(0, s.Length - 3).Trim();
            }
            else if (s.EndsWith(" KB", StringComparison.OrdinalIgnoreCase))
            {
                factor = 1024.0;
                s = s.Substring(0, s.Length - 3).Trim();
            }
            else if (s.EndsWith(" B", StringComparison.OrdinalIgnoreCase))
            {
                factor = 1;
                s = s.Substring(0, s.Length - 2).Trim();
            }

            double number = 0;
            s = s.Replace(",", ".");
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
            return number * factor;
        }

        private void UpdateSortArrow()
        {
            if (listView == null) return;
            for (int i = 0; i < listView.Columns.Count && i < listColumnTitles.Length; i++)
            {
                string arrow = "";
                if (i == sortColumn)
                    arrow = sortAscending ? " \u25B2" : " \u25BC";
                listView.Columns[i].Text = listColumnTitles[i] + arrow;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: FİLTRELEME VE ANALİZ DÜĞMELERİ (Arama ve Keşif)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Dosya listesinde anlık filtreleme yapılmasını ve
        //          en büyük dosyalar/kopyaların listelenmesini sağlar.
        // Yöntemi: TextChanged olayı 300ms gecikmeli bir zamanlayıcı
        //          (FilterTimer_Tick) tetikler. Kullanıcı yazmayı bıraktığı
        //          anda asenkron Task üzerinde LINQ filtrelemesi çalışır.
        //          TopFiles/Duplicates butonları tüm ağacı asenkron
        //          gezip sıralayarak en büyük/kopya dosyaları getirir.
        // ═══════════════════════════════════════════════════════════════════
        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            if (suppressFilterTextChanged) return;
            if (filterTimer == null) return;
            filterTimer.Stop();
            filterTimer.Start();
        }

        private void ClearFilterBoxSilently()
        {
            if (filterBox == null) return;
            suppressFilterTextChanged = true;
            filterBox.Text = "";
            suppressFilterTextChanged = false;
            if (filterTimer != null) filterTimer.Stop();
        }

        private async void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
            if (currentSelectionFiles == null || currentSelectionFiles.Count == 0) return;

            int version = Interlocked.Increment(ref treeSelectionVersion);
            string filter = filterBox.Text.Trim().ToLowerInvariant();
            List<FileNode> source = currentSelectionFiles;
            statusLabel.Text = string.IsNullOrEmpty(filter) ? "Liste yenileniyor..." : "Filtre uygulaniyor...";

            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                IEnumerable<FileNode> query = source;
                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(f =>
                        (f.Name != null && f.Name.ToLowerInvariant().Contains(filter)) ||
                        (f.Extension != null && f.Extension.ToLowerInvariant().Contains(filter)) ||
                        (f.FullPath != null && f.FullPath.ToLowerInvariant().Contains(filter)));
                }
                return query.OrderByDescending(f => f.Score).ThenByDescending(f => f.Size).ToList();
            });

            if (version != treeSelectionVersion || this.IsDisposed) return;
            await PopulateListViewAsync(files, version);
            statusLabel.Text = string.IsNullOrEmpty(filter)
                ? string.Format("{0:N0} dosya listelendi", files.Count)
                : string.Format("{0:N0} eslesme", files.Count);
        }

        private async void TopFilesButton_Click(object sender, EventArgs e)
        {
            if (rootNode == null) return;
            ClearFilterBoxSilently();
            int version = Interlocked.Increment(ref treeSelectionVersion);
            statusLabel.Text = "En büyük 100 dosya hazırlanıyor...";
            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                List<FileNode> all = new List<FileNode>();
                CollectAllFiles(rootNode, all);
                return all.OrderByDescending(f => f.Size).Take(100).ToList();
            });
            if (version != treeSelectionVersion || this.IsDisposed) return;
            currentSelectionFiles = files;
            currentSelectedDirectory = rootNode;
            await PopulateListViewAsync(files, version);
            statusLabel.Text = "En büyük 100 dosya";
        }

        private async void DuplicatesButton_Click(object sender, EventArgs e)
        {
            if (rootNode == null) return;
            ClearFilterBoxSilently();
            int version = Interlocked.Increment(ref treeSelectionVersion);
            statusLabel.Text = "Olası kopyalar hazırlanıyor...";
            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                List<FileNode> all = new List<FileNode>();
                CollectAllFiles(rootNode, all);
                List<DuplicateGroup> groups = BuildDuplicateGroups(all);
                List<FileNode> flat = new List<FileNode>();
                foreach (DuplicateGroup group in groups)
                    flat.AddRange(group.Files);
                return flat;
            });
            if (version != treeSelectionVersion || this.IsDisposed) return;
            currentSelectionFiles = files;
            currentSelectedDirectory = rootNode;
            await PopulateListViewAsync(files, version);
            statusLabel.Text = string.Format("{0:N0} olası kopya dosya listelendi", files.Count);
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: DIŞA AKTARIM VE TARAMA GEÇMİŞİ (Veri Raporlama)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Tarama sonuçlarını CSV olarak dışa aktarır ve yerel log
        //          sisteminde (CSV/TSV logları) tarama geçmişini tutar.
        // Yöntemi: StreamWriter kullanarak UTF-8 BOM ile ham CSV formatı
        //          üretir. WriteScanSnapshot metodu, aynı klasörün önceki
        //          taramalarıyla kıyaslama yaparak delta boyutu hesaplar.
        // Notlar : Loglar %LOCALAPPDATA%\AdvancedDiskAnalyzer\Logs altında
        //          scan-history.csv ve snapshots.tsv olarak saklanır.
        // ═══════════════════════════════════════════════════════════════
        private void CsvButton_Click(object sender, EventArgs e)
        {
            if (rootNode == null)
            {
                MessageBox.Show("Önce bir tarama yapın.", "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "CSV Dışa Aktar";
            dialog.Filter = "CSV dosyası (*.csv)|*.csv";
            dialog.FileName = "AdvancedDiskAnalyzer_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                List<FileNode> all = new List<FileNode>();
                CollectAllFiles(rootNode, all);
                using (StreamWriter writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Ad,BoyutBayt,Boyut,DiskteBayt,Diskte,Skor,SonDegisiklik,Uzanti,TamYol,HardLinkNotu");
                    foreach (FileNode f in all.OrderByDescending(f => f.Size))
                    {
                        long allocated = f.AllocatedSize > 0 ? f.AllocatedSize : f.Size;
                        writer.WriteLine(string.Join(",", new string[]
                        {
                            Csv(f.Name),
                            f.Size.ToString(CultureInfo.InvariantCulture),
                            Csv(FormatSize(f.Size)),
                            allocated.ToString(CultureInfo.InvariantCulture),
                            Csv(FormatSize(allocated)),
                            f.Score.ToString(CultureInfo.InvariantCulture),
                            Csv(f.LastModified.ToString("yyyy-MM-dd")),
                            Csv(f.Extension),
                            Csv(f.FullPath),
                            Csv(f.SharedHardLink ? "Hard link - tek sayıldı" : "")
                        }));
                    }
                }
                MessageBox.Show("CSV oluşturuldu:\n" + dialog.FileName, "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV oluşturulamadı:\n" + ex.Message, "CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string Csv(string value)
        {
            if (value == null) value = "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void OpenScanHistory_Click(object sender, EventArgs e)
        {
            try
            {
                string path = GetScanHistoryPath();
                EnsureScanHistoryFile(path);
                Process.Start("notepad.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarama geçmişi açılamadı:\n" + ex.Message, "Tarama Geçmişi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenSnapshotHistory_Click(object sender, EventArgs e)
        {
            try
            {
                string path = GetSnapshotHistoryPath();
                EnsureSnapshotHistoryFile(path);
                Process.Start("notepad.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Snapshot geçmişi açılamadı:\n" + ex.Message, "Snapshot Geçmişi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void WriteScanHistory(string scanPath, long totalBytes, long totalAllocatedBytes, int fileCount, TimeSpan duration, bool networkScan, string networkInfo)
        {
            try
            {
                string path = GetScanHistoryPath();
                EnsureScanHistoryFile(path);
                using (StreamWriter writer = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    writer.WriteLine(string.Join(",", new string[]
                    {
                        Csv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                        Csv(Environment.UserName),
                        Csv(Environment.MachineName),
                        Csv(scanPath),
                        totalBytes.ToString(CultureInfo.InvariantCulture),
                        Csv(FormatSize(totalBytes)),
                        totalAllocatedBytes.ToString(CultureInfo.InvariantCulture),
                        Csv(FormatSize(totalAllocatedBytes)),
                        duplicateHardLinksSkipped.ToString(CultureInfo.InvariantCulture),
                        fileCount.ToString(CultureInfo.InvariantCulture),
                        duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),
                        Csv(networkScan ? "Ağ" : "Yerel"),
                        Csv(networkInfo),
                        Csv(ultraFastMode ? "Ultra Hızlı" : "Standart"),
                        Csv(currentLicense != null ? ToTurkishPlanName(currentLicense.PlanName) : "Ücretsiz"),
                        Csv(AppVersion)
                    }));
                }
            }
            catch { }
        }

        private string GetScanHistoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AdvancedDiskAnalyzer", "Logs", "scan-history.csv");
        }

        private void EnsureScanHistoryFile(string path)
        {
            string dir = PathText.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Zaman,Kullanıcı,Makine,TaramaYolu,ToplamBayt,ToplamBoyut,DiskteBayt,DiskteBoyut,HardLinkDuzeltmesi,DosyaSayısı,SüreSaniye,TaramaTürü,AğBilgisi,Mod,Plan,Sürüm");
                }
            }
        }

        private string WriteScanSnapshot(DirectoryNode root, string scanPath, TimeSpan duration)
        {
            try
            {
                if (root == null) return "";
                string path = GetSnapshotHistoryPath();
                EnsureSnapshotHistoryFile(path);

                long previousSize = 0;
                long previousAllocated = 0;
                ReadPreviousSnapshot(path, scanPath, out previousSize, out previousAllocated);

                string largestFolder = "";
                long largestFolderSize = 0;
                DirectoryNode largest = root.SubDirectories.OrderByDescending(d => d.Size).FirstOrDefault();
                if (largest != null)
                {
                    largestFolder = largest.Path;
                    largestFolderSize = largest.Size;
                }

                using (StreamWriter writer = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    writer.WriteLine(string.Join("\t", new string[]
                    {
                        Tsv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                        Tsv(Environment.UserName),
                        Tsv(Environment.MachineName),
                        Tsv(scanPath),
                        root.Size.ToString(CultureInfo.InvariantCulture),
                        root.AllocatedSize.ToString(CultureInfo.InvariantCulture),
                        totalFilesFound.ToString(CultureInfo.InvariantCulture),
                        duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),
                        duplicateHardLinksSkipped.ToString(CultureInfo.InvariantCulture),
                        Tsv(largestFolder),
                        largestFolderSize.ToString(CultureInfo.InvariantCulture),
                        Tsv(currentLicense != null ? ToTurkishPlanName(currentLicense.PlanName) : "Ücretsiz")
                    }));
                }

                if (previousSize <= 0) return "İlk snapshot kaydı";
                long delta = root.Size - previousSize;
                if (delta == 0) return "Önceki snapshot ile aynı";
                return "Önceki snapshot: " + FormatSignedSize(delta);
            }
            catch { return ""; }
        }

        private string FormatSignedSize(long value)
        {
            if (value == 0) return FormatSize(0);
            long absolute = value == long.MinValue ? long.MaxValue : Math.Abs(value);
            return (value > 0 ? "+" : "-") + FormatSize(absolute);
        }

        private string FormatLogicalAllocated(DirectoryNode node)
        {
            if (node == null) return FormatSize(0);
            return FormatLogicalAllocated(node.Size, node.AllocatedSize);
        }

        private string FormatLogicalAllocated(long logicalSize, long allocatedSize)
        {
            if (allocatedSize > 0 && allocatedSize != logicalSize)
                return FormatSize(logicalSize) + " / " + FormatSize(allocatedSize) + " diskte";
            return FormatSize(logicalSize);
        }

        private void ReadPreviousSnapshot(string filePath, string scanPath, out long previousSize, out long previousAllocated)
        {
            previousSize = 0;
            previousAllocated = 0;
            try
            {
                if (!File.Exists(filePath)) return;
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                for (int i = lines.Length - 1; i >= 1; i--)
                {
                    string[] parts = lines[i].Split('\t');
                    if (parts.Length < 6) continue;
                    if (!string.Equals(parts[3], scanPath, StringComparison.OrdinalIgnoreCase)) continue;
                    long.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out previousSize);
                    long.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out previousAllocated);
                    return;
                }
            }
            catch { }
        }

        private string GetSnapshotHistoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AdvancedDiskAnalyzer", "Logs", "snapshots.tsv");
        }

        private void EnsureSnapshotHistoryFile(string path)
        {
            string dir = PathText.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Zaman\tKullanıcı\tMakine\tTaramaYolu\tToplamBayt\tDiskteBayt\tDosyaSayısı\tSüreSaniye\tHardLinkDüzeltmesi\tEnBüyükKlasör\tEnBüyükKlasörBayt\tPlan");
                }
            }
        }

        private string Tsv(string value)
        {
            if (value == null) value = "";
            return value.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: ASENKRON ARAYÜZ YENİLEME VE LİSTELEME (Seçim ve Doldurma)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : TreeView üzerinde bir klasör seçildiğinde, o klasörün
        //          içeriğini ListView'e asenkron ve parça parça (batch) doldurur.
        // Yöntemi: Her seçimde bir treeSelectionVersion arttırılır. Eğer yeni
        //          bir seçim yapılırsa, eski listeleme görevi (PopulateListViewAsync)
        //          sürüm eşleşmesinden dolayı iptal edilir (concurrency control).
        //          Büyük dosya listeleri Task.Delay(1) ile UI thread'i
        //          bloklamadan akıcı bir şekilde eklenir.
        // ═══════════════════════════════════════════════════════════════════
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sidebarNavPanel == null || tabControl == null) return;
            Button target = null;
            if (tabControl.SelectedIndex == 0 || tabControl.SelectedIndex == 1)
            {
                target = FindSidebarButtonByText("Genel Bakış");
            }
            else if (tabControl.SelectedIndex == 2)
            {
                target = FindSidebarButtonByText("Analizler");
            }
            if (target != null)
            {
                SetActiveSidebarButton(target);
            }
        }

        private Button FindSidebarButtonByText(string text)
        {
            if (sidebarNavPanel == null) return null;
            foreach (Control c in sidebarNavPanel.Controls)
            {
                Button btn = c as Button;
                if (btn != null && btn.Text == text)
                    return btn;
            }
            return null;
        }

        private void SetActiveSidebarButton(Button activeBtn)
        {
            if (activeBtn == null || sidebarNavPanel == null) return;
            foreach (Control c in sidebarNavPanel.Controls)
            {
                Button btn = c as Button;
                if (btn != null)
                {
                    bool isActive = (btn == activeBtn);
                    btn.Tag = isActive ? "nav-active" : "nav";
                    btn.Font = new Font("Segoe UI", 9, isActive ? FontStyle.Bold : FontStyle.Regular);
                    btn.FlatAppearance.BorderSize = isActive ? 1 : 0;
                }
            }
            ThemeSidebarChildren(sidebarNavPanel);
        }

        private async void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DirectoryNode node = e.Node.Tag as DirectoryNode;
            if (node == null) return;

            int version = Interlocked.Increment(ref treeSelectionVersion);
            currentSelectedDirectory = node;
            currentVisualNode = node;
            ClearFilterBoxSilently();

            Button target = FindSidebarButtonByText("Dosyalar");
            if (target != null) SetActiveSidebarButton(target);
            statusLabel.Text = "Klasör içeriği hazırlanıyor...";

            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                List<FileNode> result = new List<FileNode>();
                CollectAllFiles(node, result);
                return result.OrderByDescending(f => f.Score).ThenByDescending(f => f.Size).ToList();
            });

            if (version != treeSelectionVersion || this.IsDisposed) return;

            currentSelectionFiles = files;
            await PopulateListViewAsync(files, version);
            if (version != treeSelectionVersion || this.IsDisposed) return;

            BuildPieData(node);
            piePanel.Tag = node;
            piePanel.Invalidate();
            treemapPanel.Invalidate();
            if (node.FilesArePartial)
            {
                statusLabel.Text = node.Name + "  |  " + string.Format("{0:N0}", CountFiles(node)) +
                    " dosya, Turbo hızlı önizlemede " + string.Format("{0:N0}", files.Count) + " öncelikli kayıt gösteriliyor";
            }
            else
            {
                statusLabel.Text = node.Name + "  |  " + string.Format("{0:N0}", files.Count) +
                    (files.Count > DisplayFileLimit ? " dosya, ilk " + string.Format("{0:N0}", DisplayFileLimit) + " gösteriliyor" : " dosya listelendi");
            }
            RefreshModernScrollBars();
        }

        private async Task PopulateListViewAsync(List<FileNode> files, int version)
        {
            listView.BeginUpdate();
            listView.Items.Clear();
            listView.EndUpdate();

            int index = 0;
            const int batchSize = 500;
            int total = files.Count;
            int displayCount = Math.Min(total, DisplayFileLimit);
            while (index < displayCount)
            {
                if (version != treeSelectionVersion || this.IsDisposed) return;

                int end = Math.Min(displayCount, index + batchSize);
                listView.BeginUpdate();
                List<ListViewItem> batchItems = new List<ListViewItem>(end - index);
                for (int i = index; i < end; i++)
                    batchItems.Add(CreateFileListItem(files[i]));
                if (batchItems.Count > 0)
                    listView.Items.AddRange(batchItems.ToArray());
                listView.EndUpdate();

                index = end;
                if (listScrollBar != null) listScrollBar.RefreshTheme();
                if (index < displayCount)
                    await Task.Delay(1);
            }

            if (sortColumn >= 0)
                SortListView();
            else if (listScrollBar != null)
                listScrollBar.RefreshTheme();
        }

        private ListViewItem CreateFileListItem(FileNode file)
        {
            ListViewItem item = new ListViewItem(file.Name);
            item.SubItems.Add(FormatSize(file.Size));
            item.SubItems.Add(FormatSize(file.AllocatedSize > 0 ? file.AllocatedSize : file.Size));
            item.SubItems.Add(file.Score.ToString());
            item.SubItems.Add(file.LastModified.ToString("yyyy-MM-dd"));
            item.SubItems.Add(file.Extension);
            string location = !string.IsNullOrEmpty(file.DirectoryPath) ? file.DirectoryPath : PathText.GetDirectoryName(file.FullPath);
            if (file.SharedHardLink) location = "[Hard link - tek sayıldı] " + location;
            item.SubItems.Add(location);
            item.Tag = file.FullPath;

            item.BackColor = Theme.Surface;
            item.ForeColor = Theme.Text;
            return item;
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: SAĞ TIK BAĞLAM MENÜSÜ (Dosya İşlemleri)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : ListView üzerindeki öğelere sağ tıklandığında açılan
        //          Explorer'da göster, yolu kopyala ve sil seçeneklerini yönetir.
        // Yöntemi: Windows Geri Dönüşüm Kutusu API'sini (SHFileOperation)
        //          kullanarak dosyaları güvenli bir şekilde siler. Silinen
        //          öğeler bellek içi ağaç modelinden de çıkartılarak UI güncellenir.
        // ═══════════════════════════════════════════════════════════════════
        private void ContextMenu_Open(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            string path = listView.SelectedItems[0].Tag as string; if (path == null) return;
            try { Process.Start("explorer.exe", "/select,\"" + path + "\""); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void ContextMenu_CopyPath(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            string path = listView.SelectedItems[0].Tag as string;
            if (path != null) Clipboard.SetText(path);
        }
        private void ContextMenu_Delete(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;
            string path = listView.SelectedItems[0].Tag as string; if (path == null) return;
            string name = PathText.GetFileName(path);
            if (MessageBox.Show("Geri Dönüşüm Kutusu'na taşınsın mı?\n\n" + name,
                "Güvenli Silme", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    FileNode modelFile = FindFileNode(rootNode, path);
                    long freed = EstimateFreedSize(modelFile, path);
                    MoveFileToRecycleBin(path);
                    HashSet<string> deleted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    deleted.Add(path);
                    ApplyDeletedFilesToModel(deleted, freed);
                    statusLabel.Text = name + " Geri Dönüşüm Kutusu'na taşındı. Kazanç: " + FormatSize(freed);
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: PASTA GRAFİK GÖRSELLEŞTİRME (Pie Chart Render)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Seçilen klasörün alt dizin boyut dağılımlarını görselleştirir.
        // Yöntemi: GDI+ kütüphanesi kullanarak piePanel üzerinde Paint olayı ile
        //          çizim yapar. Açı hesaplaması (sliceAngles) toplam boyuta oranla
        //          yapılır. Fare koordinat takibi ile dilimlerin üzerine gelindiğinde
        //          (hover) dilim dışarı kayar ve detay kartı (tooltip) çizilir.
        // ═══════════════════════════════════════════════════════════════════
        private Color[] pieColors = new Color[]
        {
            Color.FromArgb(90, 205, 255),
            Color.FromArgb(175, 95, 255),
            Color.FromArgb(75, 215, 155),
            Color.FromArgb(255, 185, 55),
            Color.FromArgb(255, 85, 115),
            Color.FromArgb(55, 195, 195),
            Color.FromArgb(255, 135, 195),
            Color.FromArgb(135, 255, 95),
        };

        private void BuildPieData(DirectoryNode node)
        {
            currentPieSlices.Clear(); sliceAngles.Clear();
            if (node == null || node.Size == 0) return;
            var dirs = node.SubDirectories.OrderByDescending(d => d.Size).Take(7).ToList();
            long filesSize = node.Files.Sum(f => f.Size);
            foreach (var d in dirs)
                currentPieSlices.Add(new KeyValuePair<string, long>(d.Name, d.Size));
            if (filesSize > 0)
                currentPieSlices.Add(new KeyValuePair<string, long>("[Dosyalar]", filesSize));
            float start = -90f;
            foreach (var s in currentPieSlices)
            {
                float sweep = (float)(s.Value / (double)node.Size * 360.0);
                sliceAngles.Add(new float[] { start, sweep });
                start += sweep;
            }
        }

        private void PiePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int w = piePanel.Width, h = piePanel.Height;
            g.Clear(Theme.Bg);

            g.DrawString("Boyut Dagilimi",
                new Font("Segoe UI", 11, FontStyle.Bold),
                new SolidBrush(Theme.Text), new PointF(16, 14));

            if (currentPieSlices.Count == 0)
            {
                g.DrawString("Henüz tarama yapılmadı.", new Font("Segoe UI", 9),
                    new SolidBrush(Theme.SubText), new PointF(16, 50));
                return;
            }

            int legendH  = currentPieSlices.Count * 24 + 12;
            int pieSize  = Math.Max(80, Math.Min(w - 60, h - legendH - 72));
            int pieX     = (w - pieSize) / 2;
            int pieY     = 44;
            Rectangle pieRect = new Rectangle(pieX, pieY, pieSize, pieSize);
            int cx = pieX + pieSize / 2, cy = pieY + pieSize / 2;

            for (int i = 0; i < currentPieSlices.Count; i++)
            {
                float start = sliceAngles[i][0], sweep = sliceAngles[i][1];
                bool hov = (i == hoveredSlice);
                Color c = pieColors[i % pieColors.Length];
                Color fc = hov ? Color.FromArgb(
                    Math.Min(255, c.R + 45),
                    Math.Min(255, c.G + 45),
                    Math.Min(255, c.B + 45)) : c;

                Rectangle dr = pieRect;
                if (hov)
                {
                    double mid = (start + sweep / 2.0) * Math.PI / 180.0;
                    dr = new Rectangle(pieX + (int)(Math.Cos(mid) * 9),
                                       pieY + (int)(Math.Sin(mid) * 9),
                                       pieSize, pieSize);
                }
                using (SolidBrush b = new SolidBrush(fc)) g.FillPie(b, dr, start, sweep);
                using (Pen p = new Pen(Theme.Bg, hov ? 3 : 2))  g.DrawPie(p, dr, start, sweep);
            }

            // ── Donut Boşluğu Çizimi ──────────────────────────────────────────
            int holeSize = (int)(pieSize * 0.58);
            Rectangle holeRect = new Rectangle(cx - holeSize / 2, cy - holeSize / 2, holeSize, holeSize);
            using (SolidBrush bgBrush = new SolidBrush(Theme.Bg))
                g.FillEllipse(bgBrush, holeRect);
            using (Pen borderPen = new Pen(Theme.Border, 1))
                g.DrawEllipse(borderPen, holeRect);

            // Merkez Bilgi Metinleri
            string labelLine = "";
            string sizeLine = "";
            string pctLine = "";
            Color labelColor = Theme.SubText;
            Color sizeColor = Theme.Text;
            Color pctColor = Theme.SubText;

            if (hoveredSlice >= 0 && hoveredSlice < currentPieSlices.Count)
            {
                var sl = currentPieSlices[hoveredSlice];
                long tot = currentPieSlices.Sum(s => s.Value);
                double pct = sl.Value / (double)tot * 100.0;

                labelLine = sl.Key;
                if (labelLine.Length > 12) labelLine = labelLine.Substring(0, 10) + "...";
                sizeLine = FormatSize(sl.Value);
                pctLine = pct.ToString("F1") + "%";
                labelColor = pieColors[hoveredSlice % pieColors.Length]; // Slice rengini kullanarak vurgula
            }
            else
            {
                long tot = currentPieSlices.Sum(s => s.Value);
                labelLine = "Toplam Boyut";
                sizeLine = FormatSize(tot);
                pctLine = string.Format("{0:N0} dosya", totalFilesFound);
            }

            Font fontLabel = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Font fontSize = new Font("Segoe UI", 11f, FontStyle.Bold);
            Font fontPct = new Font("Segoe UI", 8f, FontStyle.Regular);

            SizeF szLabel = g.MeasureString(labelLine, fontLabel);
            SizeF szSize = g.MeasureString(sizeLine, fontSize);
            SizeF szPct = g.MeasureString(pctLine, fontPct);

            float spacing = 2;
            float totalHeight = szLabel.Height + szSize.Height + szPct.Height + (spacing * 2);
            float startY = cy - totalHeight / 2;

            // Satır 1: Başlık / Tip
            float x1 = cx - szLabel.Width / 2;
            using (SolidBrush br = new SolidBrush(labelColor))
                g.DrawString(labelLine, fontLabel, br, x1, startY);

            // Satır 2: Boyut
            float y2 = startY + szLabel.Height + spacing;
            float x2 = cx - szSize.Width / 2;
            using (SolidBrush br = new SolidBrush(sizeColor))
                g.DrawString(sizeLine, fontSize, br, x2, y2);

            // Satır 3: Yüzde / Dosya Sayısı
            float y3 = y2 + szSize.Height + spacing;
            float x3 = cx - szPct.Width / 2;
            using (SolidBrush br = new SolidBrush(pctColor))
                g.DrawString(pctLine, fontPct, br, x3, y3);

            int legendY = pieY + pieSize + 16;
            long total = currentPieSlices.Sum(s => s.Value);
            for (int i = 0; i < currentPieSlices.Count; i++)
            {
                if (legendY + 20 > h) break;
                bool hov = (i == hoveredSlice);
                Color lc = pieColors[i % pieColors.Length];
                if (hov)
                    using (SolidBrush hb = new SolidBrush(Color.FromArgb(35, lc)))
                        g.FillRectangle(hb, 8, legendY - 2, w - 16, 22);

                g.FillRectangle(new SolidBrush(lc), 14, legendY + 5, 11, 11);
                double p2 = currentPieSlices[i].Value / (double)total * 100.0;
                string lbl = currentPieSlices[i].Key + "  -  " + FormatSize(currentPieSlices[i].Value) + "  (" + p2.ToString("F1") + "%)";
                if (lbl.Length > 54) lbl = lbl.Substring(0, 51) + "...";
                g.DrawString(lbl,
                    new Font("Segoe UI", hov ? 9 : 8, hov ? FontStyle.Bold : FontStyle.Regular),
                    new SolidBrush(hov ? Theme.Text : Theme.SubText), 30, legendY + 2);
                legendY += 24;
            }
        }

        private void PiePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentPieSlices.Count == 0) return;
            int w = piePanel.Width, h = piePanel.Height;
            int legendH = currentPieSlices.Count * 24 + 12;
            int pieSize = Math.Max(80, Math.Min(w - 60, h - legendH - 72));
            int pieX = (w - pieSize) / 2, pieY = 44;
            int cx = pieX + pieSize / 2, cy = pieY + pieSize / 2;

            float dx = e.X - cx, dy = e.Y - cy;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            int newHover = -1;

            int holeSize = (int)(pieSize * 0.58);
            if (dist > holeSize / 2.0f && dist <= pieSize / 2.0f + 10)
            {
                float angle = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI) + 90;
                if (angle < 0) angle += 360;
                if (angle >= 360) angle -= 360;

                for (int i = 0; i < sliceAngles.Count; i++)
                {
                    float s = sliceAngles[i][0] + 90;
                    if (s < 0) s += 360;
                    if (s >= 360) s -= 360;
                    float end = s + sliceAngles[i][1];
                    bool inSlice = end > 360
                        ? (angle >= s || angle < end - 360)
                        : (angle >= s && angle < end);
                    if (inSlice) { newHover = i; break; }
                }
            }
            else
            {
                int legendY = pieY + pieSize + 16;
                for (int i = 0; i < currentPieSlices.Count; i++)
                {
                    if (e.Y >= legendY - 2 && e.Y < legendY + 22) { newHover = i; break; }
                    legendY += 24;
                }
            }

            if (newHover != hoveredSlice)
            {
                hoveredSlice = newHover;
                piePanel.Cursor = hoveredSlice >= 0 ? Cursors.Hand : Cursors.Default;
                piePanel.Invalidate();
            }
        }

        private void PiePanel_MouseLeave(object sender, EventArgs e)
        {
            hoveredSlice = -1;
            piePanel.Cursor = Cursors.Default;
            piePanel.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: TREEMAP GÖRSELLEŞTİRME (Alan Bazlı Dağılım Haritası)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Klasör ve dosya boyutlarını hiyerarşik dikdörtgenler halinde
        //          görselleştirerek en çok yer kaplayan alanları anında gösterir.
        // Yöntemi: Squarified veya basit bölme algoritması (BuildTreemapTiles) ile
        //          verilen alanı (area) dosya boyut oranlarına göre alt dikdörtgenlere
        //          böler. Fareyle tıklanan alt bölümler (TreemapPanel_MouseClick)
        //          ilgili dizini visual root yapar ve derinlemesine inceleme sunar.
        // ═══════════════════════════════════════════════════════════════════
        private void TreemapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.Bg);

            using (Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Theme.Text))
                g.DrawString("Treemap", titleFont, titleBrush, 16, 14);

            DirectoryNode node = currentVisualNode != null ? currentVisualNode : rootNode;
            if (node == null || node.Size <= 0)
            {
                using (Font font = new Font("Segoe UI", 9))
                using (Brush brush = new SolidBrush(Theme.SubText))
                    g.DrawString("Tarama sonrası alan haritası burada görünür.", font, brush, 16, 50);
                return;
            }

            using (Font subFont = new Font("Segoe UI", 8))
            using (Brush subBrush = new SolidBrush(Theme.SubText))
                g.DrawString(node.Name + "  /  " + FormatSize(node.Size), subFont, subBrush, 90, 18);

            Rectangle area = new Rectangle(12, 50, Math.Max(10, treemapPanel.Width - 24), Math.Max(10, treemapPanel.Height - 68));
            treemapTiles.Clear();
            List<TreemapItem> items = BuildTreemapItems(node, 36);
            BuildTreemapTiles(items, area, 0, treemapTiles);

            for (int i = 0; i < treemapTiles.Count; i++)
            {
                TreemapTile tile = treemapTiles[i];
                Rectangle r = tile.Bounds;
                if (r.Width <= 1 || r.Height <= 1) continue;

                Color baseColor = pieColors[i % pieColors.Length];
                if (i == hoveredTreemapTile)
                    baseColor = Color.FromArgb(Math.Min(255, baseColor.R + 38), Math.Min(255, baseColor.G + 38), Math.Min(255, baseColor.B + 38));

                using (SolidBrush brush = new SolidBrush(baseColor))
                    g.FillRectangle(brush, r);
                using (Pen pen = new Pen(Theme.Bg, i == hoveredTreemapTile ? 2 : 1))
                    g.DrawRectangle(pen, r);

                if (r.Width > 82 && r.Height > 34)
                {
                    string label = TruncateForReport(tile.Label, Math.Max(8, r.Width / 8));
                    using (Font font = new Font("Segoe UI", r.Height > 60 ? 8 : 7, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.White))
                         g.DrawString(label, font, brush, r.X + 5, r.Y + 4);
                }

                if (r.Width > 96 && r.Height > 54)
                {
                    using (Font font = new Font("Segoe UI", 7))
                    using (Brush brush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                        g.DrawString(FormatSize(tile.Size), font, brush, r.X + 5, r.Y + 22);
                }
            }

            if (hoveredTreemapTile >= 0 && hoveredTreemapTile < treemapTiles.Count)
            {
                TreemapTile tile = treemapTiles[hoveredTreemapTile];
                string tip = tile.Label + "  |  " + FormatSize(tile.Size);
                using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    SizeF size = g.MeasureString(tip, font);
                    Rectangle tipRect = new Rectangle(14, treemapPanel.Height - 31, (int)size.Width + 18, 22);
                    using (Brush bg = new SolidBrush(Theme.Card))
                        g.FillRectangle(bg, tipRect);
                    using (Pen pen = new Pen(Theme.Accent))
                        g.DrawRectangle(pen, tipRect);
                    using (Brush brush = new SolidBrush(Theme.Text))
                        g.DrawString(tip, font, brush, tipRect.X + 8, tipRect.Y + 4);
                }
            }
        }

        private List<TreemapItem> BuildTreemapItems(DirectoryNode node, int limit)
        {
            List<TreemapItem> items = new List<TreemapItem>();
            foreach (DirectoryNode dir in node.SubDirectories.OrderByDescending(d => d.Size).Take(limit))
            {
                if (dir.Size <= 0) continue;
                TreemapItem item = new TreemapItem();
                item.Label = dir.Name;
                item.Size = dir.Size;
                item.Directory = dir;
                items.Add(item);
            }

            long directFiles = node.Files.Sum(f => f.Size);
            if (directFiles > 0)
            {
                TreemapItem files = new TreemapItem();
                files.Label = "[Dosyalar]";
                files.Size = directFiles;
                files.Directory = node;
                items.Add(files);
            }
            return items.OrderByDescending(i => i.Size).ToList();
        }

        private void BuildTreemapTiles(List<TreemapItem> items, Rectangle area, int depth, List<TreemapTile> tiles)
        {
            if (items == null || items.Count == 0 || area.Width <= 2 || area.Height <= 2) return;
            long total = items.Sum(i => i.Size);
            if (total <= 0) return;

            bool horizontal = area.Width >= area.Height;
            int cursor = horizontal ? area.X : area.Y;
            int remainingPixels = horizontal ? area.Width : area.Height;
            long remainingSize = total;

            for (int i = 0; i < items.Count; i++)
            {
                TreemapItem item = items[i];
                int span = (i == items.Count - 1)
                    ? remainingPixels
                    : Math.Max(2, (int)Math.Round(remainingPixels * (item.Size / (double)remainingSize)));

                Rectangle r = horizontal
                    ? new Rectangle(cursor, area.Y, span, area.Height)
                    : new Rectangle(area.X, cursor, area.Width, span);

                Rectangle inner = Rectangle.Inflate(r, -1, -1);
                TreemapTile tile = new TreemapTile();
                tile.Bounds = inner;
                tile.Label = item.Label;
                tile.Size = item.Size;
                tile.Directory = item.Directory;
                tiles.Add(tile);

                if (item.Directory != null && item.Directory.SubDirectories.Count > 0 && depth < 1 &&
                    inner.Width > 120 && inner.Height > 90)
                {
                    List<TreemapItem> childItems = BuildTreemapItems(item.Directory, 8);
                    BuildTreemapTiles(childItems, Rectangle.Inflate(inner, -5, -22), depth + 1, tiles);
                }

                cursor += span;
                remainingPixels -= span;
                remainingSize -= item.Size;
                if (remainingPixels <= 0 || remainingSize <= 0) break;
            }
        }

        private void TreemapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            int hover = -1;
            for (int i = treemapTiles.Count - 1; i >= 0; i--)
            {
                if (treemapTiles[i].Bounds.Contains(e.Location))
                {
                    hover = i;
                    break;
                }
            }

            if (hover != hoveredTreemapTile)
            {
                hoveredTreemapTile = hover;
                treemapPanel.Cursor = hover >= 0 ? Cursors.Hand : Cursors.Default;
                treemapPanel.Invalidate();
            }
        }

        private void TreemapPanel_MouseLeave(object sender, EventArgs e)
        {
            hoveredTreemapTile = -1;
            treemapPanel.Cursor = Cursors.Default;
            treemapPanel.Invalidate();
        }

        private async void TreemapPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (hoveredTreemapTile < 0 || hoveredTreemapTile >= treemapTiles.Count) return;
            DirectoryNode node = treemapTiles[hoveredTreemapTile].Directory;
            if (node == null) return;

            currentVisualNode = node;
            currentSelectedDirectory = node;
            int version = Interlocked.Increment(ref treeSelectionVersion);
            statusLabel.Text = node.Name + " treemap seçimi hazırlanıyor...";

            List<FileNode> files = await Task.Run<List<FileNode>>(() =>
            {
                List<FileNode> result = new List<FileNode>();
                CollectAllFiles(node, result);
                return result.OrderByDescending(f => f.Score).ThenByDescending(f => f.Size).ToList();
            });

            if (version != treeSelectionVersion || this.IsDisposed) return;
            currentSelectionFiles = files;
            await PopulateListViewAsync(files, version);
            BuildPieData(node);
            piePanel.Invalidate();
            treemapPanel.Invalidate();
            statusLabel.Text = node.Name + "  |  " + string.Format("{0:N0}", files.Count) + " dosya";
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: AKILLI ANALİZ VE ÖNERİLER MOTORU (Gereksiz Dosya Keşfi)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Disk taraması bittikten sonra, dosyaları tarayarak akıllı
        //          temizlik planı hazırlar ve risk seviyelerine göre gruplar.
        // Yöntemi: CleanupRules sınıfı üzerinden dosyaları geçirir.
        //          deletable: Kesin silinebilir (Temp, Log, vb. risksiz konumlar)
        //          cautious: İncelemeli temizlik (Kullanıcı verileri hariç)
        //          archive: Son 6 aydır dokunulmamış büyük dosyalar (medya, zip)
        //          Kullanıcı arayüzünde dinamik olarak öneri kartları oluşturur.
        // ═══════════════════════════════════════════════════════════════════
        private List<string> deletableFilePaths = new List<string>();

        private void GenerateAIRecommendations(DirectoryNode root)
        {
            aiPanel.SuspendLayout();
            aiPanel.Controls.Clear();
            aiPanel.AutoScrollPosition = new Point(0, 0);
            deletableFilePaths.Clear();
            List<FileNode> allFiles = new List<FileNode>();
            CollectAllFiles(root, allFiles);
            if (currentSelectionFiles.Count == 0)
            {
                currentSelectionFiles = allFiles.OrderByDescending(f => f.Score).ThenByDescending(f => f.Size).ToList();
                currentSelectedDirectory = root;
            }
            int y = 14;

            Button pdfBtn = CreateAiButton(LicenseManager.HasFeature(currentLicense, LicenseFeature.PdfReport) ? "PDF Rapor" : "PDF Pro", 12, y, 132, Theme.Accent);
            pdfBtn.Click += PdfReportBtn_Click;
            aiPanel.Controls.Add(pdfBtn);

            Button csvReportBtn = CreateAiButton("CSV Dışa Aktar", 154, y, 132, Theme.Accent2);
            csvReportBtn.Click += CsvButton_Click;
            aiPanel.Controls.Add(csvReportBtn);

            Button dupReportBtn = CreateAiButton("Kopyaları Göster", 296, y, 146, Theme.Warning);
            dupReportBtn.Click += DuplicatesButton_Click;
            aiPanel.Controls.Add(dupReportBtn);
            y += 48;

            DateTime now = DateTime.Now;
            DateTime archiveThreshold = now.AddMonths(-6);
            DateTime oldThreshold = now.AddYears(-1);

            List<FileNode> deletable = allFiles
                .Where(f => CleanupRules.IsStrictSafeCleanupCandidate(f))
                .OrderByDescending(f => CleanupPriority(f)).ToList();
            long deletableSize = deletable.Sum(f => f.Size);
            deletableFilePaths = deletable.Select(f => f.FullPath).ToList();

            List<FileNode> cautiousCleanup = allFiles
                .Where(f => CleanupRules.IsReviewCleanupCandidate(f) && !deletable.Contains(f))
                .OrderByDescending(f => CleanupPriority(f))
                .Take(10).ToList();

            List<FileNode> archiveCandidates = allFiles
                .Where(f => CleanupRules.IsArchiveCandidate(f, archiveThreshold) &&
                            !CleanupRules.IsStrictSafeCleanupCandidate(f) &&
                            !CleanupRules.IsProtectedSystemPath(f.FullPath))
                .OrderByDescending(f => f.Size)
                .Take(10).ToList();
            long archiveSize = archiveCandidates.Sum(f => f.Size);

            List<FileNode> reviewCandidates = allFiles
                .Where(f => f.Score >= 60 &&
                            !CleanupRules.IsStrictSafeCleanupCandidate(f) &&
                            !CleanupRules.IsProtectedSystemPath(f.FullPath))
                .OrderByDescending(f => f.Score)
                .ThenByDescending(f => f.Size)
                .Take(8).ToList();
            long reviewSize = reviewCandidates.Sum(f => f.Size);

            List<FileNode> protectedLarge = allFiles
                .Where(f => CleanupRules.IsProtectedSystemPath(f.FullPath) && f.Size > 100L * 1024L * 1024L)
                .OrderByDescending(f => f.Size)
                .Take(5).ToList();
            long protectedSize = protectedLarge.Sum(f => f.Size);

            List<DuplicateGroup> duplicateGroups = BuildDuplicateGroups(allFiles);
            long duplicateWaste = duplicateGroups.Sum(g => g.WastedSize);

            var bigOld = allFiles
                .Where(f => f.Size > 50 * 1024 * 1024 && f.LastModified < oldThreshold)
                .OrderByDescending(f => f.Size).Take(10).ToList();

            var topLargest = allFiles.OrderByDescending(f => f.Size).Take(6).ToList();
            var bigDirs = root.SubDirectories.OrderByDescending(d => d.Size).Take(6).ToList();

            // Ağ sürücüsü ise özel not göster
            if (isNetworkDrive)
            {
                AddAiSection(ref y, "AĞ SÜRÜCÜSÜ TARAMASI  |  " + networkDriveInfo, Theme.Warning);
                AddAiNote(ref y, "Silme işlemi ağ üzerinden yapılacaktır. Dikkatli olun.");
                y += 6;
            }

            AddAiSection(ref y, "AKILLI ÖNCELİK PLANI", Theme.Accent);
            AddAiInsight(ref y, "1. Güvenli temizlik",
                deletable.Count > 0 ? deletable.Count + " dosya kesin düşük riskli görünüyor." : "Şu an kesin güvenli temizlik adayı yok.",
                FormatSize(deletableSize), deletable.Count > 0 ? Theme.Success : Theme.SubText);
            AddAiInsight(ref y, "2. Kopya kontrolü",
                duplicateGroups.Count > 0 ? duplicateGroups.Count + " olası grup var; silmeden önce içeriği doğrulayın." : "Belirgin olası kopya bulunmadı.",
                FormatSize(duplicateWaste), duplicateGroups.Count > 0 ? Theme.Warning : Theme.SubText);
            AddAiInsight(ref y, "3. Arşivle / taşı",
                archiveCandidates.Count > 0 ? "Eski büyük medya ve arşiv dosyaları ayrı diske alınabilir." : "Arşivlemeye uygun büyük eski dosya az.",
                FormatSize(archiveSize), archiveCandidates.Count > 0 ? Theme.Accent2 : Theme.SubText);
            AddAiInsight(ref y, "4. Dokunma uyarısı",
                protectedLarge.Count > 0 ? "Sistem/uygulama klasörlerinde büyük dosyalar var; otomatik silinmez." : "Riskli sistem dosyası uyarısı yok.",
                FormatSize(protectedSize), protectedLarge.Count > 0 ? Theme.Danger : Theme.SubText);
            y += 8;

            List<FileNode> smartCandidates = deletable
                .Where(f => f.Score >= 45)
                .Take(8).ToList();
            if (smartCandidates.Count > 0)
            {
                AddAiSection(ref y, "AKILLI TEMİZLİK İNDEKSİ  /  risk düşük, kazanç yüksek", Theme.Success);
                foreach (FileNode f in smartCandidates)
                    AddAiInsight(ref y, f.Name,
                        CleanupRules.CleanupReason(f) + "  |  " + CompactPath(!string.IsNullOrEmpty(f.DirectoryPath) ? f.DirectoryPath : PathText.GetDirectoryName(f.FullPath), 42),
                        "Skor " + f.Score + " / " + FormatSize(f.Size), Theme.Success);
                y += 10;
            }

            if (deletable.Count > 0)
            {
                AddAiSection(ref y, "KESİN GÜVENLİ TEMİZLİK  /  " + deletable.Count + " dosya  /  " + FormatSize(deletableSize), Theme.Success);
                AddAiNote(ref y, "Kalıcı silme yapılmaz; dosyalar Geri Dönüşüm Kutusu'na taşınır. Şüpheli dosyalar bu listeye alınmaz.");

                Button deleteAllBtn = new Button();
                deleteAllBtn.Text = LicenseManager.HasFeature(currentLicense, LicenseFeature.BulkDelete)
                    ? "Geri Dönüşüm Kutusuna Taşı  -  " + FormatSize(deletableSize)
                    : "Toplu Taşıma  (Kurumsal)";
                deleteAllBtn.Location = new Point(12, y);
                deleteAllBtn.Size = new Size(420, 34);
                deleteAllBtn.BackColor = Theme.Success;
                deleteAllBtn.ForeColor = Theme.IsDark ? Color.FromArgb(8, 12, 18) : Color.White;
                deleteAllBtn.FlatStyle = FlatStyle.Flat;
                deleteAllBtn.FlatAppearance.BorderSize = 0;
                deleteAllBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                deleteAllBtn.Cursor = Cursors.Hand;
                deleteAllBtn.Click += DeleteAllBtn_Click;
                aiPanel.Controls.Add(deleteAllBtn);
                y += 42;

                foreach (var f in deletable.Take(12))
                    AddAiInsight(ref y, f.Name, CleanupRules.CleanupReason(f) + "  |  " + CleanupRules.CleanupConfidence(f), FormatSize(f.Size), Theme.Success);
                if (deletable.Count > 12)
                    AddAiNote(ref y, "... ve " + (deletable.Count - 12) + " dosya daha");
                y += 10;
            }

            if (cautiousCleanup.Count > 0)
            {
                AddAiSection(ref y, "İNCELEMELİ TEMİZLİK  /  otomatik silme yok", Theme.Warning);
                foreach (FileNode f in cautiousCleanup)
                    AddAiInsight(ref y, f.Name,
                        CleanupRules.CleanupReason(f) + "  |  " + CleanupRules.CleanupConfidence(f),
                        FormatSize(f.Size), Theme.Warning);
                y += 10;
            }

            if (archiveCandidates.Count > 0)
            {
                AddAiSection(ref y, "ARŞİVLE / TAŞI  /  " + archiveCandidates.Count + " aday  /  " + FormatSize(archiveSize), Theme.Accent2);
                foreach (FileNode f in archiveCandidates)
                    AddAiInsight(ref y, f.Name,
                        CleanupRules.ArchiveReason(f) + "  |  Son kullanım: " + f.LastModified.ToString("yyyy-MM-dd"),
                        FormatSize(f.Size), Theme.Accent2);
                y += 10;
            }

            if (reviewCandidates.Count > 0)
            {
                AddAiSection(ref y, "İNCELEME GEREKTİREN DOSYALAR  /  otomatik silme yok", Theme.Warning);
                foreach (FileNode f in reviewCandidates)
                    AddAiInsight(ref y, f.Name,
                        "Skor yüksek ama güvenli silme sinyali zayıf; kullanıcı kararı gerekir.",
                        "Skor " + f.Score + " / " + FormatSize(f.Size), Theme.Warning);
                y += 10;
            }

            if (bigOld.Count > 0)
            {
                AddAiSection(ref y, "BÜYÜK & ESKİ  /  " + bigOld.Count + " adet  /  50MB+ ve 1 yıl+", Theme.Warning);
                foreach (var f in bigOld)
                    AddAiRow(ref y, f.Name, FormatSize(f.Size) + "  " + f.LastModified.ToString("yyyy-MM-dd"), Theme.Warning);
                y += 10;
            }

            if (duplicateGroups.Count > 0)
            {
                AddAiSection(ref y, "OLASI KOPYALAR  /  " + duplicateGroups.Count + " grup  /  " + FormatSize(duplicateWaste) + " tekrar alan", Theme.Warning);
                foreach (DuplicateGroup group in duplicateGroups.Take(8))
                {
                    AddAiInsight(ref y, group.Name,
                        group.Files.Count + " dosya, aynı ad ve boyut. İçerik doğrulaması önerilir.",
                        FormatSize(group.WastedSize), Theme.Warning);
                }
                if (duplicateGroups.Count > 8)
                    AddAiNote(ref y, "... ve " + (duplicateGroups.Count - 8) + " kopya grubu daha");
                y += 10;
            }

            if (topLargest.Count > 0)
            {
                AddAiSection(ref y, "EN ÇOK ALAN KULLANANLAR", Theme.Accent2);
                foreach (FileNode f in topLargest)
                    AddAiRow(ref y, f.Name, FormatSize(f.Size), Theme.Accent2);
                y += 10;
            }

            if (bigDirs.Count > 0)
            {
                AddAiSection(ref y, "EN BÜYÜK KLASÖRLER", Theme.Accent);
                foreach (var d in bigDirs)
                {
                    double pct = d.Size / (double)root.Size * 100.0;
                    AddAiRow(ref y, d.Name, FormatSize(d.Size) + "  %" + pct.ToString("F1"), Theme.Accent);
                }
                y += 10;
            }

            AddAiSection(ref y, "ÖZET", Theme.Success);
            AddAiRow(ref y, "Toplam boyut", FormatSize(root.Size), Theme.Success);
            AddAiRow(ref y, "Diskte kaplanan", FormatSize(root.AllocatedSize), Theme.Success);
            AddAiRow(ref y, "Toplam dosya", string.Format("{0:N0}", allFiles.Count), Theme.Success);
            AddAiRow(ref y, "Temizlenebilir", FormatSize(deletableSize), Theme.Success);
            AddAiRow(ref y, "Tekrar alan", FormatSize(duplicateWaste), Theme.Success);
            AddAiRow(ref y, "Arşiv adayı", FormatSize(archiveSize), Theme.Success);
            AddAiRow(ref y, "İnceleme alanı", FormatSize(reviewSize), Theme.Warning);
            if (duplicateHardLinksSkipped > 0)
                AddAiRow(ref y, "Hard link düzeltmesi", string.Format("{0:N0} çift sayım önlendi", duplicateHardLinksSkipped), Theme.Warning);
            if (isNetworkDrive)
                AddAiRow(ref y, "Tarama türü", "Ağ Sürücüsü", Theme.Warning);

            aiPanel.AutoScrollMinSize = new Size(0, y + 26);
            SmoothScrollPanel smooth = aiPanel as SmoothScrollPanel;
            if (smooth != null) smooth.HideChrome();
            aiPanel.ResumeLayout();
            RefreshModernScrollBars();
            tabControl.SelectedIndex = 2;
        }

        private Button CreateAiButton(string text, int x, int y, int width, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = color;
            btn.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void AddAiSection(ref int y, string text, Color color)
        {
            Panel line = new Panel();
            line.Location = new Point(12, y); line.Size = new Size(430, 2); line.BackColor = color;
            aiPanel.Controls.Add(line); y += 6;
            Label lbl = new Label();
            lbl.AutoSize = false; lbl.Width = 440; lbl.Height = 22; lbl.Location = new Point(12, y);
            lbl.Text = text; lbl.Font = new Font("Courier New", 8, FontStyle.Bold); lbl.ForeColor = color;
            aiPanel.Controls.Add(lbl); y += 26;
        }

        private void AddAiRow(ref int y, string name, string value, Color accent)
        {
            Label nl = new Label();
            nl.AutoSize = false; nl.Width = 275; nl.Height = 20; nl.Location = new Point(18, y);
            nl.Text = name; nl.Font = new Font("Segoe UI", 9); nl.ForeColor = Theme.Text;
            nl.AutoEllipsis = true;
            aiPanel.Controls.Add(nl);
            Label vl = new Label();
            vl.AutoSize = false; vl.Width = 165; vl.Height = 20; vl.Location = new Point(295, y);
            vl.Text = value; vl.Font = new Font("Segoe UI", 9, FontStyle.Bold); vl.ForeColor = accent;
            vl.AutoEllipsis = true;
            aiPanel.Controls.Add(vl);
            y += 22;
        }

        private void AddAiInsight(ref int y, string title, string detail, string value, Color accent)
        {
            Label titleLabel = new Label();
            titleLabel.AutoSize = false;
            titleLabel.Width = 270;
            titleLabel.Height = 20;
            titleLabel.Location = new Point(18, y);
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            titleLabel.ForeColor = Theme.Text;
            titleLabel.AutoEllipsis = true;
            aiPanel.Controls.Add(titleLabel);

            Label valueLabel = new Label();
            valueLabel.AutoSize = false;
            valueLabel.Width = 150;
            valueLabel.Height = 20;
            valueLabel.Location = new Point(292, y);
            valueLabel.Text = value;
            valueLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            valueLabel.ForeColor = accent;
            valueLabel.TextAlign = ContentAlignment.MiddleRight;
            valueLabel.AutoEllipsis = true;
            aiPanel.Controls.Add(valueLabel);

            Label detailLabel = new Label();
            detailLabel.AutoSize = false;
            detailLabel.Width = 420;
            detailLabel.Height = 19;
            detailLabel.Location = new Point(18, y + 19);
            detailLabel.Text = detail;
            detailLabel.Font = new Font("Segoe UI", 8);
            detailLabel.ForeColor = Theme.SubText;
            detailLabel.AutoEllipsis = true;
            aiPanel.Controls.Add(detailLabel);

            y += 43;
        }

        private double CleanupPriority(FileNode file)
        {
            if (file == null) return 0.0;
            double mb = file.Size / (1024.0 * 1024.0);
            return (file.Score * 12.0) + Math.Min(mb, 4096.0) + (CleanupRules.LocationScore(file.FullPath) * 500.0);
        }

        private void AddAiNote(ref int y, string text)
        {
            Label lbl = new Label();
            lbl.AutoSize = false; lbl.Width = 440; lbl.Height = 20; lbl.Location = new Point(18, y);
            lbl.Text = text; lbl.Font = new Font("Segoe UI", 8); lbl.ForeColor = Theme.SubText;
            aiPanel.Controls.Add(lbl); y += 22;
        }

        private void PdfReportBtn_Click(object sender, EventArgs e)
        {
            if (!EnsureFeature(LicenseFeature.PdfReport, "PDF rapor"))
                return;

            if (rootNode == null)
            {
                MessageBox.Show("Önce bir klasör taraması yapın.", "PDF Rapor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "PDF Rapor Kaydet";
            dialog.Filter = "PDF dosyası (*.pdf)|*.pdf";
            dialog.FileName = "AdvancedDiskAnalyzer_Rapor_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf";
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                byte[] chartBytes;
                int chartWidth, chartHeight;
                using (Bitmap chart = CreateReportPieBitmap(rootNode))
                {
                    chartWidth = chart.Width;
                    chartHeight = chart.Height;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        chart.Save(ms, ImageFormat.Jpeg);
                        chartBytes = ms.ToArray();
                    }
                }

                PdfReportExporter.Save(dialog.FileName, rootNode, selectedScanPath,
                    isNetworkDrive, networkDriveInfo, chartBytes, chartWidth, chartHeight);

                MessageBox.Show("PDF rapor oluşturuldu:\n" + dialog.FileName,
                    "PDF Rapor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF rapor oluşturulamadı:\n" + ex.Message,
                    "PDF Rapor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap CreateReportPieBitmap(DirectoryNode node)
        {
            Bitmap bmp = new Bitmap(900, 520);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.FromArgb(248, 250, 255));

                using (Font titleFont = new Font("Segoe UI", 18, FontStyle.Bold))
                using (Brush titleBrush = new SolidBrush(Color.FromArgb(25, 28, 45)))
                    g.DrawString("Boyut Dagilimi", titleFont, titleBrush, 34, 28);

                if (node == null || node.Size <= 0)
                {
                    using (Font font = new Font("Segoe UI", 11))
                    using (Brush brush = new SolidBrush(Color.FromArgb(90, 95, 120)))
                        g.DrawString("Grafik icin veri yok.", font, brush, 34, 80);
                    return bmp;
                }

                List<KeyValuePair<string, long>> slices = new List<KeyValuePair<string, long>>();
                foreach (DirectoryNode dir in node.SubDirectories.OrderByDescending(d => d.Size).Take(7))
                    slices.Add(new KeyValuePair<string, long>(dir.Name, dir.Size));

                long filesSize = node.Files.Sum(f => f.Size);
                if (filesSize > 0)
                    slices.Add(new KeyValuePair<string, long>("[Dosyalar]", filesSize));

                Rectangle pieRect = new Rectangle(48, 104, 300, 300);
                float start = -90f;
                for (int i = 0; i < slices.Count; i++)
                {
                    float sweep = (float)(slices[i].Value / (double)node.Size * 360.0);
                    using (Brush brush = new SolidBrush(pieColors[i % pieColors.Length]))
                        g.FillPie(brush, pieRect, start, sweep);
                    using (Pen pen = new Pen(Color.White, 3))
                        g.DrawPie(pen, pieRect, start, sweep);
                    start += sweep;
                }

                using (Font rowFont = new Font("Segoe UI", 11))
                using (Font rowBold = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(35, 38, 58)))
                using (Brush subBrush = new SolidBrush(Color.FromArgb(88, 94, 120)))
                {
                    int y = 108;
                    for (int i = 0; i < slices.Count; i++)
                    {
                        Color c = pieColors[i % pieColors.Length];
                        using (Brush swatch = new SolidBrush(c))
                            g.FillRectangle(swatch, 390, y + 5, 16, 16);
                        double pct = slices[i].Value / (double)node.Size * 100.0;
                        string name = TruncateForReport(slices[i].Key, 34);
                        g.DrawString(name, rowBold, textBrush, 416, y);
                        g.DrawString(FormatSize(slices[i].Value) + "  (" + pct.ToString("F1") + "%)",
                            rowFont, subBrush, 416, y + 24);
                        y += 58;
                    }
                }
            }
            return bmp;
        }

        private string TruncateForReport(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }

        // ═══════════════════════════════════════════════════════════════════
        // BÖLÜM: MODEL GÜNCELLEME VE TEMİZLİK ETKİLERİ (Veri Senkronizasyonu)
        // ═══════════════════════════════════════════════════════════════════
        // Amacı  : Silinen dosyaları bellek içi ağaç veri modelinden düşer ve
        //          pasta grafik, treemap ile dashboard metriklerini anlık yeniler.
        // Yöntemi: Silinen dosya setini (deletedPaths) ağaçta bulur ve siler,
        //          ardından RebuildDirectoryTotals metodu ile tüm üst dizinlerin
        //          boyutlarını, dosya sayılarını ve hard link düzeltmelerini
        //          özyinelemeli (recursive) olarak yeniden hesaplar.
        // ═══════════════════════════════════════════════════════════════════
        private void DeleteAllBtn_Click(object sender, EventArgs e)
        {
            if (!EnsureFeature(LicenseFeature.BulkDelete, "Toplu silme"))
                return;

            int count = deletableFilePaths.Count; if (count == 0) return;
            if (isNetworkDrive)
            {
                MessageBox.Show("Ağ sürücülerinde güvenli toplu silme kapalıdır.\n\nAğ paylaşımlarında Geri Dönüşüm Kutusu garantili değildir; dosyaları tek tek inceleyin.",
                    "Güvenli Silme", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string warning = count + " kesin düşük riskli dosya Geri Dönüşüm Kutusu'na taşınacak.\n\n" +
                "Kalıcı silme yapılmaz. Emin değilseniz önce listeden birkaç dosyayı Explorer'da açıp kontrol edin.";

            if (MessageBox.Show(warning, "Güvenli Temizlik Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            int deleted = 0; long freed = 0;
            HashSet<string> deletedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in deletableFilePaths.ToList())
            {
                try
                {
                    FileNode modelFile = FindFileNode(rootNode, path);
                    long sz = EstimateFreedSize(modelFile, path);
                    MoveFileToRecycleBin(path);
                    deleted++;
                    freed += sz;
                    deletedPaths.Add(path);
                }
                catch { }
            }
            deletableFilePaths.Clear();
            if (deletedPaths.Count > 0)
                ApplyDeletedFilesToModel(deletedPaths, freed);
            string msg = deleted + " dosya Geri Dönüşüm Kutusu'na taşındı, " + FormatSize(freed) + " kazanıldı.";
            MessageBox.Show(msg, "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            statusLabel.Text = msg;
        }

        private void ApplyDeletedFilesToModel(HashSet<string> deletedPaths, long freedBytes)
        {
            if (deletedPaths == null || deletedPaths.Count == 0) return;

            RemoveDeletedItemsFromListView(deletedPaths);
            currentSelectionFiles.RemoveAll(f => f != null && deletedPaths.Contains(f.FullPath));
            deletableFilePaths.RemoveAll(p => deletedPaths.Contains(p));

            if (rootNode != null)
            {
                RemoveDeletedFilesFromTree(rootNode, deletedPaths);
                RebuildDirectoryTotals(rootNode);
                totalFilesFound = CountFiles(rootNode);
                totalBytesScanned = rootNode.Size;
                totalAllocatedScanned = rootNode.AllocatedSize;
                liveCountLabel.Text = string.Format("{0:N0} dosya", totalFilesFound);

                DirectoryNode visualNode = currentSelectedDirectory != null ? currentSelectedDirectory : rootNode;
                BuildPieData(visualNode);
                piePanel.Tag = visualNode;
                piePanel.Invalidate();
                currentVisualNode = visualNode;
                treemapPanel.Invalidate();
                GenerateAIRecommendations(rootNode);

                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                treeView.Nodes.Add(BuildTreeNode(rootNode));
                if (treeView.Nodes.Count > 0) treeView.Nodes[0].Expand();
                treeView.EndUpdate();

                string totalDisplay = FormatLogicalAllocated(rootNode);
                statTotal.Text = "Toplam: " + totalDisplay;
                statFiles.Text = string.Format("{0:N0} dosya", totalFilesFound);
                UpdateSummaryCards(totalDisplay, string.Format("{0:N0}", totalFilesFound), null, selectedScanPath);
            }

            if (freedBytes > 0)
                statusLabel.Text = "Model güncellendi. Kazanç: " + FormatSize(freedBytes);
            RefreshModernScrollBars();
        }

        private void RemoveDeletedItemsFromListView(HashSet<string> deletedPaths)
        {
            NativeListViewPaint.SetRedraw(listView, false);
            try
            {
                for (int i = listView.Items.Count - 1; i >= 0; i--)
                {
                    string path = listView.Items[i].Tag as string;
                    if (!string.IsNullOrEmpty(path) && deletedPaths.Contains(path))
                        listView.Items.RemoveAt(i);
                }
            }
            finally
            {
                NativeListViewPaint.SetRedraw(listView, true);
                listView.Invalidate();
            }
        }

        private void RemoveDeletedFilesFromTree(DirectoryNode node, HashSet<string> deletedPaths)
        {
            if (node == null) return;
            for (int i = node.Files.Count - 1; i >= 0; i--)
            {
                FileNode file = node.Files[i];
                if (file != null && deletedPaths.Contains(file.FullPath))
                    node.Files.RemoveAt(i);
            }

            foreach (DirectoryNode sub in node.SubDirectories)
                RemoveDeletedFilesFromTree(sub, deletedPaths);
        }

        private void RebuildDirectoryTotals(DirectoryNode node)
        {
            HashSet<string> hardLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int skipped = 0;
            RebuildDirectoryTotals(node, hardLinks, ref skipped);
            duplicateHardLinksSkipped = skipped;
        }

        private void RebuildDirectoryTotals(DirectoryNode node, HashSet<string> hardLinks, ref int skipped)
        {
            if (node == null) return;
            long logical = 0;
            long allocated = 0;
            int fileCount = 0;
            bool partial = false;

            foreach (DirectoryNode sub in node.SubDirectories)
            {
                RebuildDirectoryTotals(sub, hardLinks, ref skipped);
                logical += sub.Size;
                allocated += sub.AllocatedSize;
                fileCount += sub.FileCount > 0 ? sub.FileCount : sub.Files.Count;
                if (sub.FilesArePartial) partial = true;
            }

            foreach (FileNode file in node.Files)
            {
                if (file == null) continue;
                file.CountedSize = file.Size;
                file.CountedAllocatedSize = file.AllocatedSize > 0 ? file.AllocatedSize : file.Size;
                if (hardLinkAccuracyEnabled && file.HardLinkCount > 1 && !string.IsNullOrEmpty(file.FileIdKey))
                {
                    if (!hardLinks.Add(file.FileIdKey))
                    {
                        file.SharedHardLink = true;
                        file.CountedSize = 0;
                        file.CountedAllocatedSize = 0;
                        skipped++;
                    }
                    else
                    {
                        file.SharedHardLink = false;
                    }
                }
                logical += file.CountedSize;
                allocated += file.CountedAllocatedSize;
                fileCount++;
            }

            node.Size = logical;
            node.AllocatedSize = allocated;
            node.FileCount = fileCount;
            node.FilesArePartial = partial;
        }

        private FileNode FindFileNode(DirectoryNode node, string fullPath)
        {
            if (node == null || string.IsNullOrEmpty(fullPath)) return null;
            foreach (FileNode file in node.Files)
                if (file != null && string.Equals(file.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                    return file;
            foreach (DirectoryNode sub in node.SubDirectories)
            {
                FileNode found = FindFileNode(sub, fullPath);
                if (found != null) return found;
            }
            return null;
        }

        private long EstimateFreedSize(FileNode file, string path)
        {
            if (file != null)
            {
                if (file.HardLinkCount > 1) return 0;
                return file.AllocatedSize > 0 ? file.AllocatedSize : file.Size;
            }
            try { return File.Exists(path) ? new FileInfo(path).Length : 0; }
            catch { return 0; }
        }

        private void MoveFileToRecycleBin(string path)
        {
            if (NativeFileApi.MoveToRecycleBin(path))
                return;
            throw new IOException("Dosya Geri Dönüşüm Kutusu'na taşınamadı. Kalıcı silme yapılmadı.");
        }

        private int CountFiles(DirectoryNode node)
        {
            if (node == null) return 0;
            if (node.FileCount > 0 || node.FilesArePartial)
                return node.FileCount;
            int count = node.Files.Count;
            foreach (DirectoryNode sub in node.SubDirectories)
                count += CountFiles(sub);
            return count;
        }

        private void CollectAllFiles(DirectoryNode node, List<FileNode> result)
        {
            if (node == null || result == null) return;
            result.AddRange(node.Files);
            foreach (var sub in node.SubDirectories) CollectAllFiles(sub, result);
        }

        private List<DuplicateGroup> BuildDuplicateGroups(List<FileNode> files)
        {
            Dictionary<string, DuplicateGroup> map = new Dictionary<string, DuplicateGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (FileNode file in files)
            {
                if (file.Size <= 0 || string.IsNullOrEmpty(file.Name)) continue;
                string key = file.Name.ToLowerInvariant() + "|" + file.Size.ToString(CultureInfo.InvariantCulture);
                DuplicateGroup group;
                if (!map.TryGetValue(key, out group))
                {
                    group = new DuplicateGroup();
                    group.Name = file.Name;
                    group.Size = file.Size;
                    map[key] = group;
                }
                group.Files.Add(file);
            }

            return map.Values
                .Where(g => g.Files.Count > 1)
                .OrderByDescending(g => g.WastedSize)
                .ToList();
        }

        // =====================================================================
        // SURUCU TIPI ALGILAMA VE YARDIMCI METOTLAR
        // =====================================================================
        // Amacı  : Disk türünün (M.2/SSD veya HDD) tespit edilmesini sağlar. Bu tespit,
        //          tarama motorunun paralel iş parçacığı (paralel thread) miktarını dinamik
        //          olarak optimize etmek için kullanılır (HDD'de kafa hareketlerini azaltmak
        //          için düşük paralellik, SSD'de ise tam paralel mod).
        // Yöntemi: WMI (Windows Management Instrumentation) altyapısı üzerinden Win32_DiskDrive
        //          ve MSFT_PhysicalDisk sınıfları sorgulanır. Sorgu sonuçları pahalı disk I/O
        //          ve IPC operasyonlarından kaçınmak amacıyla 'driveTypeCache' sözlüğünde önbelleğe alınır.
        // =====================================================================

        /// <summary>
        /// Belirtilen dosya yolunun bağlı olduğu mantıksal sürücünün SSD veya HDD olduğunu tespit eder.
        /// Elde edilen sonucu durum çubuğuna yansıtır ve önbelleğe (cache) yazar.
        /// </summary>
        private void DetectDriveType(string path)
        {
            try
            {
                string root = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(root)) root = path;
                root = root.TrimEnd('\\');
                bool cached;
                if (driveTypeCache.TryGetValue(root, out cached))
                {
                    isSSD = cached;
                    statusLabel.Text = isSSD ? "M.2/SSD - tam paralel mod" : "HDD - güvenli mod";
                    return;
                }

                isSSD = false;
                isSSD = DetectLogicalDriveIsSsd(root);
                driveTypeCache[root] = isSSD;
                statusLabel.Text = isSSD ? "M.2/SSD - tam paralel mod" : "HDD - güvenli mod";
            }
            catch
            {
                isSSD = false;
                statusLabel.Text = "Sürücü tipi bilinmiyor";
            }
        }

        /// <summary>
        /// WMI sorguları aracılığıyla mantıksal sürücünün altındaki fiziksel diskin özelliklerini denetler.
        /// MediaType, Model, Caption ve MSFT_PhysicalDisk sınıfındaki donanımsal öznitelikleri analiz eder.
        /// </summary>
        private bool DetectLogicalDriveIsSsd(string driveRoot)
        {
            try
            {
                string escapedDrive = driveRoot.Replace("\\", "\\\\").Replace("'", "\\'");
                ManagementObject logicalDisk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + escapedDrive + "'");
                foreach (ManagementObject partition in logicalDisk.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject disk in partition.GetRelated("Win32_DiskDrive"))
                    {
                        if (LooksLikeSsd(disk["MediaType"] as string) ||
                            LooksLikeSsd(disk["Model"] as string) ||
                            LooksLikeSsd(disk["Caption"] as string))
                            return true;
                    }
                }
            }
            catch { }

            try
            {
                // Windows 8 ve üzeri modern işletim sistemleri için MSFT_PhysicalDisk kontrolü
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "root\\Microsoft\\Windows\\Storage",
                    "SELECT MediaType, FriendlyName FROM MSFT_PhysicalDisk");
                int diskCount = 0;
                bool anySsd = false;
                foreach (ManagementObject disk in searcher.Get())
                {
                    diskCount++;
                    object media = disk["MediaType"];
                    int mediaType = media == null ? 0 : Convert.ToInt32(media, CultureInfo.InvariantCulture);
                    // MediaType 4 = SSD
                    if (mediaType == 4 || LooksLikeSsd(disk["FriendlyName"] as string))
                        anySsd = true;
                }
                if (diskCount == 1 && anySsd)
                    return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Disk model ismi veya medya tipi verisinde katı hal sürücüsü (SSD) işaretçilerini arar.
        /// </summary>
        private bool LooksLikeSsd(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string v = value.ToUpperInvariant();
            return v.IndexOf("SSD") >= 0 || v.IndexOf("NVME") >= 0 || v.IndexOf("NVM") >= 0 || v.IndexOf("M.2") >= 0;
        }

        /// <summary>
        /// Byte cinsinden dosya boyutunu okunabilir (human-readable) KB, MB veya GB biçimine dönüştürür.
        /// </summary>
        private string FormatSize(long size)
        {
            if (size > 1024L * 1024 * 1024) return (size / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
            if (size > 1024 * 1024)         return (size / (1024.0 * 1024)).ToString("F2") + " MB";
            if (size > 1024)                return (size / 1024.0).ToString("F2") + " KB";
            return size + " B";
        }
    }

    // =====================================================================
    // BÖLÜM: ÖZEL KULLANICI ARAYÜZÜ (UI) KONTROLLERİ VE TEMA UYUMLULUĞU
    // =====================================================================
    // Amacı  : Windows Forms'un klasik ve demode duran standart kontrolleri yerine,
    //          modern donanım ivmeli çizim yöntemleri (DoubleBuffered, UserPaint)
    //          kullanılarak tasarlanmış, akıcı (smooth) ve premium görünümlü
    //          arayüz bileşenleri (custom controls) sağlar.
    // İçerik : - MetricCardPanel    : Vurgulu sol şerit çizgisine sahip kart paneli.
    //          - SmoothListView     : Standart Windows kaydırma çubukları gizlenmiş liste.
    //          - SmoothScrollPanel  : Akıcı kaydırma yeteneğine sahip panel.
    //          - ThemedTabControl   : Özel çizim (OwnerDrawFixed) sekmeler.
    //          - DarkTitleBar       : Windows DWM API ile pencere başlığını koyu yapma.
    //          - ModernScrollBar    : Tamamen GDI+ ile el ile çizilen kaydırma çubuğu.
    // =====================================================================

    /// <summary>
    /// Dashboard istatistiklerinin gösterildiği, sol kenarında renkli accent çizgisi barındıran kart bileşeni.
    /// </summary>
    public class MetricCardPanel : Panel
    {
        public MetricCardPanel()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (SolidBrush brush = new SolidBrush(Theme.Card))
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            using (Pen pen = new Pen(Theme.Border))
                e.Graphics.DrawRectangle(pen, rect);
            using (SolidBrush brush = new SolidBrush(Theme.Accent))
                e.Graphics.FillRectangle(brush, 0, 0, 3, this.Height);
        }
    }

    /// <summary>
    /// Klasik Windows 3D tarzı kaydırma çubuklarını (WS_VSCROLL, WS_HSCROLL) Win32 API ile devredışı bırakarak
    /// yerine özel ModernScrollBar yerleştirilmesini sağlayan akıcı ListView türevidir.
    /// </summary>
    public class SmoothListView : ListView
    {
        private const int WM_PAINT = 0x000F;
        private const int WM_SIZE = 0x0005;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        public SmoothListView()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.ResizeRedraw, true);
            try
            {
                // Yansıma (Reflection) ile korumalı DoubleBuffered özelliğini aktif ederek flicker (titreme) efektini önleriz.
                System.Reflection.PropertyInfo prop = typeof(Control).GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(this, true, null);
            }
            catch { }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideChrome();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Listede herhangi bir çizim veya boyutlama olayında yerleşik scrollbar'ları zorla gizleriz
            if (m.Msg == WM_PAINT || m.Msg == WM_SIZE || m.Msg == WM_NCPAINT ||
                m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL || m.Msg == WM_MOUSEHWHEEL)
                HideChrome();
        }

        /// <summary>
        /// P/Invoke aracılığıyla ListView nesnesinin pencerelendirme (style) bitlerinden kaydırma çubuğu bayraklarını temizler.
        /// </summary>
        public void HideChrome()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    int style = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE);
                    style &= ~NativeMethods.WS_HSCROLL;
                    style &= ~NativeMethods.WS_VSCROLL;
                    NativeMethods.SetWindowLong(this.Handle, NativeMethods.GWL_STYLE, style);
                    NativeMethods.ShowScrollBar(this.Handle, NativeMethods.SB_BOTH, false);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Klasik kaydırma çubukları gizlenmiş, ModernScrollBar ile entegre edilebilen akıcı arayüz paneli.
    /// </summary>
    public class SmoothScrollPanel : Panel
    {
        private const int WM_PAINT = 0x000F;
        private const int WM_SIZE = 0x0005;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_MOUSEWHEEL = 0x020A;

        public SmoothScrollPanel()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.ResizeRedraw, true);
            this.AutoScroll = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideChrome();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT || m.Msg == WM_SIZE || m.Msg == WM_NCPAINT ||
                m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL)
                HideChrome();
        }

        public void HideChrome()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    int style = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_STYLE);
                    style &= ~NativeMethods.WS_HSCROLL;
                    style &= ~NativeMethods.WS_VSCROLL;
                    NativeMethods.SetWindowLong(this.Handle, NativeMethods.GWL_STYLE, style);
                    NativeMethods.ShowScrollBar(this.Handle, NativeMethods.SB_BOTH, false);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// GDI+ OwnerDrawFixed çizim yöntemi kullanılarak tasarlanmış,
    /// seçili sekme renklerini ve kenarlıkları aktif tema renklerine göre boyayan özel TabControl.
    /// </summary>
    public class ThemedTabControl : TabControl
    {
        public ThemedTabControl()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SizeMode = TabSizeMode.Fixed;
            this.ItemSize = new Size(120, 28);
            this.Padding = new Point(0, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Bg);
            Rectangle content = this.DisplayRectangle;
            using (SolidBrush brush = new SolidBrush(Theme.Bg))
                e.Graphics.FillRectangle(brush, content);

            // Tab başlık alanının hemen altına ince bir sınır çizgisi çiz
            int headerHeight = this.ItemSize.Height;
            using (Pen borderPen = new Pen(Theme.Border))
            {
                e.Graphics.DrawLine(borderPen, 0, headerHeight, this.Width, headerHeight);
            }

            for (int i = 0; i < this.TabPages.Count; i++)
            {
                Rectangle r = this.GetTabRect(i);
                bool selected = i == this.SelectedIndex;
                
                // Modern düz ve sekme tasarımı (VS Code esintili)
                Color bg = selected ? Theme.Surface : Theme.Bg;
                Color fg = selected ? Theme.Accent : Theme.SubText;

                using (SolidBrush brush = new SolidBrush(bg))
                    e.Graphics.FillRectangle(brush, r);

                // Sekme başlıklarının sağ tarafına dikey ayrım çizgisi
                using (Pen sepPen = new Pen(Theme.Border))
                {
                    e.Graphics.DrawLine(sepPen, r.Right - 1, r.Top, r.Right - 1, r.Bottom);
                }

                // Seçili sekmenin üst kısmına 3px kalınlığında vurgu çizgisi çiz
                if (selected)
                {
                    using (SolidBrush accentBrush = new SolidBrush(Theme.Accent))
                    {
                        e.Graphics.FillRectangle(accentBrush, r.X, r.Y, r.Width - 1, 3);
                    }
                }

                TextRenderer.DrawText(e.Graphics, this.TabPages[i].Text.Trim(), this.Font, r, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // İçerik alanının etrafına sınır çizgisi çiz
            using (Pen pen = new Pen(Theme.Border))
                e.Graphics.DrawRectangle(pen, content.X, content.Y, content.Width - 1, content.Height - 1);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.Invalidate();
        }
    }

    /// <summary>
    /// Windows UXTheme kitaplığını kullanarak TreeView ve ListView kontrollerini
    /// modern Windows Explorer/VS Code stiline (chevron okları, yumuşak seçim efektleri vb.) kavuşturur.
    /// </summary>
    public static class WindowThemeHelper
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public static void ApplyExplorerTheme(Control ctrl)
        {
            if (ctrl != null)
            {
                // Handle zorla oluşturularak temanın uygulanması garanti edilir
                IntPtr handle = ctrl.Handle;
                try { SetWindowTheme(handle, "explorer", null); } catch { }
            }
        }
    }

    /// <summary>
    /// Masaüstü Pencere Yöneticisi (Desktop Window Manager - DWM) özniteliklerini
    /// P/Invoke kullanarak değiştirip, Windows OS başlık çubuğunu koyu (Dark Mode) temaya uyarlar.
    /// </summary>
    public static class DarkTitleBar
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void Apply(IntPtr handle, bool dark)
        {
            if (handle == IntPtr.Zero) return;
            int value = dark ? 1 : 0;
            try
            {
                // DWMWA_USE_IMMERSIVE_DARK_MODE attribute = 20 (Windows 11 / Windows 10 build 18985 ve üzeri)
                int result = DwmSetWindowAttribute(handle, 20, ref value, sizeof(int));
                if (result != 0)
                    // Windows 10 eski sürümler için fallback attribute = 19
                    DwmSetWindowAttribute(handle, 19, ref value, sizeof(int));
            }
            catch { }
        }
    }

    /// <summary>
    /// ListView güncellemelerinde ekran titremesini engellemek amacıyla
    /// WM_SETREDRAW mesajını kullanarak pencere çizimini geçici olarak askıya alır.
    /// </summary>
    public static class NativeListViewPaint
    {
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void SetRedraw(Control control, bool redraw)
        {
            if (control == null || !control.IsHandleCreated) return;
            try
            {
                SendMessage(control.Handle, WM_SETREDRAW, redraw ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
            }
            catch { }
        }
    }

    /// <summary>
    /// Win32 API pencerelendirme sabitleri ve User32.dll kütüphane çağrı tanımları.
    /// </summary>
    public static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int WS_HSCROLL = 0x00100000;
        public const int WS_VSCROLL = 0x00200000;
        public const int SB_HORZ = 0;
        public const int SB_VERT = 1;
        public const int SB_BOTH = 3;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
    }

    /// <summary>
    /// ListView, TreeView veya ScrollableControl nesnelerine bağlanarak çalışan,
    /// kaydırma durumlarını GDI+ çizimiyle modern bir 'şerit kaydırıcı' olarak sunan UI kontrolü.
    /// </summary>
    public class ModernScrollBar : Panel
    {
        private Control target;
        private Rectangle thumbRect = Rectangle.Empty;
        private bool hovering = false;
        private bool dragging = false;
        private int dragOffset = 0;

        public ModernScrollBar()
        {
            this.Dock = DockStyle.Right;
            this.Width = 6;
            this.TabStop = true;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.Selectable, true);
        }

        public void Attach(Control control)
        {
            target = control;
            if (target == null) return;

            target.HandleCreated += delegate { BeginRefresh(); };
            target.SizeChanged += delegate { BeginRefresh(); };
            target.MouseEnter += delegate { TryFocusTarget(); };
            target.MouseWheel += TargetMouseWheel;
            target.MouseUp += delegate { BeginRefresh(); };
            target.KeyUp += delegate { BeginRefresh(); };
            ModernMouseWheelRouter.Register(this);

            TreeView tv = target as TreeView;
            if (tv != null)
            {
                tv.AfterExpand += delegate { BeginRefresh(); };
                tv.AfterCollapse += delegate { BeginRefresh(); };
                tv.AfterSelect += delegate { BeginRefresh(); };
            }

            ScrollableControl scrollable = target as ScrollableControl;
            if (scrollable != null)
                scrollable.Scroll += delegate { BeginRefresh(); };
        }

        private void TryFocusTarget()
        {
            try
            {
                if (target != null && target.CanFocus)
                    target.Focus();
            }
            catch { }
        }

        private void TargetMouseWheel(object sender, MouseEventArgs e)
        {
            ScrollWheelDelta(e.Delta);
        }

        public bool ContainsScreenPoint(Point screenPoint)
        {
            if (target == null || target.IsDisposed || !target.Visible) return false;
            try
            {
                Rectangle rect = target.RectangleToScreen(target.ClientRectangle);
                return rect.Contains(screenPoint);
            }
            catch { return false; }
        }

        public void ScrollWheelDelta(int wheelDelta)
        {
            int lines = Math.Max(1, SystemInformation.MouseWheelScrollLines);
            int delta = (-wheelDelta / 120) * lines;
            if (delta == 0 && wheelDelta != 0)
                delta = wheelDelta < 0 ? lines : -lines;

            if (delta != 0)
            {
                int total, visible, top;
                GetMetrics(out total, out visible, out top);
                ScrollableControl scrollable = target as ScrollableControl;
                if (scrollable != null)
                    ScrollToTopIndex(top + delta * 42);
                else
                    ScrollToTopIndex(top + delta);
                RefreshTheme();
            }
        }

        private void BeginRefresh()
        {
            HideNativeScrollBar();
            try
            {
                if (this.IsHandleCreated)
                    this.BeginInvoke(new MethodInvoker(RefreshTheme));
                else
                    RefreshTheme();
            }
            catch { RefreshTheme(); }
        }

        public void RefreshTheme()
        {
            HideNativeScrollBar();
            UpdateThumb();
            this.Invalidate();
        }

        private void HideNativeScrollBar()
        {
            try
            {
                if (target != null && target.IsHandleCreated)
                {
                    int style = NativeMethods.GetWindowLong(target.Handle, NativeMethods.GWL_STYLE);
                    style &= ~NativeMethods.WS_HSCROLL;
                    style &= ~NativeMethods.WS_VSCROLL;
                    NativeMethods.SetWindowLong(target.Handle, NativeMethods.GWL_STYLE, style);
                    NativeMethods.ShowScrollBar(target.Handle, NativeMethods.SB_BOTH, false);
                }
            }
            catch { }
        }

        private void UpdateThumb()
        {
            int total, visible, top;
            GetMetrics(out total, out visible, out top);

            if (total <= 0 || visible >= total || this.Height <= 0)
            {
                thumbRect = Rectangle.Empty;
                return;
            }

            int thumbHeight = Math.Max(28, (int)(this.Height * (visible / (double)total)));
            thumbHeight = Math.Min(this.Height, thumbHeight);
            int maxY = Math.Max(1, this.Height - thumbHeight);
            int maxTop = Math.Max(1, total - visible);
            int y = (int)Math.Round((top / (double)maxTop) * maxY);
            y = Math.Max(0, Math.Min(maxY, y));
            thumbRect = new Rectangle(0, y, this.Width, thumbHeight);
        }

        private void GetMetrics(out int total, out int visible, out int top)
        {
            total = 0; visible = 0; top = 0;
            ListView lv = target as ListView;
            if (lv != null)
            {
                total = lv.Items.Count;
                if (total == 0) return;

                int itemHeight = 18;
                int topOffset = 0;
                try
                {
                    Rectangle r = lv.GetItemRect(0);
                    itemHeight = Math.Max(1, r.Height);
                    topOffset = Math.Max(0, r.Top);
                }
                catch { }

                int usableHeight = Math.Max(1, lv.ClientSize.Height - topOffset);
                visible = Math.Max(1, Math.Min(total, usableHeight / itemHeight));
                try { top = lv.TopItem != null ? lv.TopItem.Index : 0; }
                catch { top = 0; }
                top = Math.Max(0, Math.Min(Math.Max(0, total - visible), top));
                return;
            }

            TreeView tv = target as TreeView;
            if (tv != null)
            {
                List<TreeNode> nodes = GetVisibleTreeNodes(tv);
                total = nodes.Count;
                if (total == 0) return;
                visible = Math.Max(1, Math.Min(total, tv.VisibleCount));
                top = nodes.IndexOf(tv.TopNode);
                if (top < 0) top = 0;
                top = Math.Max(0, Math.Min(Math.Max(0, total - visible), top));
                return;
            }

            ScrollableControl scrollable = target as ScrollableControl;
            if (scrollable != null)
            {
                total = Math.Max(scrollable.ClientSize.Height, scrollable.DisplayRectangle.Height);
                visible = Math.Max(1, scrollable.ClientSize.Height);
                top = Math.Max(0, -scrollable.AutoScrollPosition.Y);
                top = Math.Max(0, Math.Min(Math.Max(0, total - visible), top));
            }
        }

        private List<TreeNode> GetVisibleTreeNodes(TreeView tv)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (TreeNode node in tv.Nodes)
                AddVisibleTreeNode(node, nodes);
            return nodes;
        }

        private void AddVisibleTreeNode(TreeNode node, List<TreeNode> nodes)
        {
            nodes.Add(node);
            if (!node.IsExpanded) return;
            foreach (TreeNode child in node.Nodes)
                AddVisibleTreeNode(child, nodes);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color track = target != null ? target.BackColor : Theme.Surface;
            using (SolidBrush brush = new SolidBrush(track))
                e.Graphics.FillRectangle(brush, this.ClientRectangle);

            if (thumbRect == Rectangle.Empty) return;

            Color thumb = (hovering || dragging) ? Theme.Accent : Theme.Border;
            using (SolidBrush brush = new SolidBrush(thumb))
                e.Graphics.FillRectangle(brush, thumbRect);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            if (thumbRect == Rectangle.Empty || e.Button != MouseButtons.Left) return;

            if (thumbRect.Contains(e.Location))
            {
                dragging = true;
                dragOffset = e.Y - thumbRect.Y;
            }
            else
            {
                int total, visible, top;
                GetMetrics(out total, out visible, out top);
                ScrollToTopIndex(e.Y < thumbRect.Y ? top - visible : top + visible);
                RefreshTheme();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool wasHovering = hovering;
            hovering = thumbRect.Contains(e.Location);

            if (dragging)
            {
                ScrollFromThumbPosition(e.Y - dragOffset);
                hovering = true;
            }

            if (wasHovering != hovering || dragging)
                this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
            RefreshTheme();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (dragging) return;
            hovering = false;
            this.Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollWheelDelta(e.Delta);
        }

        private void ScrollFromThumbPosition(int proposedY)
        {
            int total, visible, top;
            GetMetrics(out total, out visible, out top);
            if (total <= visible || thumbRect == Rectangle.Empty) return;

            int maxY = Math.Max(1, this.Height - thumbRect.Height);
            int y = Math.Max(0, Math.Min(maxY, proposedY));
            int maxTop = Math.Max(0, total - visible);
            int newTop = (int)Math.Round((y / (double)maxY) * maxTop);
            ScrollToTopIndex(newTop);
            RefreshTheme();
        }

        private void ScrollToTopIndex(int index)
        {
            int total, visible, top;
            GetMetrics(out total, out visible, out top);
            if (total <= 0) return;
            int maxTop = Math.Max(0, total - visible);
            index = Math.Max(0, Math.Min(maxTop, index));

            ListView lv = target as ListView;
            if (lv != null)
            {
                try
                {
                    if (lv.Items.Count > 0)
                        lv.TopItem = lv.Items[Math.Min(index, lv.Items.Count - 1)];
                }
                catch { }
                HideNativeScrollBar();
                return;
            }

            TreeView tv = target as TreeView;
            if (tv != null)
            {
                List<TreeNode> nodes = GetVisibleTreeNodes(tv);
                if (nodes.Count == 0) return;
                try { tv.TopNode = nodes[Math.Min(index, nodes.Count - 1)]; }
                catch { }
                HideNativeScrollBar();
                return;
            }

            ScrollableControl scrollable = target as ScrollableControl;
            if (scrollable != null)
            {
                try { scrollable.AutoScrollPosition = new Point(0, index); }
                catch { }
                HideNativeScrollBar();
            }
        }
    }

    public class ModernMouseWheelRouter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private static ModernMouseWheelRouter instance = null;
        private static List<ModernScrollBar> bars = new List<ModernScrollBar>();

        public static void Register(ModernScrollBar bar)
        {
            if (bar == null) return;
            if (!bars.Contains(bar))
                bars.Add(bar);

            if (instance == null)
            {
                instance = new ModernMouseWheelRouter();
                Application.AddMessageFilter(instance);
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL) return false;

            Point cursor = Cursor.Position;
            for (int i = bars.Count - 1; i >= 0; i--)
            {
                ModernScrollBar bar = bars[i];
                if (bar == null || bar.IsDisposed)
                {
                    bars.RemoveAt(i);
                    continue;
                }

                if (bar.ContainsScreenPoint(cursor))
                {
                    int delta = unchecked((short)((m.WParam.ToInt64() >> 16) & 0xffff));
                    bar.ScrollWheelDelta(delta);
                    return true;
                }
            }
            return false;
        }
    }

    // =====================================================================
    // BÖLÜM: ÇEVRİMDIŞI LİSANS ALTYAPISI VE RSA DOĞRULAMA (ASİMETRİK ŞİFRELEME)
    // =====================================================================
    // Amacı  : Uygulamanın Pro ve Kurumsal (Enterprise) sürümlerinin lisans
    //          geçerliliğini internet bağlantısı olmaksızın asimetrik şifreleme
    //          (RSA-2048) kullanarak yerel olarak doğrular.
    // Yöntemi: Lisans anahtarı; plan, şirket adı, e-posta, geçerlilik tarihi,
    //          kullanıcı sayısı ve donanım kimliği (Hardware ID) bilgilerini içeren
    //          bir metin katarının (payload) ve bu katarın SHA-256 ile imzalanmış
    //          özel dijital imzasının (Base64 kodlanmış) birleşiminden oluşur.
    //          Lisans sunucusundaki özel anahtarla (private key) imzalanmış bu veri,
    //          programda sertifikalı açık anahtarla (public key) VerifyData yöntemiyle
    //          çift sayım ve kurcalamaya (tampering) karşı kesin zamanlı doğrulanır.
    // =====================================================================

    /// <summary>
    /// Lisans paket planlarını belirler.
    /// </summary>
    public enum LicensePlan
    {
        Free,
        Pro,
        Enterprise
    }

    /// <summary>
    /// Plana göre erişilebilen modül ve özellikleri (feature flag) listeler.
    /// </summary>
    public enum LicenseFeature
    {
        PdfReport,
        NetworkScan,
        BulkDelete,
        CustomEula
    }

    /// <summary>
    /// Aktif lisansın anlık durumunu ve kısıtlamalarını bellekte tutan model.
    /// </summary>
    public class LicenseState
    {
        public LicensePlan Plan = LicensePlan.Free;
        public string PlanName = "Ücretsiz";
        public string Company = "Ücretsiz Kullanıcı";
        public string Email = "";
        public DateTime? Expires = null;
        public int Seats = 1;
        public string HardwareId = "";
        public bool IsValid = false;
        public string RawKey = "";
        public string Message = "Ücretsiz plan aktif.";
    }

    /// <summary>
    /// Kayıt defteri (Registry) okuma/yazma operasyonlarını yürüten ve RSA doğrulamasını gerçekleştiren sınıf.
    /// </summary>
    public static class LicenseManager
    {
        private const string LicenseRegistryPath = @"Software\AdvancedDiskAnalyzer\License";
        
        // RSA-2048 Asimetrik şifreleme açık anahtarı (public key xml).
        // İmza bütünlüğünün doğrulanması için kaynak koda sabitlenmiştir.
        private const string PublicKeyXml =
            "<RSAKeyValue><Modulus>rJdXjF5+pUzOTHWnIdekvB85+OJ5TvhLygVyCXV1EllnmTYsHHLLmo9f9OfFTMZMyQG/Lo6IHUkYoXP1uq5P8Vc3Gd9GFcnj0HaLHaYJueNlDIjj3YFxJrA99ntyYdi+cM36++t057tTBujqEDncP4zgX1T029IgNewt7+F5bTWGzSJU/hKBHFxmTh/0LbiwQRxf/qj5qKUX4O0bZDYiFKgU1noPCIynDDzxnOoIDS0i9FysYBHkJeFwz0nMA81YmOtacCp9Rlg5M3+aCFqAk4j1di+WUgjAb2m3WMn07+Y58qqHRR7Cs5OHVAyed6QXvo7QFtTm2RNiCGQarM4W0Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// Windows Kayıt Defteri'nde (Registry HKCU) saklanan lisans anahtarını yükler ve doğrular.
        /// Herhangi bir hata veya imza uyuşmazlığı durumunda geri dönüp kısıtlı "Free" durumunu aktif eder.
        /// </summary>
        public static LicenseState Load()
        {
            LicenseState free = FreeState();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LicenseRegistryPath))
                {
                    if (key == null) return free;
                    string raw = key.GetValue("Key") as string;
                    if (string.IsNullOrEmpty(raw)) return free;

                    string message;
                    LicenseState state;
                    if (Validate(raw, out state, out message))
                        return state;

                    free.Message = message;
                    return free;
                }
            }
            catch { return free; }
        }

        /// <summary>
        /// Yeni girilen lisans anahtarını asimetrik olarak denetler, geçerli ise Kayıt Defteri'ne kalıcı olarak yazar.
        /// </summary>
        public static bool Install(string rawKey, out string message)
        {
            LicenseState state;
            if (!Validate(rawKey, out state, out message))
                return false;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(LicenseRegistryPath))
                {
                    if (key == null)
                    {
                        message = "Registry yazılamadı.";
                        return false;
                    }
                    key.SetValue("Key", rawKey.Trim());
                    key.SetValue("InstalledAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                message = state.PlanName + " lisansı etkinleştirildi.";
                return true;
            }
            catch (Exception ex)
            {
                message = "Lisans kaydedilemedi: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Kayıt defterindeki lisans girdilerini silerek yazılımı lisanssız sürüme düşürür.
        /// </summary>
        public static void Remove()
        {
            try { Registry.CurrentUser.DeleteSubKeyTree(LicenseRegistryPath); }
            catch { }
        }

        /// <summary>
        /// Plana göre ilgili özelliğin (Feature Flag) kullanılabilir olup olmadığını sorgular.
        /// </summary>
        public static bool HasFeature(LicenseState state, LicenseFeature feature)
        {
            if (state == null) state = FreeState();
            if (state.Plan == LicensePlan.Enterprise)
                return true;

            if (state.Plan == LicensePlan.Pro)
                return feature == LicenseFeature.PdfReport;

            return false;
        }

        public static bool HasPaidPlan(LicenseState state)
        {
            return state != null && (state.Plan == LicensePlan.Pro || state.Plan == LicensePlan.Enterprise);
        }

        public static bool IsEnterprise(LicenseState state)
        {
            return state != null && state.Plan == LicensePlan.Enterprise;
        }

        public static string RequiredPlanName(LicenseFeature feature)
        {
            return feature == LicenseFeature.PdfReport ? "Pro" : "Kurumsal";
        }

        public static string GetHardwareId()
        {
            string raw = "";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("MachineGuid");
                        if (value != null) raw = value.ToString();
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(raw))
                raw = Environment.MachineName + "|" + Environment.UserName;

            using (SHA256Managed sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24);
            }
        }

        public static string GetEnterpriseEulaText(LicenseState state)
        {
            if (!HasFeature(state, LicenseFeature.CustomEula)) return "";
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LicenseRegistryPath))
                    return key == null ? "" : (key.GetValue("EnterpriseEula") as string) ?? "";
            }
            catch { return ""; }
        }

        public static bool SaveEnterpriseEulaText(LicenseState state, string text, out string message)
        {
            if (!HasFeature(state, LicenseFeature.CustomEula))
            {
                message = "EULA özelleştirme Kurumsal planda kullanılabilir.";
                return false;
            }

            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(LicenseRegistryPath))
                {
                    if (key == null)
                    {
                        message = "Registry yazılamadı.";
                        return false;
                    }
                    key.SetValue("EnterpriseEula", text ?? "");
                }
                message = "Kurumsal EULA metni kaydedildi.";
                return true;
            }
            catch (Exception ex)
            {
                message = "EULA kaydedilemedi: " + ex.Message;
                return false;
            }
        }

        private static bool Validate(string rawKey, out LicenseState state, out string message)
        {
            state = FreeState();
            message = "Lisans geçersiz.";
            if (string.IsNullOrEmpty(rawKey)) return false;

            string normalized = rawKey.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
            string[] parts = normalized.Split('|');
            if (parts.Length != 8 || parts[0] != "ADA1")
            {
                message = "Lisans formatı geçersiz.";
                return false;
            }

            string payload = string.Join("|", parts, 0, 7);
            string signatureText = parts[7];

            try
            {
                byte[] signature = Convert.FromBase64String(signatureText);
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(PublicKeyXml);
                    byte[] data = Encoding.UTF8.GetBytes(payload);
                    if (!rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature))
                    {
                        message = "Lisans imzası doğrulanamadı.";
                        return false;
                    }
                }
            }
            catch
            {
                message = "Lisans imzası okunamadı.";
                return false;
            }

            LicensePlan plan;
            if (!TryParsePlan(parts[1], out plan) || plan == LicensePlan.Free)
            {
                message = "Lisans planı geçersiz.";
                return false;
            }

            DateTime? expires = null;
            if (!string.Equals(parts[4], "PERPETUAL", StringComparison.OrdinalIgnoreCase))
            {
                DateTime parsed;
                if (!DateTime.TryParseExact(parts[4], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    message = "Lisans bitiş tarihi geçersiz.";
                    return false;
                }
                expires = parsed.Date;
                if (DateTime.Now.Date > expires.Value)
                {
                    message = "Lisans süresi dolmuş.";
                    return false;
                }
            }

            int seats = 1;
            int.TryParse(parts[5], out seats);
            if (seats < 1) seats = 1;

            string hardware = parts[6];
            string localHardware = GetHardwareId();
            if (hardware != "*" && !string.Equals(hardware, localHardware, StringComparison.OrdinalIgnoreCase))
            {
                message = "Lisans bu cihaz için üretilmemiş.";
                return false;
            }

            state = new LicenseState();
            state.Plan = plan;
            state.PlanName = PlanToName(plan);
            state.Company = string.IsNullOrEmpty(parts[2]) ? "Kurumsal Müşteri" : parts[2];
            state.Email = parts[3];
            state.Expires = expires;
            state.Seats = seats;
            state.HardwareId = hardware;
            state.IsValid = true;
            state.RawKey = rawKey.Trim();
            state.Message = "Lisans aktif.";
            message = state.Message;
            return true;
        }

        private static bool TryParsePlan(string text, out LicensePlan plan)
        {
            plan = LicensePlan.Free;
            if (string.Equals(text, "PRO", StringComparison.OrdinalIgnoreCase))
            {
                plan = LicensePlan.Pro;
                return true;
            }
            if (string.Equals(text, "ENTERPRISE", StringComparison.OrdinalIgnoreCase))
            {
                plan = LicensePlan.Enterprise;
                return true;
            }
            return false;
        }

        private static string PlanToName(LicensePlan plan)
        {
            if (plan == LicensePlan.Pro) return "Pro";
            if (plan == LicensePlan.Enterprise) return "Kurumsal";
            return "Ücretsiz";
        }

        private static LicenseState FreeState()
        {
            LicenseState state = new LicenseState();
            state.Plan = LicensePlan.Free;
            state.PlanName = "Ücretsiz";
            state.Company = "Ücretsiz Kullanıcı";
            state.HardwareId = GetHardwareId();
            state.IsValid = false;
            state.Message = "Ücretsiz plan aktif.";
            return state;
        }
    }

    // =====================================================================
    // BÖLÜM: ÇEVRİM İÇİ LİSANS İSTEMCİSİ (OnlineLicenseClient)
    // =====================================================================
    // Amacı  : Lisans sunucusuyla (LicenseServer.exe) HTTP protokolü üzerinden
    //          haberleşerek kayıt olma (Register), giriş yapma (Login), lisans satın alma
    //          (Checkout) ve donanım kimliğiyle lisans yenileme (Activate) işlemlerini gerçekleştirir.
    // Yöntemi: WebClient nesnesinin UploadValues metoduyla sunucuya "application/x-www-form-urlencoded"
    //          tipinde HTTP POST istekleri gönderilir. Dönen veri satır bazlı çözümlenerek (parse)
    //          sunucudan gelen token ve hata mesajları sisteme entegre edilir.
    // =====================================================================
    public static class OnlineLicenseClient
    {
        private const string RegistryPath = @"Software\AdvancedDiskAnalyzer\License";
        private const string DefaultServerUrl = "http://localhost:8765";

        /// <summary>
        /// Kayıt defterinde saklanan lisans sunucusu URL'ini getirir. Bulamazsa varsayılan yerel URL'i döner.
        /// </summary>
        public static string GetServerUrl()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null) return DefaultServerUrl;
                    string value = key.GetValue("ServerUrl") as string;
                    return string.IsNullOrEmpty(value) ? DefaultServerUrl : value.TrimEnd('/');
                }
            }
            catch { return DefaultServerUrl; }
        }

        public static void SaveServerUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) url = DefaultServerUrl;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null) key.SetValue("ServerUrl", url.TrimEnd('/'));
                }
            }
            catch { }
        }

        public static string GetSavedEmail()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    return key == null ? "" : (key.GetValue("AccountEmail") as string) ?? "";
            }
            catch { return ""; }
        }

        public static string GetSavedToken()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    return key == null ? "" : (key.GetValue("AccountToken") as string) ?? "";
            }
            catch { return ""; }
        }

        public static void SaveAccount(string email, string token)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key == null) return;
                    key.SetValue("AccountEmail", email ?? "");
                    key.SetValue("AccountToken", token ?? "");
                }
            }
            catch { }
        }

        public static void ClearAccount()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key == null) return;
                    key.DeleteValue("AccountEmail", false);
                    key.DeleteValue("AccountToken", false);
                }
            }
            catch { }
        }

        public static bool Register(string serverUrl, string email, string company, string password, out string token, out string message)
        {
            token = "";
            return ReadAccountResponse(Clean(serverUrl) + "/register", Form(
                "email", email,
                "company", company,
                "password", password), out token, out message);
        }

        public static bool Login(string serverUrl, string email, string password, out string token, out string message)
        {
            token = "";
            return ReadAccountResponse(Clean(serverUrl) + "/login", Form(
                "email", email,
                "password", password), out token, out message);
        }

        public static bool Checkout(string serverUrl, string email, string company, string token, string plan, string hardwareId, out string licenseKey, out string message)
        {
            licenseKey = "";
            return ReadLicenseResponse(Clean(serverUrl) + "/checkout", Form(
                "email", email,
                "company", company,
                "token", token,
                "plan", plan,
                "hardwareId", hardwareId), out licenseKey, out message);
        }

        public static bool Activate(string serverUrl, string email, string token, string hardwareId, out string licenseKey, out string message)
        {
            licenseKey = "";
            return ReadLicenseResponse(Clean(serverUrl) + "/activate", Form(
                "email", email,
                "token", token,
                "hardwareId", hardwareId), out licenseKey, out message);
        }

        private static bool ReadAccountResponse(string url, NameValueCollection fields, out string token, out string message)
        {
            token = "";
            message = "";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string response = PostForm(client, url, fields);
                    if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                    {
                        message = response;
                        return false;
                    }

                    string[] lines = response.Replace("\r", "").Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("TOKEN=", StringComparison.OrdinalIgnoreCase))
                            token = line.Substring("TOKEN=".Length).Trim();
                        if (line.StartsWith("MESSAGE=", StringComparison.OrdinalIgnoreCase))
                            message = line.Substring("MESSAGE=".Length).Trim();
                    }
                    if (string.IsNullOrEmpty(token))
                    {
                        message = "Sunucu oturum bilgisi döndürmedi.";
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = FriendlyServerError(ex);
                return false;
            }
        }

        private static bool ReadLicenseResponse(string url, NameValueCollection fields, out string licenseKey, out string message)
        {
            licenseKey = "";
            message = "";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string response = PostForm(client, url, fields);
                    if (!response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                    {
                        message = response;
                        return false;
                    }

                    string[] lines = response.Replace("\r", "").Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("LICENSE=", StringComparison.OrdinalIgnoreCase))
                            licenseKey = line.Substring("LICENSE=".Length).Trim();
                        if (line.StartsWith("MESSAGE=", StringComparison.OrdinalIgnoreCase))
                            message = line.Substring("MESSAGE=".Length).Trim();
                    }

                    if (string.IsNullOrEmpty(licenseKey))
                    {
                        message = "Sunucu lisans anahtarı döndürmedi.";
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = FriendlyServerError(ex);
                return false;
            }
        }

        public static bool IsAccountAlreadyExists(string message)
        {
            return !string.IsNullOrEmpty(message) &&
                message.IndexOf("Account already exists", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FriendlyServerError(Exception ex)
        {
            WebException webEx = ex as WebException;
            if (webEx != null)
            {
                string body = ReadWebExceptionBody(webEx);
                string serverMessage = ExtractServerMessage(body);
                if (!string.IsNullOrEmpty(serverMessage))
                {
                    if (serverMessage == "Account already exists")
                        return "Bu e-posta ile zaten hesap var.";
                    if (serverMessage == "Invalid email or password")
                        return "E-posta veya şifre hatalı.";
                    if (serverMessage == "Login required")
                        return "Oturum süresi dolmuş. Lütfen tekrar giriş yapın.";
                    if (serverMessage == "No license found")
                        return "Bu hesap için bu cihazda aktif lisans bulunamadı.";
                    return serverMessage;
                }

                HttpWebResponse response = webEx.Response as HttpWebResponse;
                if (response != null)
                {
                    if ((int)response.StatusCode == 409)
                        return "Bu e-posta ile zaten hesap var.";
                    if ((int)response.StatusCode == 401)
                        return "E-posta veya şifre hatalı.";
                }
            }
            return "Lisans sunucusuna ulaşılamadı. Sunucunun çalıştığından emin olun.";
        }

        private static string ReadWebExceptionBody(WebException ex)
        {
            try
            {
                if (ex.Response == null) return "";
                using (Stream stream = ex.Response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch { return ""; }
        }

        private static string ExtractServerMessage(string response)
        {
            if (string.IsNullOrEmpty(response)) return "";
            string[] lines = response.Replace("\r", "").Split('\n');
            foreach (string line in lines)
                if (line.StartsWith("MESSAGE=", StringComparison.OrdinalIgnoreCase))
                    return line.Substring("MESSAGE=".Length).Trim();
            return "";
        }

        private static string Clean(string url)
        {
            if (string.IsNullOrEmpty(url)) url = DefaultServerUrl;
            return url.TrimEnd('/');
        }

        private static NameValueCollection Form(params string[] values)
        {
            NameValueCollection fields = new NameValueCollection();
            for (int i = 0; i + 1 < values.Length; i += 2)
                fields[values[i]] = values[i + 1] ?? "";
            return fields;
        }

        private static string PostForm(WebClient client, string url, NameValueCollection fields)
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            byte[] responseBytes = client.UploadValues(url, "POST", fields);
            return Encoding.UTF8.GetString(responseBytes);
        }
    }

    public class AccountForm : Form
    {
        private TextBox emailBox;
        private TextBox companyBox;
        private TextBox passwordBox;
        private Label statusLabel;

        public AccountForm()
        {
            this.Text = "Hesap";
            this.Size = new Size(560, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.Surface;
            this.ForeColor = Theme.Text;
            BuildUi();
            RefreshStatus();
        }

        private void BuildUi()
        {
            Label title = new Label();
            title.Text = "Advanced Disk Analyzer Hesabı";
            title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            title.Location = new Point(26, 22);
            title.Size = new Size(480, 34);
            title.ForeColor = Theme.Accent;
            this.Controls.Add(title);

            statusLabel = new Label();
            statusLabel.Location = new Point(30, 62);
            statusLabel.Size = new Size(480, 24);
            statusLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            statusLabel.ForeColor = Theme.SubText;
            this.Controls.Add(statusLabel);

            Label emailLabel = FieldLabel("E-posta", 30, 106);
            this.Controls.Add(emailLabel);
            emailBox = FieldBox(30, 130, 480);
            emailBox.Text = OnlineLicenseClient.GetSavedEmail();
            this.Controls.Add(emailBox);

            Label companyLabel = FieldLabel("Firma / Kurum", 30, 166);
            this.Controls.Add(companyLabel);
            companyBox = FieldBox(30, 190, 480);
            this.Controls.Add(companyBox);

            Label passwordLabel = FieldLabel("Şifre", 30, 226);
            this.Controls.Add(passwordLabel);
            passwordBox = FieldBox(30, 250, 480);
            passwordBox.PasswordChar = '*';
            this.Controls.Add(passwordBox);

            Button register = AccentButton("Kayıt Ol", 30, 302, 118);
            register.Click += Register_Click;
            this.Controls.Add(register);

            Button login = AccentButton("Giriş Yap", 160, 302, 118);
            login.Click += Login_Click;
            this.Controls.Add(login);

            Button logout = NeutralButton("Çıkış", 290, 302, 88);
            logout.Click += delegate
            {
                OnlineLicenseClient.ClearAccount();
                RefreshStatus();
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(logout);

            Button settings = NeutralButton("Ayar", 390, 302, 88);
            settings.Click += ServerSettings_Click;
            this.Controls.Add(settings);

            Button close = NeutralButton("Kapat", 390, 344, 88);
            close.Click += delegate { this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(close);
        }

        private Label FieldLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(200, 20);
            label.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            label.ForeColor = Theme.Text;
            return label;
        }

        private TextBox FieldBox(int x, int y, int width)
        {
            TextBox box = new TextBox();
            box.Location = new Point(x, y);
            box.Size = new Size(width, 24);
            box.Font = new Font("Segoe UI", 9);
            box.BackColor = Theme.Card;
            box.ForeColor = Theme.Text;
            return box;
        }

        private Button AccentButton(string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = Theme.Accent;
            btn.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
        }

        private Button NeutralButton(string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = Theme.Card;
            btn.ForeColor = Theme.Text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Theme.Border;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
        }

        private void Register_Click(object sender, EventArgs e)
        {
            string email = emailBox.Text.Trim();
            string company = companyBox.Text.Trim();
            string password = passwordBox.Text;
            if (!ValidateFields(email, password)) return;
            if (string.IsNullOrEmpty(company)) company = email;

            string token, message;
            if (OnlineLicenseClient.Register(OnlineLicenseClient.GetServerUrl(), email, company, password, out token, out message))
            {
                OnlineLicenseClient.SaveAccount(email, token);
                RefreshStatus();
                MessageBox.Show("Hesap oluşturuldu ve giriş yapıldı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else if (OnlineLicenseClient.IsAccountAlreadyExists(message))
            {
                if (OnlineLicenseClient.Login(OnlineLicenseClient.GetServerUrl(), email, password, out token, out message))
                {
                    OnlineLicenseClient.SaveAccount(email, token);
                    RefreshStatus();
                    MessageBox.Show("Bu hesap zaten vardı; giriş yapıldı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                    MessageBox.Show("Bu e-posta ile hesap var. Şifreyi kontrol edip Giriş Yap'ı kullanın.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(message, "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Login_Click(object sender, EventArgs e)
        {
            string email = emailBox.Text.Trim();
            string password = passwordBox.Text;
            if (!ValidateFields(email, password)) return;

            string token, message;
            if (OnlineLicenseClient.Login(OnlineLicenseClient.GetServerUrl(), email, password, out token, out message))
            {
                OnlineLicenseClient.SaveAccount(email, token);
                RefreshStatus();
                MessageBox.Show("Giriş başarılı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(message, "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private bool ValidateFields(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || email.IndexOf("@") < 1)
            {
                MessageBox.Show("Geçerli bir e-posta girin.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Şifre en az 6 karakter olmalı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void ServerSettings_Click(object sender, EventArgs e)
        {
            string value = PromptForm.Show("Lisans Sunucusu", "Kurumsal veya test sunucusu adresi:", OnlineLicenseClient.GetServerUrl());
            if (value == null) return;
            OnlineLicenseClient.SaveServerUrl(value);
            MessageBox.Show("Lisans sunucusu kaydedildi.", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshStatus()
        {
            string email = OnlineLicenseClient.GetSavedEmail();
            statusLabel.Text = string.IsNullOrEmpty(email) ? "Oturum açık değil" : "Oturum açık: " + email;
            statusLabel.ForeColor = string.IsNullOrEmpty(email) ? Theme.SubText : Theme.Success;
        }
    }

    public class UpgradeForm : Form
    {
        public UpgradeForm(LicenseState current, LicenseFeature feature, string title, string purchaseUrl, string enterpriseUrl)
        {
            this.Text = "Özellik Kilitli";
            this.Size = new Size(520, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.Surface;

            Label heading = new Label();
            heading.Text = title + " için " + LicenseManager.RequiredPlanName(feature) + " gerekir";
            heading.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            heading.Location = new Point(24, 24);
            heading.Size = new Size(460, 30);
            heading.ForeColor = Theme.Text;
            this.Controls.Add(heading);

            Label body = new Label();
            body.Text = "Mevcut plan: " + (current == null ? "Ücretsiz" : current.PlanName) +
                "\n\nPro: PDF rapor\nKurumsal: ağ sürücüsü, toplu silme, kurumsal EULA";
            body.Font = new Font("Segoe UI", 9);
            body.Location = new Point(26, 70);
            body.Size = new Size(450, 78);
            body.ForeColor = Theme.SubText;
            this.Controls.Add(body);

            Button buy = new Button();
            buy.Text = "Planları Aç";
            buy.Location = new Point(26, 170);
            buy.Size = new Size(170, 34);
            buy.BackColor = Theme.Accent;
            buy.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            buy.FlatStyle = FlatStyle.Flat;
            buy.FlatAppearance.BorderSize = 0;
            buy.Click += delegate
            {
                using (LicenseForm form = new LicenseForm(LicenseManager.Load(), purchaseUrl, enterpriseUrl))
                    form.ShowDialog(this);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(buy);

            Button manage = new Button();
            manage.Text = "Hesap";
            manage.Location = new Point(208, 170);
            manage.Size = new Size(130, 34);
            manage.BackColor = Theme.Card;
            manage.ForeColor = Theme.Text;
            manage.FlatStyle = FlatStyle.Flat;
            manage.FlatAppearance.BorderColor = Theme.Border;
            manage.Click += delegate
            {
                using (AccountForm form = new AccountForm())
                    form.ShowDialog(this);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(manage);

            Button close = new Button();
            close.Text = "Kapat";
            close.Location = new Point(350, 170);
            close.Size = new Size(120, 34);
            close.BackColor = Theme.Card;
            close.ForeColor = Theme.SubText;
            close.FlatStyle = FlatStyle.Flat;
            close.FlatAppearance.BorderColor = Theme.Border;
            close.DialogResult = DialogResult.Cancel;
            this.Controls.Add(close);
        }
    }

    public class LicenseForm : Form
    {
        private LicenseState current;
        private TextBox keyBox;
        private TextBox eulaBox;
        private TextBox emailBox;
        private TextBox companyBox;
        private TextBox passwordBox;
        private TextBox serverBox;
        private Label statusLabel;
        private Label accountLabel;
        private string purchaseUrl;
        private string enterpriseUrl;

        public LicenseForm(LicenseState state, string purchaseUrl, string enterpriseUrl)
        {
            current = state ?? LicenseManager.Load();
            this.purchaseUrl = purchaseUrl;
            this.enterpriseUrl = enterpriseUrl;

            this.Text = "Plan ve Lisans";
            this.Size = new Size(820, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(760, 680);
            this.BackColor = Theme.Surface;
            this.ForeColor = Theme.Text;

            BuildUi();
            RefreshStatus();
        }

        private void BuildUi()
        {
            Label title = new Label();
            title.Text = "Plan ve Lisans";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(26, 20);
            title.Size = new Size(560, 34);
            title.ForeColor = Theme.Accent;
            this.Controls.Add(title);

            statusLabel = new Label();
            statusLabel.Location = new Point(30, 60);
            statusLabel.Size = new Size(740, 26);
            statusLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            statusLabel.ForeColor = Theme.Text;
            this.Controls.Add(statusLabel);

            accountLabel = new Label();
            accountLabel.Location = new Point(520, 62);
            accountLabel.Size = new Size(250, 22);
            accountLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            accountLabel.TextAlign = ContentAlignment.MiddleRight;
            accountLabel.ForeColor = Theme.SubText;
            this.Controls.Add(accountLabel);

            AddPlanColumn(30, 104, "Ücretsiz", "Temel analiz\nTreemap\nKopya görünümü\nCSV dışa aktarım");
            AddPlanColumn(286, 104, "Pro", "Ücretsiz özellikleri\nPDF rapor\nProfesyonel çıktı\nÇevrimdışı lisans");
            AddPlanColumn(542, 104, "Kurumsal", "Pro özellikleri\nAğ sürücüsü\nToplu silme\nKurumsal EULA");

            Label hardware = new Label();
            hardware.Text = "Cihaz ID: " + LicenseManager.GetHardwareId();
            hardware.Location = new Point(32, 250);
            hardware.Size = new Size(520, 22);
            hardware.Font = new Font("Segoe UI", 9);
            hardware.ForeColor = Theme.SubText;
            this.Controls.Add(hardware);

            Button copyHardware = SmallButton("Cihaz ID Kopyala", 580, 244, 160);
            copyHardware.Click += delegate { Clipboard.SetText(LicenseManager.GetHardwareId()); };
            this.Controls.Add(copyHardware);

            Label emailLabel = new Label();
            emailLabel.Text = "E-posta";
            emailLabel.Location = new Point(32, 284);
            emailLabel.Size = new Size(120, 18);
            emailLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            emailLabel.ForeColor = Theme.Text;
            emailLabel.Visible = false;
            this.Controls.Add(emailLabel);

            emailBox = new TextBox();
            emailBox.Location = new Point(34, 306);
            emailBox.Size = new Size(220, 23);
            emailBox.Font = new Font("Segoe UI", 9);
            emailBox.BackColor = Theme.Card;
            emailBox.ForeColor = Theme.Text;
            emailBox.Text = !string.IsNullOrEmpty(OnlineLicenseClient.GetSavedEmail()) ? OnlineLicenseClient.GetSavedEmail() : current.Email;
            emailBox.Visible = false;
            this.Controls.Add(emailBox);

            Label companyLabel = new Label();
            companyLabel.Text = "Firma";
            companyLabel.Location = new Point(270, 284);
            companyLabel.Size = new Size(120, 18);
            companyLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            companyLabel.ForeColor = Theme.Text;
            companyLabel.Visible = false;
            this.Controls.Add(companyLabel);

            companyBox = new TextBox();
            companyBox.Location = new Point(272, 306);
            companyBox.Size = new Size(210, 23);
            companyBox.Font = new Font("Segoe UI", 9);
            companyBox.BackColor = Theme.Card;
            companyBox.ForeColor = Theme.Text;
            companyBox.Text = (current.Company == "Free Kullanici" || current.Company == "Ücretsiz Kullanıcı") ? "" : current.Company;
            companyBox.Visible = false;
            this.Controls.Add(companyBox);

            Label passwordLabel = new Label();
            passwordLabel.Text = "Şifre";
            passwordLabel.Location = new Point(498, 284);
            passwordLabel.Size = new Size(120, 18);
            passwordLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            passwordLabel.ForeColor = Theme.Text;
            passwordLabel.Visible = false;
            this.Controls.Add(passwordLabel);

            passwordBox = new TextBox();
            passwordBox.Location = new Point(500, 306);
            passwordBox.Size = new Size(168, 23);
            passwordBox.Font = new Font("Segoe UI", 9);
            passwordBox.BackColor = Theme.Card;
            passwordBox.ForeColor = Theme.Text;
            passwordBox.PasswordChar = '*';
            passwordBox.Visible = false;
            this.Controls.Add(passwordBox);

            Button serverSettings = SmallButton("Ayar", 680, 301, 62);
            serverSettings.Click += ServerSettings_Click;
            serverSettings.Visible = false;
            this.Controls.Add(serverSettings);

            serverBox = new TextBox();
            serverBox.Visible = false;
            serverBox.Text = OnlineLicenseClient.GetServerUrl();
            this.Controls.Add(serverBox);

            Label keyLabel = new Label();
            keyLabel.Text = "Lisans anahtarı";
            keyLabel.Location = new Point(32, 286);
            keyLabel.Size = new Size(200, 20);
            keyLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            keyLabel.ForeColor = Theme.Text;
            this.Controls.Add(keyLabel);

            keyBox = new TextBox();
            keyBox.Location = new Point(34, 310);
            keyBox.Size = new Size(708, 76);
            keyBox.Multiline = true;
            keyBox.ScrollBars = ScrollBars.Vertical;
            keyBox.Font = new Font("Consolas", 9);
            keyBox.Text = current.RawKey;
            keyBox.BackColor = Theme.Card;
            keyBox.ForeColor = Theme.Text;
            this.Controls.Add(keyBox);

            Button register = SmallButton("Hesap", 34, 402, 92);
            register.Click += OpenAccount_Click;
            this.Controls.Add(register);

            Button login = ActionButton("Pro Al", 136, 402, Theme.Accent);
            login.Click += delegate { OnlineCheckout("PRO"); };
            this.Controls.Add(login);

            Button activate = SmallButton("Anahtar Etkinleştir", 276, 402, 120);
            activate.Click += Activate_Click;
            this.Controls.Add(activate);

            Button remove = SmallButton("Ücretsiz", 406, 402, 72);
            remove.Click += delegate
            {
                LicenseManager.Remove();
                current = LicenseManager.Load();
                keyBox.Text = "";
                RefreshStatus();
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(remove);

            Button buyPro = SmallButton("Kurumsal", 484, 402, 96);
            buyPro.Click += delegate { OnlineCheckout("ENTERPRISE"); };
            this.Controls.Add(buyPro);

            Button enterprise = SmallButton("Aktar", 590, 402, 82);
            enterprise.Click += OnlineActivate_Click;
            this.Controls.Add(enterprise);

            Button onlineActivate = SmallButton("Aktar", 682, 402, 60);
            onlineActivate.Click += OnlineActivate_Click;
            onlineActivate.Visible = false;
            this.Controls.Add(onlineActivate);

            Label eulaLabel = new Label();
            eulaLabel.Text = "Kurumsal EULA metni";
            eulaLabel.Location = new Point(32, 456);
            eulaLabel.Size = new Size(220, 20);
            eulaLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            eulaLabel.ForeColor = Theme.Text;
            this.Controls.Add(eulaLabel);

            eulaBox = new TextBox();
            eulaBox.Location = new Point(34, 480);
            eulaBox.Size = new Size(568, 82);
            eulaBox.Multiline = true;
            eulaBox.ScrollBars = ScrollBars.Vertical;
            eulaBox.Font = new Font("Segoe UI", 9);
            eulaBox.BackColor = Theme.Card;
            eulaBox.ForeColor = Theme.Text;
            eulaBox.Text = LicenseManager.GetEnterpriseEulaText(current);
            eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
            this.Controls.Add(eulaBox);

            Button saveEula = SmallButton("EULA Kaydet", 620, 480, 122);
            saveEula.Click += SaveEula_Click;
            this.Controls.Add(saveEula);

            Button close = SmallButton("Kapat", 620, 528, 122);
            close.Click += delegate { this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(close);
        }

        private void AddPlanColumn(int x, int y, string title, string body)
        {
            Panel panel = new Panel();
            panel.Location = new Point(x, y);
            panel.Size = new Size(226, 122);
            panel.BackColor = Theme.Card;
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Theme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            this.Controls.Add(panel);

            Label heading = new Label();
            heading.Text = title;
            heading.Location = new Point(12, 10);
            heading.Size = new Size(190, 24);
            heading.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            heading.ForeColor = title == "Kurumsal" ? Theme.Success : Theme.Accent;
            panel.Controls.Add(heading);

            Label desc = new Label();
            desc.Text = body;
            desc.Location = new Point(14, 40);
            desc.Size = new Size(196, 72);
            desc.Font = new Font("Segoe UI", 8);
            desc.ForeColor = Theme.SubText;
            panel.Controls.Add(desc);
        }

        private Button ActionButton(string text, int x, int y, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(126, 34);
            btn.BackColor = color;
            btn.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
        }

        private Button SmallButton(string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = Theme.Card;
            btn.ForeColor = Theme.Text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Theme.Border;
            btn.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            return btn;
        }

        private void Activate_Click(object sender, EventArgs e)
        {
            string message;
            if (LicenseManager.Install(keyBox.Text, out message))
            {
                current = LicenseManager.Load();
                RefreshStatus();
                eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
                MessageBox.Show(message, "Lisans", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(message, "Lisans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenAccount_Click(object sender, EventArgs e)
        {
            using (AccountForm form = new AccountForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    emailBox.Text = OnlineLicenseClient.GetSavedEmail();
                    RefreshStatus();
                }
            }
        }

        private void Register_Click(object sender, EventArgs e)
        {
            string email = emailBox.Text.Trim();
            string company = companyBox.Text.Trim();
            string password = passwordBox.Text;
            if (!ValidateAccountFields(email, password)) return;
            if (string.IsNullOrEmpty(company)) company = email;

            string token, message;
            if (OnlineLicenseClient.Register(serverBox.Text.Trim(), email, company, password, out token, out message))
            {
                OnlineLicenseClient.SaveAccount(email, token);
                accountLabel.Text = "Oturum açık: " + email;
                companyBox.Text = company;
                MessageBox.Show("Hesap oluşturuldu ve giriş yapıldı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (OnlineLicenseClient.IsAccountAlreadyExists(message))
            {
                if (OnlineLicenseClient.Login(serverBox.Text.Trim(), email, password, out token, out message))
                {
                    OnlineLicenseClient.SaveAccount(email, token);
                    accountLabel.Text = "Oturum açık: " + email;
                    MessageBox.Show("Bu hesap zaten vardı; giriş yapıldı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Bu e-posta ile hesap var. Şifreyi kontrol edip Giriş Yap'ı kullanın.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(message, "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Login_Click(object sender, EventArgs e)
        {
            string email = emailBox.Text.Trim();
            string password = passwordBox.Text;
            if (!ValidateAccountFields(email, password)) return;

            string token, message;
            if (OnlineLicenseClient.Login(serverBox.Text.Trim(), email, password, out token, out message))
            {
                OnlineLicenseClient.SaveAccount(email, token);
                accountLabel.Text = "Oturum açık: " + email;
                MessageBox.Show("Giriş başarılı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(message, "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private bool ValidateAccountFields(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || email.IndexOf("@") < 1)
            {
                MessageBox.Show("Geçerli bir e-posta girin.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Şifre en az 6 karakter olmalı.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private bool EnsureSignedIn(out string email, out string token)
        {
            email = emailBox.Text.Trim();
            token = OnlineLicenseClient.GetSavedToken();
            if (string.IsNullOrEmpty(email))
                email = OnlineLicenseClient.GetSavedEmail();
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email))
                return true;

            MessageBox.Show("Önce Kayıt Ol veya Giriş Yap ile hesabını aç.", "Hesap gerekli", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void ServerSettings_Click(object sender, EventArgs e)
        {
            string currentUrl = OnlineLicenseClient.GetServerUrl();
            string value = PromptForm.Show("Lisans Sunucusu", "Kurumsal veya test sunucusu adresi:", currentUrl);
            if (value == null) return;
            OnlineLicenseClient.SaveServerUrl(value);
            serverBox.Text = OnlineLicenseClient.GetServerUrl();
            MessageBox.Show("Lisans sunucusu kaydedildi.", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnlineCheckout(string plan)
        {
            string email, token;
            if (!EnsureSignedIn(out email, out token)) return;
            string company = companyBox.Text.Trim();
            string serverUrl = serverBox.Text.Trim();
            if (string.IsNullOrEmpty(company)) company = email;

            OnlineLicenseClient.SaveServerUrl(serverUrl);
            string licenseKey, message;
            if (!OnlineLicenseClient.Checkout(serverUrl, email, company, token, plan, LicenseManager.GetHardwareId(), out licenseKey, out message))
            {
                MessageBox.Show(message, "Çevrim İçi Lisans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            keyBox.Text = licenseKey;
            if (LicenseManager.Install(licenseKey, out message))
            {
                current = LicenseManager.Load();
                RefreshStatus();
                eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
                MessageBox.Show(message, "Çevrim İçi Lisans", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(message, "Çevrim İçi Lisans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OnlineActivate_Click(object sender, EventArgs e)
        {
            string email, token;
            if (!EnsureSignedIn(out email, out token)) return;
            string serverUrl = serverBox.Text.Trim();

            OnlineLicenseClient.SaveServerUrl(serverUrl);
            string licenseKey, message;
            if (!OnlineLicenseClient.Activate(serverUrl, email, token, LicenseManager.GetHardwareId(), out licenseKey, out message))
            {
                MessageBox.Show(message, "Çevrim İçi Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            keyBox.Text = licenseKey;
            if (LicenseManager.Install(licenseKey, out message))
            {
                current = LicenseManager.Load();
                RefreshStatus();
                eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
                MessageBox.Show(message, "Çevrim İçi Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(message, "Çevrim İçi Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SaveEula_Click(object sender, EventArgs e)
        {
            string message;
            if (LicenseManager.SaveEnterpriseEulaText(current, eulaBox.Text, out message))
                MessageBox.Show(message, "Kurumsal EULA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(message, "Kurumsal EULA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void RefreshStatus()
        {
            string expires = current.Expires.HasValue ? current.Expires.Value.ToString("yyyy-MM-dd") : "Süresiz";
            statusLabel.Text = "Plan: " + current.PlanName + "  |  Firma: " + current.Company + "  |  Bitiş: " + expires;
            statusLabel.ForeColor = LicenseManager.HasPaidPlan(current) ? Theme.Success : Theme.SubText;
            string savedEmail = OnlineLicenseClient.GetSavedEmail();
            if (accountLabel != null)
                accountLabel.Text = string.IsNullOrEmpty(savedEmail) ? "Hesap yok" : "Oturum açık: " + savedEmail;
        }
    }

    /// <summary>
    /// Kullanıcıdan basit metinsel veri girdisi (örneğin kurumsal sunucu URL'i)
    /// almak için kullanılan modal diyalog kutusu.
    /// </summary>
    public class PromptForm : Form
    {
        private TextBox input;
        private string result = null;

        public static string Show(string title, string label, string defaultValue)
        {
            using (PromptForm form = new PromptForm(title, label, defaultValue))
            {
                return form.ShowDialog() == DialogResult.OK ? form.result : null;
            }
        }

        private PromptForm(string title, string label, string defaultValue)
        {
            this.Text = title;
            this.Size = new Size(520, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.Surface;

            Label lbl = new Label();
            lbl.Text = label;
            lbl.Location = new Point(20, 18);
            lbl.Size = new Size(460, 24);
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Theme.Text;
            this.Controls.Add(lbl);

            input = new TextBox();
            input.Location = new Point(22, 50);
            input.Size = new Size(460, 24);
            input.Text = defaultValue;
            input.BackColor = Theme.Card;
            input.ForeColor = Theme.Text;
            this.Controls.Add(input);

            Button ok = new Button();
            ok.Text = "Kaydet";
            ok.Location = new Point(276, 94);
            ok.Size = new Size(96, 32);
            ok.BackColor = Theme.Accent;
            ok.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            ok.FlatStyle = FlatStyle.Flat;
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += delegate { result = input.Text.Trim(); this.DialogResult = DialogResult.OK; this.Close(); };
            this.Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "İptal";
            cancel.Location = new Point(386, 94);
            cancel.Size = new Size(96, 32);
            cancel.BackColor = Theme.Card;
            cancel.ForeColor = Theme.Text;
            cancel.FlatStyle = FlatStyle.Flat;
            cancel.FlatAppearance.BorderColor = Theme.Border;
            cancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancel);

            this.AcceptButton = ok;
            this.CancelButton = cancel;
        }
    }

    // =====================================================================
    // BÖLÜM: İLK AÇILIŞ GİZLİLIK EKRANI (PrivacyConsentForm)
    // =====================================================================
    // Amacı  : Uygulama ilk kez çalıştırıldığında veya lisans değiştiğinde,
    //          kullanıcıya gizlilik politikası ve EULA (Son Kullanıcı Lisans Sözleşmesi)
    //          koşullarını sunarak yasal kabul (consent) alır.
    // =====================================================================
    public class PrivacyConsentForm : Form
    {
        public PrivacyConsentForm(string version, string stampDate, string customEulaText)
        {
            this.Text = "Gizlilik Güvencesi";
            this.Size = new Size(560, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Theme.Surface;
            this.ForeColor = Theme.Text;

            Label title = new Label();
            title.Text = "Advanced Disk Analyzer";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(28, 24);
            title.Size = new Size(500, 32);
            title.ForeColor = Theme.Accent;
            this.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "EULA / Gizlilik Güvencesi";
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            subtitle.Location = new Point(30, 64);
            subtitle.Size = new Size(480, 24);
            subtitle.ForeColor = Theme.Text;
            this.Controls.Add(subtitle);

            string[] lines = string.IsNullOrEmpty(customEulaText)
                ? new string[]
            {
                "- Bu yazılım tamamen çevrimdışı çalışır.",
                "- Hiçbir dosya adı, boyut veya içerik dış sunuculara gönderilmez.",
                "- Geliştirici hiçbir kullanıcı verisine erişemez."
            }
                : customEulaText.Replace("\r", "").Split('\n');

            int y = 108;
            for (int i = 0; i < lines.Length; i++)
            {
                if (y > 214) break;
                Label line = new Label();
                line.Text = lines[i];
                line.Font = new Font("Segoe UI", 10);
                line.Location = new Point(34, y);
                line.Size = new Size(480, 26);
                line.ForeColor = Theme.Text;
                this.Controls.Add(line);
                y += 36;
            }

            Label stamp = new Label();
            stamp.Text = "Sürüm: " + version + "  |  Metin tarihi: " + stampDate;
            stamp.Font = new Font("Segoe UI", 9);
            stamp.Location = new Point(34, 232);
            stamp.Size = new Size(480, 22);
            stamp.ForeColor = Theme.SubText;
            this.Controls.Add(stamp);

            Button accept = new Button();
            accept.Text = "Kabul Et";
            accept.Location = new Point(374, 270);
            accept.Size = new Size(140, 34);
            accept.BackColor = Theme.Accent;
            accept.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            accept.FlatStyle = FlatStyle.Flat;
            accept.FlatAppearance.BorderSize = 0;
            accept.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            accept.DialogResult = DialogResult.OK;
            this.Controls.Add(accept);

            this.AcceptButton = accept;
        }
    }

    // =====================================================================
    // BÖLÜM: HARİCİ KÜTÜPHANESİZ PDF RAPOR OLUŞTURUCU (PdfReportExporter)
    // =====================================================================
    // Amacı  : Pro ve Kurumsal sürümlerde, disk analizi sonuçlarını (en büyük dosyalar,
    //          temizlik önerileri, pasta grafik vb.) harici hiçbir PDF motoruna (iTextSharp vb.)
    //          bağımlı olmadan, ham PDF-1.4 dosya yapısını binary düzeyde el ile
    //          inşa ederek PDF formatında dışa aktarır.
    // Yöntemi: PDF dosya formatı spesifikasyonuna uygun olarak catalog, font, resim ve
    //          sayfa stream nesnelerini (PDF objects) byte dizileri halinde oluşturur.
    //          Görüntüleri DCTDecode (JPEG stream) filtresi ile doğrudan PDF içine gömer.
    //          Dosya sonuna xref (cross-reference table) tablosunu kesin byte konumlarıyla yazarak
    //          standart PDF okuyucular tarafından sorunsuz açılmasını garanti eder.
    // =====================================================================
    public class PdfReportExporter
    {
        private const float PageWidth = 595f;
        private const float PageHeight = 842f;
        private const float Margin = 40f;

        private readonly List<StringBuilder> pages = new List<StringBuilder>();
        private StringBuilder page;
        private float y;
        private byte[] chartBytes;
        private int chartWidth;
        private int chartHeight;

        public static void Save(string filePath, DirectoryNode root, string scanPath,
            bool isNetworkDrive, string networkInfo, byte[] chartJpegBytes,
            int chartBitmapWidth, int chartBitmapHeight)
        {
            PdfReportExporter exporter = new PdfReportExporter();
            exporter.chartBytes = chartJpegBytes;
            exporter.chartWidth = chartBitmapWidth;
            exporter.chartHeight = chartBitmapHeight;
            exporter.Build(root, scanPath, isNetworkDrive, networkInfo);
            exporter.WritePdf(filePath);
        }

        private void Build(DirectoryNode root, string scanPath, bool isNetworkDrive, string networkInfo)
        {
            List<FileNode> allFiles = new List<FileNode>();
            CollectAllFiles(root, allFiles);

            List<FileNode> deletable = allFiles
                .Where(f => CleanupRules.IsStrictSafeCleanupCandidate(f))
                .OrderByDescending(f => f.Size).ToList();
            long deletableSize = deletable.Sum(f => f.Size);

            List<FileNode> bigOld = allFiles
                .Where(f => f.Size > 50 * 1024 * 1024 && f.LastModified < DateTime.Now.AddYears(-1))
                .OrderByDescending(f => f.Size).ToList();

            List<FileNode> topLargest = allFiles
                .OrderByDescending(f => f.Size).Take(30).ToList();

            List<FileNode> duplicateFiles = new List<FileNode>();
            foreach (DuplicateGroup group in BuildDuplicateGroups(allFiles).Take(15))
                duplicateFiles.AddRange(group.Files);

            List<DirectoryNode> bigDirs = root.SubDirectories
                .OrderByDescending(d => d.Size).Take(18).ToList();

            if (string.IsNullOrEmpty(scanPath))
                scanPath = root.Path;

            StartPage("Advanced Disk Analyzer Raporu");
            WriteLine("Rapor tarihi: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 10, false);
            WriteLine("Tarama klasörü: " + scanPath, 10, false);
            WriteLine("Toplam boyut: " + FormatSize(root.Size), 10, false);
            WriteLine("Diskte kaplanan: " + FormatSize(root.AllocatedSize), 10, false);
            WriteLine("Dosya sayısı: " + string.Format("{0:N0}", allFiles.Count), 10, false);
            WriteLine("Güvenli taşınabilir alan: " + FormatSize(deletableSize), 10, false);
            if (isNetworkDrive)
                WriteLine("Ağ sürücüsü / sunucu: " + networkInfo, 10, true);

            AddChart();
            AddFileTable("En Büyük Dosyalar", topLargest, "Dosya bulunamadı.");
            AddFileTable("Geri Dönüşüm Kutusuna Taşınabilecek Dosyalar", deletable.Take(30).ToList(), "Kesin güvenli temizlik dosyası bulunamadı.");
            AddFileTable("Büyük ve Eski Dosyalar", bigOld.Take(30).ToList(), "Büyük ve eski dosya bulunamadı.");
            AddFileTable("Olası Kopyalar", duplicateFiles.Take(45).ToList(), "Olası kopya bulunamadı.");
            AddDirectoryTable("En Büyük Klasörler", bigDirs, root.Size);
        }

        private static void CollectAllFiles(DirectoryNode node, List<FileNode> result)
        {
            if (node == null) return;
            result.AddRange(node.Files);
            foreach (DirectoryNode sub in node.SubDirectories)
                CollectAllFiles(sub, result);
        }

        private static List<DuplicateGroup> BuildDuplicateGroups(List<FileNode> files)
        {
            Dictionary<string, DuplicateGroup> map = new Dictionary<string, DuplicateGroup>(StringComparer.OrdinalIgnoreCase);
            foreach (FileNode file in files)
            {
                if (file.Size <= 0 || string.IsNullOrEmpty(file.Name)) continue;
                string key = file.Name.ToLowerInvariant() + "|" + file.Size.ToString(CultureInfo.InvariantCulture);
                DuplicateGroup group;
                if (!map.TryGetValue(key, out group))
                {
                    group = new DuplicateGroup();
                    group.Name = file.Name;
                    group.Size = file.Size;
                    map[key] = group;
                }
                group.Files.Add(file);
            }

            return map.Values
                .Where(g => g.Files.Count > 1)
                .OrderByDescending(g => g.WastedSize)
                .ToList();
        }

        private void StartPage(string title)
        {
            page = new StringBuilder();
            pages.Add(page);
            y = PageHeight - Margin;
            DrawTextAt(Margin, y, title, 16, true);
            y -= 24;
            DrawLine();
            y -= 18;
        }

        private void EnsureSpace(float needed)
        {
            if (page == null || y - needed < Margin)
                StartPage("Advanced Disk Analyzer Raporu - devam");
        }

        private void WriteLine(string text, int size, bool bold)
        {
            EnsureSpace(size + 10);
            DrawTextAt(Margin, y, text, size, bold);
            y -= size + 8;
        }

        private void AddSection(string title)
        {
            EnsureSpace(42);
            y -= 8;
            DrawTextAt(Margin, y, title, 12, true);
            y -= 16;
            DrawLine();
            y -= 12;
        }

        private void AddChart()
        {
            EnsureSpace(285);
            y -= 8;
            DrawTextAt(Margin, y, "Pasta Grafik", 12, true);
            y -= 22;

            float width = 390f;
            float height = chartHeight > 0 ? width * chartHeight / chartWidth : 225f;
            if (height > 235f) height = 235f;
            float x = (PageWidth - width) / 2f;
            float imageY = y - height;
            page.Append("q ");
            page.Append(F(width)); page.Append(" 0 0 ");
            page.Append(F(height)); page.Append(" ");
            page.Append(F(x)); page.Append(" ");
            page.Append(F(imageY)); page.Append(" cm /Im1 Do Q\n");
            y = imageY - 24;
        }

        private void AddFileTable(string title, List<FileNode> files, string emptyText)
        {
            AddSection(title);
            if (files.Count == 0)
            {
                WriteLine(emptyText, 9, false);
                return;
            }

            DrawFileHeader();
            foreach (FileNode f in files)
                DrawFileRow(f);
            y -= 8;
        }

        private void DrawFileHeader()
        {
            EnsureSpace(22);
            DrawFilledRect(Margin, y - 5, PageWidth - Margin * 2, 16, "0.93 0.94 0.97");
            DrawTextAt(44, y, "Dosya", 8, true);
            DrawTextAt(250, y, "Boyut", 8, true);
            DrawTextAt(318, y, "Skor", 8, true);
            DrawTextAt(360, y, "Tarih", 8, true);
            DrawTextAt(430, y, "Konum", 8, true);
            y -= 18;
        }

        private void DrawFileRow(FileNode f)
        {
            EnsureSpace(18);
            DrawTextAt(44, y, Truncate(f.Name, 34), 8, false);
            DrawTextAt(250, y, FormatSize(f.Size), 8, false);
            DrawTextAt(318, y, f.Score.ToString(), 8, false);
            DrawTextAt(360, y, f.LastModified.ToString("yyyy-MM-dd"), 8, false);
            DrawTextAt(430, y, Truncate(!string.IsNullOrEmpty(f.DirectoryPath) ? f.DirectoryPath : PathText.GetDirectoryName(f.FullPath), 24), 8, false);
            y -= 15;
        }

        private void AddDirectoryTable(string title, List<DirectoryNode> dirs, long rootSize)
        {
            AddSection(title);
            if (dirs.Count == 0)
            {
                WriteLine("Klasör verisi bulunamadı.", 9, false);
                return;
            }

            EnsureSpace(22);
            DrawFilledRect(Margin, y - 5, PageWidth - Margin * 2, 16, "0.93 0.94 0.97");
            DrawTextAt(44, y, "Klasör", 8, true);
            DrawTextAt(305, y, "Boyut", 8, true);
            DrawTextAt(395, y, "Oran", 8, true);
            DrawTextAt(455, y, "Konum", 8, true);
            y -= 18;

            foreach (DirectoryNode d in dirs)
            {
                EnsureSpace(18);
                double pct = rootSize > 0 ? d.Size / (double)rootSize * 100.0 : 0;
                DrawTextAt(44, y, Truncate(d.Name, 42), 8, false);
                DrawTextAt(305, y, FormatSize(d.Size), 8, false);
                DrawTextAt(395, y, pct.ToString("F1") + "%", 8, false);
                DrawTextAt(455, y, Truncate(d.Path, 22), 8, false);
                y -= 15;
            }
        }

        private void DrawTextAt(float x, float yPos, string text, int size, bool bold)
        {
            page.Append("BT /");
            page.Append(bold ? "F2" : "F1");
            page.Append(" ");
            page.Append(size.ToString(CultureInfo.InvariantCulture));
            page.Append(" Tf ");
            page.Append(F(x));
            page.Append(" ");
            page.Append(F(yPos));
            page.Append(" Td (");
            page.Append(EscapePdfText(text));
            page.Append(") Tj ET\n");
        }

        private void DrawLine()
        {
            page.Append("0.78 0.80 0.86 RG ");
            page.Append(F(Margin)); page.Append(" "); page.Append(F(y)); page.Append(" m ");
            page.Append(F(PageWidth - Margin)); page.Append(" "); page.Append(F(y)); page.Append(" l S 0 0 0 RG\n");
        }

        private void DrawFilledRect(float x, float yPos, float w, float h, string rgb)
        {
            page.Append(rgb);
            page.Append(" rg ");
            page.Append(F(x)); page.Append(" ");
            page.Append(F(yPos)); page.Append(" ");
            page.Append(F(w)); page.Append(" ");
            page.Append(F(h)); page.Append(" re f 0 0 0 rg\n");
        }

        private void WritePdf(string filePath)
        {
            int pageCount = pages.Count;
            int pageStartId = 6;
            int contentStartId = pageStartId + pageCount;
            int objectCount = 5 + pageCount * 2;
            byte[][] objects = new byte[objectCount + 1][];

            objects[1] = A("<< /Type /Catalog /Pages 2 0 R >>");

            StringBuilder kids = new StringBuilder();
            for (int i = 0; i < pageCount; i++)
                kids.Append(pageStartId + i).Append(" 0 R ");
            objects[2] = A("<< /Type /Pages /Kids [ " + kids.ToString() + "] /Count " + pageCount + " >>");
            objects[3] = A("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects[4] = A("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
            objects[5] = Combine(
                A("<< /Type /XObject /Subtype /Image /Width " + chartWidth +
                  " /Height " + chartHeight +
                  " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " +
                  chartBytes.Length + " >>\nstream\n"),
                chartBytes,
                A("\nendstream"));

            for (int i = 0; i < pageCount; i++)
            {
                int pageId = pageStartId + i;
                int contentId = contentStartId + i;
                objects[pageId] = A("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] " +
                    "/Resources << /Font << /F1 3 0 R /F2 4 0 R >> /XObject << /Im1 5 0 R >> >> " +
                    "/Contents " + contentId + " 0 R >>");

                byte[] stream = A(pages[i].ToString());
                objects[contentId] = Combine(
                    A("<< /Length " + stream.Length + " >>\nstream\n"),
                    stream,
                    A("\nendstream"));
            }

            long[] offsets = new long[objectCount + 1];
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                WriteBytes(fs, A("%PDF-1.4\n"));
                for (int i = 1; i <= objectCount; i++)
                    WriteObject(fs, i, objects[i], offsets);

                long xref = fs.Position;
                WriteBytes(fs, A("xref\n0 " + (objectCount + 1) + "\n"));
                WriteBytes(fs, A("0000000000 65535 f \n"));
                for (int i = 1; i <= objectCount; i++)
                    WriteBytes(fs, A(offsets[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n"));

                WriteBytes(fs, A("trailer\n<< /Size " + (objectCount + 1) + " /Root 1 0 R >>\nstartxref\n" +
                    xref.ToString(CultureInfo.InvariantCulture) + "\n%%EOF"));
            }
        }

        private static void WriteObject(FileStream fs, int id, byte[] body, long[] offsets)
        {
            offsets[id] = fs.Position;
            WriteBytes(fs, A(id.ToString(CultureInfo.InvariantCulture) + " 0 obj\n"));
            WriteBytes(fs, body);
            WriteBytes(fs, A("\nendobj\n"));
        }

        private static void WriteBytes(FileStream fs, byte[] bytes)
        {
            fs.Write(bytes, 0, bytes.Length);
        }

        private static byte[] A(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            int length = 0;
            foreach (byte[] arr in arrays)
                if (arr != null) length += arr.Length;
            byte[] result = new byte[length];
            int offset = 0;
            foreach (byte[] arr in arrays)
            {
                if (arr == null) continue;
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string EscapePdfText(string text)
        {
            text = ToAscii(text);
            return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static string ToAscii(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                switch (ch)
                {
                    case 'ç': case 'Ç': sb.Append('c'); break;
                    case 'ğ': case 'Ğ': sb.Append('g'); break;
                    case 'ı': case 'İ': case 'i': case 'I': sb.Append(ch == 'I' ? 'I' : 'i'); break;
                    case 'ö': case 'Ö': sb.Append('o'); break;
                    case 'ş': case 'Ş': sb.Append('s'); break;
                    case 'ü': case 'Ü': sb.Append('u'); break;
                    default:
                        sb.Append(ch >= 32 && ch <= 126 ? ch : '?');
                        break;
                }
            }
            return sb.ToString();
        }

        private static string Truncate(string text, int maxLength)
        {
            text = ToAscii(text);
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }

        private static string FormatSize(long size)
        {
            if (size > 1024L * 1024 * 1024) return (size / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
            if (size > 1024 * 1024)         return (size / (1024.0 * 1024)).ToString("F2") + " MB";
            if (size > 1024)                return (size / 1024.0).ToString("F2") + " KB";
            return size + " B";
        }
    }

    // =====================================================================
    // BÖLÜM: NATIVE VERİ MODELLERİ VE WIN32 API (P/INVOKE) BİLEŞENLERİ
    // =====================================================================
    // Amacı  : Windows çekirdek (Kernel32.dll ve Shell32.dll) kütüphanelerindeki
    //          düşük seviyeli Win32 I/O fonksiyonlarına doğrudan erişim sağlayarak
    //          standard .NET kütüphanelerinin (System.IO) getirdiği ek yükleri azaltır
    //          ve işletim sisteminin dosya indeksleme hızından maksimum düzeyde yararlanır.
    // İçerik : - Win32FindData           : Dosya özniteliklerini ve zaman damgalarını tutan C++ yapısı.
    //          - ByHandleFileInformation : Hard link tespiti ve tekil dosya kimlik tespiti yapısı.
    //          - NativeFileApi           : FindFirstFile, CreateFile ve SHFileOperation sarmalayıcıları.
    //          - PathText                : Garbage Collector yükünü azaltan hızlı metin bölme sınıfı.
    // =====================================================================

    /// <summary>
    /// Tarama esnasında bellek tahsisatını (allocation) azaltmak için kullanılan optimize dosya girdi modeli.
    /// </summary>
    public class FastFileEntry
    {
        public string Name;
        public string FullPath;
        public long Size;
        public DateTime LastWriteTime;
        public FileAttributes Attributes;
        public bool IsDirectory;
    }

    /// <summary>
    /// NTFS üzerindeki hard link'lenmiş dosyaların aynı veri bloğunu işaret ettiğini
    /// doğrulamak için kullanılan donanımsal dosya kimlik yapısı.
    /// </summary>
    public struct FileIdentity
    {
        public string Key; // Hacim Seri Numarası + Dosya İndeksi (VolumeSerial + FileIndex)
        public uint LinkCount;
    }

    /// <summary>
    /// Win32 FindFirstFile/FindNextFile API'leri tarafından doldurulan
    /// ve dosya metaverilerini barındıran ardışık (sequential) Win32 veri yapısı.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Win32FindData
    {
        public uint dwFileAttributes;
        public uint ftCreationTimeLow;
        public uint ftCreationTimeHigh;
        public uint ftLastAccessTimeLow;
        public uint ftLastAccessTimeHigh;
        public uint ftLastWriteTimeLow;
        public uint ftLastWriteTimeHigh;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    /// <summary>
    /// Windows dosya tablosundan doğrudan disk seri numarası ve index kimliği
    /// çekmek için kullanılan Win32 dosya bilgi yapısı.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ByHandleFileInformation
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint dwVolumeSerialNumber;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint nNumberOfLinks;
        public uint nFileIndexHigh;
        public uint nFileIndexLow;
    }

    /// <summary>
    /// Win32 tabanlı dosya arama, sıkıştırılmış dosya boyutu sorgulama,
    /// hard link kimlik eşleştirme ve Geri Dönüşüm Kutusu'na taşıma çağrılarını barındıran sınıf.
    /// </summary>
    public static class NativeFileApi
    {
        public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFile(string lpFileName, out Win32FindData lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextFile(IntPtr hFindFile, out Win32FindData lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetCompressedFileSize(string lpFileName, out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string lpFileName);

        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_NOERRORUI = 0x0400;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpace(string lpRootPathName,
            out uint lpSectorsPerCluster, out uint lpBytesPerSector,
            out uint lpNumberOfFreeClusters, out uint lpTotalNumberOfClusters);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out ByHandleFileInformation lpFileInformation);

        public static bool TryGetFileIdentity(string path, out FileIdentity identity)
        {
            identity = new FileIdentity();
            try
            {
                using (SafeFileHandle handle = CreateFile(ToExtendedPath(path), 0,
                    FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                    IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero))
                {
                    if (handle == null || handle.IsInvalid)
                        return false;

                    ByHandleFileInformation info;
                    if (!GetFileInformationByHandle(handle, out info))
                        return false;

                    identity.LinkCount = info.nNumberOfLinks;
                    identity.Key = info.dwVolumeSerialNumber.ToString("X8", CultureInfo.InvariantCulture) + ":" +
                                   info.nFileIndexHigh.ToString("X8", CultureInfo.InvariantCulture) +
                                   info.nFileIndexLow.ToString("X8", CultureInfo.InvariantCulture);
                    return true;
                }
            }
            catch { return false; }
        }

        public static bool MoveToRecycleBin(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            try
            {
                SHFILEOPSTRUCT op = new SHFILEOPSTRUCT();
                op.wFunc = FO_DELETE;
                op.pFrom = path + "\0\0";
                op.fFlags = (ushort)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI);
                int result = SHFileOperation(ref op);
                return result == 0 && !op.fAnyOperationsAborted;
            }
            catch { return false; }
        }

        public static string ToExtendedPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.StartsWith(@"\\?\", StringComparison.Ordinal)) return path;
            if (path.StartsWith(@"\\", StringComparison.Ordinal))
                return @"\\?\UNC\" + path.Substring(2);
            try
            {
                if (Path.IsPathRooted(path))
                    return @"\\?\" + path;
            }
            catch { }
            return path;
        }
    }

    public static class PathText
    {
        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            string trimmed = path.TrimEnd('\\', '/');
            if (trimmed.Length == 0) return path;
            int index = Math.Max(trimmed.LastIndexOf('\\'), trimmed.LastIndexOf('/'));
            if (index >= 0 && index < trimmed.Length - 1)
                return trimmed.Substring(index + 1);
            return trimmed;
        }

        public static string GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            string trimmed = path.TrimEnd('\\', '/');
            if (trimmed.Length == 0) return "";
            int index = Math.Max(trimmed.LastIndexOf('\\'), trimmed.LastIndexOf('/'));
            if (index < 0) return "";
            if (index == 2 && trimmed.Length > 1 && trimmed[1] == ':')
                return trimmed.Substring(0, 3);
            if (index == 0 && trimmed.StartsWith(@"\\", StringComparison.Ordinal))
                return trimmed;
            return trimmed.Substring(0, index);
        }

        public static string GetExtension(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            int slash = Math.Max(name.LastIndexOf('\\'), name.LastIndexOf('/'));
            int dot = name.LastIndexOf('.');
            if (dot <= slash || dot < 0 || dot == name.Length - 1) return "";
            return name.Substring(dot);
        }

        public static string GetRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.Length >= 2 && path[1] == ':')
                return path.Length >= 3 && (path[2] == '\\' || path[2] == '/') ? path.Substring(0, 3) : path.Substring(0, 2) + "\\";

            if (path.StartsWith(@"\\", StringComparison.Ordinal))
            {
                int serverEnd = path.IndexOf('\\', 2);
                if (serverEnd < 0) return path;
                int shareEnd = path.IndexOf('\\', serverEnd + 1);
                if (shareEnd < 0) return path;
                return path.Substring(0, shareEnd + 1);
            }

            return "";
        }

        public static string Combine(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent)) return child ?? "";
            if (string.IsNullOrEmpty(child)) return parent;
            char last = parent[parent.Length - 1];
            if (last == '\\' || last == '/')
                return parent + child;
            return parent + "\\" + child;
        }
    }

    // =====================================================================
    // BÖLÜM: NTFS TURBO TARAMA MOTORU (NtfsMftScanner)
    // =====================================================================
    // Amacı  : NTFS dosya sistemine sahip yerel disklerde, klasörleri tek tek dolaşmak
    //          (directory traversal) yerine, diskin en başındaki $MFT (Master File Table)
    //          meta-dosyasını sektör seviyesinde ham olarak okur. Bu sayede milyonlarca dosyayı
    //          birkaç saniye içerisinde belleğe alıp analiz edebilir. Yönetici yetkisi (Admin) gerektirir.
    // Yöntemi: 1. CreateFile ile mantıksal sürücü ("\\.\C:") raw disk okuma moduyla açılır.
    //          2. Disk sektör sıfırdan NTFS Boot Sector yapısı okunarak küme (cluster) boyutu
    //             ve $MFT başlangıç sektörü (MftStartOffset) çözümlenir.
    //          3. $MFT'nin kendi veri bloklarının disk üzerindeki dağılım haritası (Data Runs) çıkarılır.
    //          4. Tüm dosya kayıtları (MFT Records, 1024-byte bloklar) belleğe sıralı okunur.
    //          5. Okunan kayıtların imza düzeltmeleri (Fixup/USN) uygulanarak dosya ismi ($FILE_NAME),
    //             boyutu ($DATA) ve üst klasör ID'si (Parent Directory ID) ilişkilendirilip
    //             bellekte hiyerarşik ağaç yapısı oluşturulur.
    // =====================================================================

    /// <summary>
    /// Master File Table ($MFT) içindeki her bir dosya veya klasör kaydını temsil eden model.
    /// </summary>
    public class NtfsMftRecord
    {
        public long Id;
        public long ParentId;
        public string Name;
        public bool IsDirectory;
        public long Size;
        public long AllocatedSize;
        public DateTime LastModified;
        public FileAttributes Attributes;
        public ushort LinkCount;
    }

    /// <summary>
    /// NTFS veri parçalarının disk üzerindeki ardışık küme yerleşim haritası girdisi.
    /// </summary>
    public class NtfsDataRun
    {
        public long Lcn;            // Mantıksal Küme Numarası (Logical Cluster Number)
        public long ClusterLength;  // Küme cinsinden uzunluk
    }

    /// <summary>
    /// Raw disk okuma protokolünü uygulayan statik NTFS analiz motoru.
    /// </summary>
    public static class NtfsMftScanner
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const int FILE_RECORD_MAGIC = 0x454C4946; // "FILE" ASCII Magic Numarası
        private const int AttributeStandardInformation = 0x10;
        private const int AttributeFileName = 0x30;
        private const int AttributeData = 0x80;
        private const int AttributeEnd = unchecked((int)0xFFFFFFFF);
        private const int TurboCompactRecordThreshold = 300000;
        private const long TurboMaterializeMinBytes = 16L * 1024L * 1024L;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer,
            int nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove,
            out long lpNewFilePointer, uint dwMoveMethod);

        public static DirectoryNode TryScan(string scanPath, CancellationToken token,
            AdaptiveScoringModel scoringModel, DateTime oldFileThreshold, bool allocatedSizeEnabled,
            Action<FileNode> liveFile, out string message)
        {
            message = "";
            try
            {
                Stopwatch turboWatch = Stopwatch.StartNew();
                string root = PathText.GetRoot(scanPath);
                if (string.IsNullOrEmpty(root) || root.Length < 2 || root[1] != ':')
                {
                    message = "NTFS Turbo yalnızca yerel sürücülerde kullanılabilir.";
                    return null;
                }

                try
                {
                    DriveInfo drive = new DriveInfo(root);
                    if (!string.Equals(drive.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase))
                    {
                        message = "NTFS Turbo için sürücü NTFS olmalı.";
                        return null;
                    }
                }
                catch
                {
                    message = "Sürücü biçimi okunamadı.";
                    return null;
                }

                string volumePath = "\\\\.\\" + char.ToUpperInvariant(root[0]) + ":";
                using (SafeFileHandle volume = CreateFile(volumePath, GENERIC_READ,
                    FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                    IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero))
                {
                    if (volume == null || volume.IsInvalid)
                    {
                        message = "NTFS Turbo için disk erişimi açılamadı. Yönetici olarak çalıştırmayı deneyin.";
                        return null;
                    }

                    NtfsBootInfo boot;
                    if (!ReadBootInfo(volume, out boot))
                    {
                        message = "NTFS önyükleme bilgisi okunamadı.";
                        return null;
                    }

                    byte[] firstRecord = new byte[boot.RecordSize];
                    if (!ReadAt(volume, boot.MftStartOffset, firstRecord, firstRecord.Length))
                    {
                        message = "$MFT başlangıcı okunamadı.";
                        return null;
                    }

                    if (!ApplyFixup(firstRecord, 0, boot.RecordSize, boot.BytesPerSector))
                    {
                        message = "$MFT dosya kaydı doğrulanamadı.";
                        return null;
                    }

                    long mftBytes;
                    List<NtfsDataRun> runs = ReadMftDataRuns(firstRecord, boot, out mftBytes);
                    if (runs.Count == 0 || mftBytes <= 0)
                    {
                        message = "$MFT veri akışı çözümlenemedi.";
                        return null;
                    }

                    Dictionary<long, NtfsMftRecord> records = ReadRecords(volume, runs, boot, mftBytes, token);
                    double readSeconds = turboWatch.Elapsed.TotalSeconds;
                    if (records.Count == 0)
                    {
                        message = "$MFT içinde geçerli dosya kaydı bulunamadı.";
                        return null;
                    }

                    Dictionary<long, List<NtfsMftRecord>> children = BuildChildren(records);
                    long scanRootId = ResolveScanRootId(scanPath, root, children);
                    if (scanRootId < 0 || !records.ContainsKey(scanRootId))
                    {
                        message = "Seçilen klasör MFT içinde eşleştirilemedi.";
                        return null;
                    }

                    bool compactMode = records.Count >= TurboCompactRecordThreshold;
                    DirectoryNode rootNode = BuildDirectory(scanRootId, scanPath, records, children, scoringModel,
                        oldFileThreshold, allocatedSizeEnabled, compactMode, liveFile, token);
                    double totalSeconds = turboWatch.Elapsed.TotalSeconds;
                    if (rootNode == null)
                    {
                        message = "NTFS Turbo klasör ağacı oluşturamadı.";
                        return null;
                    }

                    message = "NTFS Turbo aktif: " + string.Format("{0:N0}", records.Count) +
                        " MFT kaydı okundu" + (compactMode ? " (hızlı önizleme)" : "") +
                        ". Okuma " + readSeconds.ToString("F1", CultureInfo.InvariantCulture) +
                        " sn, model " + (totalSeconds - readSeconds).ToString("F1", CultureInfo.InvariantCulture) + " sn.";
                    return rootNode;
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                message = "NTFS Turbo kullanılamadı: " + ex.Message;
                return null;
            }
        }

        private struct NtfsBootInfo
        {
            public int BytesPerSector;
            public int SectorsPerCluster;
            public int BytesPerCluster;
            public int RecordSize;
            public long MftStartOffset;
        }

        private static bool ReadBootInfo(SafeFileHandle volume, out NtfsBootInfo boot)
        {
            boot = new NtfsBootInfo();
            byte[] sector = new byte[512];
            if (!ReadAt(volume, 0, sector, sector.Length)) return false;
            string signature = Encoding.ASCII.GetString(sector, 3, 8).Trim();
            if (!string.Equals(signature, "NTFS", StringComparison.OrdinalIgnoreCase)) return false;

            boot.BytesPerSector = ReadUInt16(sector, 11);
            boot.SectorsPerCluster = sector[13];
            boot.BytesPerCluster = boot.BytesPerSector * boot.SectorsPerCluster;
            long mftCluster = ReadInt64(sector, 48);
            sbyte clustersPerRecord = unchecked((sbyte)sector[64]);
            boot.RecordSize = clustersPerRecord < 0
                ? 1 << -clustersPerRecord
                : clustersPerRecord * boot.BytesPerCluster;
            boot.MftStartOffset = mftCluster * (long)boot.BytesPerCluster;
            return boot.BytesPerSector > 0 && boot.BytesPerCluster > 0 &&
                   boot.RecordSize >= 512 && boot.MftStartOffset > 0;
        }

        private static List<NtfsDataRun> ReadMftDataRuns(byte[] record, NtfsBootInfo boot, out long mftBytes)
        {
            mftBytes = 0;
            List<NtfsDataRun> runs = new List<NtfsDataRun>();
            int attrOffset = ReadUInt16(record, 20);
            while (attrOffset > 0 && attrOffset + 16 < record.Length)
            {
                int type = ReadInt32(record, attrOffset);
                if (type == AttributeEnd) break;
                int length = ReadInt32(record, attrOffset + 4);
                if (length <= 0 || attrOffset + length > record.Length) break;
                bool nonResident = record[attrOffset + 8] != 0;
                if (type == AttributeData && nonResident)
                {
                    int runOffset = ReadUInt16(record, attrOffset + 32);
                    long realSize = ReadInt64(record, attrOffset + 48);
                    mftBytes = realSize;
                    runs = DecodeDataRuns(record, attrOffset + runOffset, attrOffset + length);
                    break;
                }
                attrOffset += length;
            }
            return runs;
        }

        private static Dictionary<long, NtfsMftRecord> ReadRecords(SafeFileHandle volume, List<NtfsDataRun> runs,
            NtfsBootInfo boot, long mftBytes, CancellationToken token)
        {
            Dictionary<long, NtfsMftRecord> records = new Dictionary<long, NtfsMftRecord>();
            int chunkSize = Math.Max(boot.RecordSize, (4 * 1024 * 1024 / boot.RecordSize) * boot.RecordSize);
            byte[] buffer = new byte[chunkSize];
            long streamOffset = 0;
            long recordIndex = 0;

            foreach (NtfsDataRun run in runs)
            {
                token.ThrowIfCancellationRequested();
                long runBytes = run.ClusterLength * (long)boot.BytesPerCluster;
                long remaining = Math.Min(runBytes, mftBytes - streamOffset);
                long diskOffset = run.Lcn * (long)boot.BytesPerCluster;

                while (remaining >= boot.RecordSize)
                {
                    token.ThrowIfCancellationRequested();
                    int wanted = (int)Math.Min(buffer.Length, remaining);
                    wanted = (wanted / boot.RecordSize) * boot.RecordSize;
                    if (wanted <= 0) break;
                    if (!ReadAt(volume, diskOffset, buffer, wanted)) break;

                    int recordsInChunk = wanted / boot.RecordSize;
                    for (int i = 0; i < recordsInChunk; i++)
                    {
                        int offset = i * boot.RecordSize;
                        NtfsMftRecord record = ParseRecord(buffer, offset, boot, recordIndex);
                        if (record != null && !records.ContainsKey(record.Id))
                            records.Add(record.Id, record);
                        recordIndex++;
                    }

                    diskOffset += wanted;
                    streamOffset += wanted;
                    remaining -= wanted;
                }
            }

            return records;
        }

        private static NtfsMftRecord ParseRecord(byte[] buffer, int offset, NtfsBootInfo boot, long recordIndex)
        {
            if (offset + boot.RecordSize > buffer.Length) return null;
            if (ReadInt32(buffer, offset) != FILE_RECORD_MAGIC) return null;
            if (!ApplyFixup(buffer, offset, boot.RecordSize, boot.BytesPerSector)) return null;

            ushort flags = ReadUInt16(buffer, offset + 22);
            bool inUse = (flags & 0x0001) != 0;
            if (!inUse) return null;

            long baseRef = (long)(ReadUInt64(buffer, offset + 32) & 0x0000FFFFFFFFFFFFUL);
            if (baseRef != 0) return null;

            NtfsMftRecord result = new NtfsMftRecord();
            result.Id = recordIndex;
            result.IsDirectory = (flags & 0x0002) != 0;
            result.LinkCount = ReadUInt16(buffer, offset + 18);
            result.LastModified = DateTime.MinValue;
            result.AllocatedSize = 0;
            result.Size = 0;

            int attrOffset = offset + ReadUInt16(buffer, offset + 20);
            int recordEnd = offset + boot.RecordSize;
            int bestNameRank = -1;

            while (attrOffset > offset && attrOffset + 16 < recordEnd)
            {
                int type = ReadInt32(buffer, attrOffset);
                if (type == AttributeEnd) break;
                int length = ReadInt32(buffer, attrOffset + 4);
                if (length <= 0 || attrOffset + length > recordEnd) break;
                bool nonResident = buffer[attrOffset + 8] != 0;

                if (type == AttributeStandardInformation && !nonResident)
                    ReadStandardInfo(buffer, attrOffset, result);
                else if (type == AttributeFileName && !nonResident)
                    ReadFileNameInfo(buffer, attrOffset, result, ref bestNameRank);
                else if (type == AttributeData)
                    ReadDataInfo(buffer, attrOffset, result, nonResident);

                attrOffset += length;
            }

            if (string.IsNullOrEmpty(result.Name)) return null;
            if (result.LastModified == DateTime.MinValue)
                result.LastModified = DateTime.Now;
            return result;
        }

        private static void ReadStandardInfo(byte[] buffer, int attrOffset, NtfsMftRecord result)
        {
            int valueLength = ReadInt32(buffer, attrOffset + 16);
            int valueOffset = ReadUInt16(buffer, attrOffset + 20);
            int value = attrOffset + valueOffset;
            if (valueLength < 32 || value + 32 > buffer.Length) return;
            result.LastModified = FileTimeToDateTime(ReadInt64(buffer, value + 16));
        }

        private static void ReadFileNameInfo(byte[] buffer, int attrOffset, NtfsMftRecord result, ref int bestNameRank)
        {
            int valueLength = ReadInt32(buffer, attrOffset + 16);
            int valueOffset = ReadUInt16(buffer, attrOffset + 20);
            int value = attrOffset + valueOffset;
            if (valueLength < 66 || value + valueLength > buffer.Length) return;

            int nameLength = buffer[value + 64];
            int nameSpace = buffer[value + 65];
            if (nameLength <= 0 || value + 66 + nameLength * 2 > buffer.Length) return;
            string name = Encoding.Unicode.GetString(buffer, value + 66, nameLength * 2);
            if (string.IsNullOrEmpty(name)) return;

            int rank = nameSpace == 1 || nameSpace == 3 ? 3 : (nameSpace == 0 ? 2 : 1);
            if (rank < bestNameRank) return;
            if (rank == bestNameRank && result.Name != null && result.Name.Length >= name.Length) return;

            result.ParentId = (long)(ReadUInt64(buffer, value) & 0x0000FFFFFFFFFFFFUL);
            result.Name = name;
            result.AllocatedSize = Math.Max(0, ReadInt64(buffer, value + 40));
            result.Size = Math.Max(0, ReadInt64(buffer, value + 48));
            result.Attributes = (FileAttributes)ReadUInt32(buffer, value + 56);
            if (result.LastModified == DateTime.MinValue)
                result.LastModified = FileTimeToDateTime(ReadInt64(buffer, value + 16));
            bestNameRank = rank;
        }

        private static void ReadDataInfo(byte[] buffer, int attrOffset, NtfsMftRecord result, bool nonResident)
        {
            byte nameLength = buffer[attrOffset + 9];
            if (nameLength != 0) return;

            if (nonResident)
            {
                if (attrOffset + 56 > buffer.Length) return;
                result.AllocatedSize = Math.Max(0, ReadInt64(buffer, attrOffset + 40));
                result.Size = Math.Max(0, ReadInt64(buffer, attrOffset + 48));
            }
            else
            {
                int valueLength = ReadInt32(buffer, attrOffset + 16);
                result.Size = Math.Max(0, valueLength);
                if (result.AllocatedSize <= 0) result.AllocatedSize = result.Size;
            }
        }

        private static Dictionary<long, List<NtfsMftRecord>> BuildChildren(Dictionary<long, NtfsMftRecord> records)
        {
            Dictionary<long, List<NtfsMftRecord>> children = new Dictionary<long, List<NtfsMftRecord>>();
            foreach (NtfsMftRecord record in records.Values)
            {
                if (record.Id == record.ParentId) continue;
                List<NtfsMftRecord> list;
                if (!children.TryGetValue(record.ParentId, out list))
                {
                    list = new List<NtfsMftRecord>();
                    children[record.ParentId] = list;
                }
                list.Add(record);
            }
            return children;
        }

        private static long ResolveScanRootId(string scanPath, string volumeRoot, Dictionary<long, List<NtfsMftRecord>> children)
        {
            string relative = scanPath;
            if (relative.StartsWith(volumeRoot, StringComparison.OrdinalIgnoreCase))
                relative = relative.Substring(volumeRoot.Length);
            relative = relative.Trim('\\', '/');
            long current = 5;
            if (relative.Length == 0) return current;

            string[] parts = relative.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                List<NtfsMftRecord> list;
                if (!children.TryGetValue(current, out list)) return -1;
                long next = -1;
                foreach (NtfsMftRecord child in list)
                {
                    if (child.IsDirectory && string.Equals(child.Name, parts[i], StringComparison.OrdinalIgnoreCase))
                    {
                        next = child.Id;
                        break;
                    }
                }
                if (next < 0) return -1;
                current = next;
            }
            return current;
        }

        private static DirectoryNode BuildDirectory(long id, string path, Dictionary<long, NtfsMftRecord> records,
            Dictionary<long, List<NtfsMftRecord>> children, AdaptiveScoringModel scoringModel,
            DateTime oldFileThreshold, bool allocatedSizeEnabled, bool compactMode, Action<FileNode> liveFile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NtfsMftRecord record;
            if (!records.TryGetValue(id, out record)) return null;

            DirectoryNode node = new DirectoryNode();
            node.Name = id == 5 ? path : record.Name;
            node.Path = path;

            List<NtfsMftRecord> list;
            if (!children.TryGetValue(id, out list)) return node;

            foreach (NtfsMftRecord child in list)
            {
                token.ThrowIfCancellationRequested();
                if (child.IsDirectory)
                {
                    string childPath = PathText.Combine(path, child.Name);
                    DirectoryNode sub = BuildDirectory(child.Id, childPath, records, children, scoringModel,
                        oldFileThreshold, allocatedSizeEnabled, compactMode, liveFile, token);
                    if (sub == null) continue;
                    node.SubDirectories.Add(sub);
                    node.Size += sub.Size;
                    node.AllocatedSize += sub.AllocatedSize;
                    node.FileCount += sub.FileCount;
                    if (sub.FilesArePartial) node.FilesArePartial = true;
                }
                else
                {
                    long logical = Math.Max(0, child.Size);
                    long allocated = allocatedSizeEnabled ? Math.Max(child.AllocatedSize, logical) : logical;
                    node.FileCount++;
                    node.Size += logical;
                    node.AllocatedSize += allocated;

                    string extension = PathText.GetExtension(child.Name).ToLowerInvariant();
                    bool materialize = !compactMode || ShouldMaterializeTurboFile(child, extension);
                    if (!materialize)
                    {
                        node.FilesArePartial = true;
                        continue;
                    }

                    string childPath = PathText.Combine(path, child.Name);
                    FileNode file = new FileNode();
                    file.Name = child.Name;
                    file.DirectoryPath = path;
                    file.FullPath = childPath;
                    file.Size = logical;
                    file.AllocatedSize = allocated;
                    file.CountedSize = logical;
                    file.CountedAllocatedSize = allocated;
                    file.LastModified = child.LastModified;
                    file.Extension = extension;
                    file.HardLinkCount = child.LinkCount;
                    file.Score = scoringModel.Score(file.Size, file.LastModified, file.Extension, file.Name, file.FullPath, oldFileThreshold);
                    node.Files.Add(file);
                    if (liveFile != null) liveFile(file);
                }
            }
            return node;
        }

        private static bool ShouldMaterializeTurboFile(NtfsMftRecord record, string extension)
        {
            if (record.Size >= TurboMaterializeMinBytes) return true;
            if (record.AllocatedSize >= TurboMaterializeMinBytes) return true;
            if (extension == ".tmp" || extension == ".log" || extension == ".bak" ||
                extension == ".old" || extension == ".dmp")
                return true;
            string name = record.Name ?? "";
            if (name.Length > 0 && name[0] == '~') return true;
            return false;
        }

        private static bool ApplyFixup(byte[] buffer, int offset, int recordSize, int bytesPerSector)
        {
            if (offset + recordSize > buffer.Length) return false;
            if (ReadInt32(buffer, offset) != FILE_RECORD_MAGIC) return false;
            int usaOffset = ReadUInt16(buffer, offset + 4);
            int usaCount = ReadUInt16(buffer, offset + 6);
            if (usaOffset <= 0 || usaCount <= 0 || offset + usaOffset + usaCount * 2 > offset + recordSize) return false;
            ushort usn = ReadUInt16(buffer, offset + usaOffset);
            for (int i = 1; i < usaCount; i++)
            {
                int sectorEnd = offset + i * bytesPerSector - 2;
                if (sectorEnd < offset || sectorEnd + 2 > offset + recordSize) return false;
                if (ReadUInt16(buffer, sectorEnd) != usn) return false;
                ushort replacement = ReadUInt16(buffer, offset + usaOffset + i * 2);
                buffer[sectorEnd] = (byte)(replacement & 0xFF);
                buffer[sectorEnd + 1] = (byte)((replacement >> 8) & 0xFF);
            }
            return true;
        }

        private static List<NtfsDataRun> DecodeDataRuns(byte[] buffer, int offset, int end)
        {
            List<NtfsDataRun> runs = new List<NtfsDataRun>();
            long currentLcn = 0;
            int p = offset;
            while (p < end)
            {
                int header = buffer[p++];
                if (header == 0) break;
                int lengthBytes = header & 0x0F;
                int offsetBytes = (header >> 4) & 0x0F;
                if (lengthBytes == 0 || p + lengthBytes + offsetBytes > end) break;

                long clusterLength = ReadVariableUInt(buffer, p, lengthBytes);
                p += lengthBytes;
                long lcnDelta = ReadVariableInt(buffer, p, offsetBytes);
                p += offsetBytes;
                currentLcn += lcnDelta;
                if (clusterLength > 0 && currentLcn > 0)
                {
                    NtfsDataRun run = new NtfsDataRun();
                    run.Lcn = currentLcn;
                    run.ClusterLength = clusterLength;
                    runs.Add(run);
                }
            }
            return runs;
        }

        private static bool ReadAt(SafeFileHandle handle, long offset, byte[] buffer, int count)
        {
            long newPosition;
            if (!SetFilePointerEx(handle, offset, out newPosition, 0)) return false;
            int read;
            return ReadFile(handle, buffer, count, out read, IntPtr.Zero) && read == count;
        }

        private static DateTime FileTimeToDateTime(long fileTime)
        {
            try
            {
                if (fileTime <= 0) return DateTime.MinValue;
                return DateTime.FromFileTimeUtc(fileTime).ToLocalTime();
            }
            catch { return DateTime.MinValue; }
        }

        private static long ReadVariableUInt(byte[] buffer, int offset, int count)
        {
            long value = 0;
            for (int i = 0; i < count; i++)
                value |= ((long)buffer[offset + i]) << (8 * i);
            return value;
        }

        private static long ReadVariableInt(byte[] buffer, int offset, int count)
        {
            if (count == 0) return 0;
            long value = ReadVariableUInt(buffer, offset, count);
            long signBit = 1L << (count * 8 - 1);
            if ((value & signBit) != 0)
                value |= -1L << (count * 8);
            return value;
        }

        private static ushort ReadUInt16(byte[] b, int o)
        {
            return (ushort)(b[o] | (b[o + 1] << 8));
        }

        private static uint ReadUInt32(byte[] b, int o)
        {
            return (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
        }

        private static int ReadInt32(byte[] b, int o)
        {
            return unchecked((int)ReadUInt32(b, o));
        }

        private static ulong ReadUInt64(byte[] b, int o)
        {
            uint low = ReadUInt32(b, o);
            uint high = ReadUInt32(b, o + 4);
            return ((ulong)high << 32) | low;
        }

        private static long ReadInt64(byte[] b, int o)
        {
            return unchecked((long)ReadUInt64(b, o));
        }
    }

    // =====================================================================
    // BÖLÜM: TEMİZLİK KURALLARI MOTORU (CleanupRules)
    // =====================================================================
    // Amacı  : Disk analizinden sonra, hangi dosyaların "kesin güvenli", hangilerinin
    //          "riskli / sistem dosyası" veya "kullanıcı şahsi verisi" olduğunu analiz ederek
    //          yanlışlıkla kritik dosyaların silinmesini önler.
    // Kurallar: 1. Sistem dosyası koruması (Protected System Path): Windows, Program Files,
    //             System Volume Information altındaki hiçbir dosya otomatik olarak silinmez.
    //          2. Şahsi veri koruması (User Content): Masaüstü, Belgeler, Resimler,
    //             Downloads gibi doğrudan kullanıcıya ait klasörler otomatik silme kapsamı dışındadır.
    //          3. Güvenli silinebilir uzantılar: .tmp, .bak, .old, .dmp ve Temp klasörü altındaki
    //             belirli bir tarihten eski (ör. 7 gün) log dosyaları.
    // =====================================================================
    public static class CleanupRules
    {
        private static readonly string WindowsPath = Lower(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        private static readonly string ProgramFilesPath = Lower(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        private static readonly string ProgramFilesX86Path = Lower(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

        public static bool IsSafeCleanupCandidate(FileNode file)
        {
            if (file == null) return false;
            return IsSafeCleanupCandidate(file.Extension, file.Name, file.FullPath);
        }

        public static bool IsStrictSafeCleanupCandidate(FileNode file)
        {
            if (file == null || string.IsNullOrEmpty(file.FullPath)) return false;
            if (file.SharedHardLink || file.HardLinkCount > 1) return false;
            if (IsProtectedSystemPath(file.FullPath)) return false;

            string ext = NormalizeExtension(file.Extension);
            string path = Lower(file.FullPath);
            string name = Lower(file.Name);
            if (IsNeverAutoDeleteExtension(ext)) return false;
            if (IsUserContentPath(path) && path.IndexOf("\\$recycle.bin\\") < 0) return false;

            int ageDays = AgeDays(file.LastModified);
            if (path.IndexOf("\\$recycle.bin\\") >= 0)
                return ageDays >= 1;

            if (IsTemporaryLocation(path))
            {
                if (ageDays < 7) return false;
                if (ext == ".tmp" || ext == ".temp" || ext == ".log" || ext == ".dmp") return true;
                if ((ext == ".bak" || ext == ".old") && ageDays >= 30) return true;
                if (!string.IsNullOrEmpty(name) && name.StartsWith("~")) return true;
                return false;
            }

            if (IsCacheLocation(path))
                return ageDays >= 14 && !IsInstallerOrUserDataExtension(ext);

            if (path.IndexOf("\\logs\\") >= 0 || path.EndsWith("\\logs"))
                return ext == ".log" && ageDays >= 14;

            if (ext == ".dmp" && ageDays >= 7 && !IsUserContentPath(path))
                return true;

            return false;
        }

        public static bool IsReviewCleanupCandidate(FileNode file)
        {
            if (file == null || IsStrictSafeCleanupCandidate(file)) return false;
            if (!IsSafeCleanupCandidate(file)) return false;
            if (IsProtectedSystemPath(file.FullPath)) return false;
            string path = Lower(file.FullPath);
            if (IsUserContentPath(path)) return false;
            return true;
        }

        public static bool IsSafeCleanupCandidate(string extension, string name, string fullPath)
        {
            string ext = NormalizeExtension(extension);
            string lowerPath = Lower(fullPath);
            string lowerName = Lower(name);

            if (ext == ".tmp" || ext == ".bak" || ext == ".old" || ext == ".dmp") return true;
            if (ext == ".log" && (IsTemporaryLocation(lowerPath) || lowerPath.IndexOf("\\logs\\") >= 0)) return true;
            if (!string.IsNullOrEmpty(lowerName) && lowerName.StartsWith("~")) return true;
            if (IsTemporaryLocation(lowerPath)) return true;
            if (IsCacheLocation(lowerPath)) return true;
            if (lowerPath.IndexOf("\\$recycle.bin\\") >= 0) return true;
            return false;
        }

        public static bool IsProtectedSystemPath(string fullPath)
        {
            string path = Lower(fullPath);
            if (string.IsNullOrEmpty(path)) return false;
            if (IsTemporaryLocation(path) || path.IndexOf("\\$recycle.bin\\") >= 0) return false;

            if (!string.IsNullOrEmpty(WindowsPath) && path.StartsWith(WindowsPath)) return true;
            if (!string.IsNullOrEmpty(ProgramFilesPath) && path.StartsWith(ProgramFilesPath)) return true;
            if (!string.IsNullOrEmpty(ProgramFilesX86Path) && path.StartsWith(ProgramFilesX86Path)) return true;
            if (path.IndexOf("\\system volume information\\") >= 0) return true;
            return false;
        }

        public static double LocationScore(string fullPath)
        {
            string path = Lower(fullPath);
            if (string.IsNullOrEmpty(path)) return 0.0;
            if (path.IndexOf("\\$recycle.bin\\") >= 0) return 1.0;
            if (IsTemporaryLocation(path)) return 0.95;
            if (IsCacheLocation(path)) return 0.80;
            if (path.IndexOf("\\downloads\\") >= 0) return 0.35;
            if (path.IndexOf("\\logs\\") >= 0) return 0.35;
            return 0.0;
        }

        public static bool HasCleanupHint(string extension, string name, string fullPath)
        {
            return HasCleanupHint(extension, name, fullPath, LocationScore(fullPath));
        }

        public static bool HasCleanupHint(string extension, string name, string fullPath, double locationScore)
        {
            string ext = NormalizeExtension(extension);
            string lowerName = Lower(name);
            if (ext == ".tmp" || ext == ".log" || ext == ".bak" || ext == ".old" || ext == ".dmp") return true;
            if (!string.IsNullOrEmpty(lowerName) && lowerName.StartsWith("~")) return true;
            return locationScore >= 0.75;
        }

        public static bool IsArchiveCandidate(FileNode file, DateTime threshold)
        {
            if (file == null || file.Size < 100L * 1024L * 1024L) return false;
            if (file.LastModified > threshold) return false;
            string ext = NormalizeExtension(file.Extension);
            string path = Lower(file.FullPath);
            if (ext == ".zip" || ext == ".rar" || ext == ".7z" ||
                ext == ".iso" || ext == ".img" || ext == ".wim" ||
                ext == ".mp4" || ext == ".mkv" || ext == ".mov" ||
                ext == ".avi" || ext == ".psd" || ext == ".blend" ||
                ext == ".bak" || ext == ".old")
                return true;
            return path.IndexOf("\\downloads\\") >= 0 || path.IndexOf("\\desktop\\") >= 0;
        }

        public static string CleanupReason(FileNode file)
        {
            if (file == null) return "Güvenli temizlik sinyali";
            string ext = NormalizeExtension(file.Extension);
            string path = Lower(file.FullPath);
            string name = Lower(file.Name);
            if (path.IndexOf("\\$recycle.bin\\") >= 0) return "Geri dönüşüm kutusu kalıntısı";
            if (IsTemporaryLocation(path)) return "Geçici klasör dosyası";
            if (path.IndexOf("\\cache\\") >= 0 || path.IndexOf("\\caches\\") >= 0) return "Önbellek dosyası";
            if (ext == ".tmp") return "Geçici dosya uzantısı";
            if (ext == ".log") return "Log dosyası";
            if (ext == ".bak" || ext == ".old") return "Eski yedek dosyası";
            if (ext == ".dmp") return "Hata dökümü dosyası";
            if (!string.IsNullOrEmpty(name) && name.StartsWith("~")) return "Geçici adlandırma sinyali";
            return "Düşük riskli temizlik sinyali";
        }

        public static string CleanupConfidence(FileNode file)
        {
            if (file == null) return "Güven: bilinmiyor";
            if (IsStrictSafeCleanupCandidate(file))
                return "Güven: yüksek";
            if (IsReviewCleanupCandidate(file))
                return "Güven: incele";
            return "Güven: düşük";
        }

        public static string ArchiveReason(FileNode file)
        {
            if (file == null) return "Eski ve büyük dosya";
            string ext = NormalizeExtension(file.Extension);
            if (ext == ".mp4" || ext == ".mkv" || ext == ".mov" || ext == ".avi")
                return "Eski büyük medya dosyası";
            if (ext == ".zip" || ext == ".rar" || ext == ".7z")
                return "Eski arşiv paketi";
            if (ext == ".iso" || ext == ".img" || ext == ".wim")
                return "Disk imajı / kurulum arşivi";
            if (ext == ".psd" || ext == ".blend")
                return "Büyük proje dosyası";
            return "Eski ve büyük dosya";
        }

        private static bool IsTemporaryLocation(string lowerPath)
        {
            if (string.IsNullOrEmpty(lowerPath)) return false;
            return lowerPath.IndexOf("\\temp\\") >= 0 ||
                   lowerPath.IndexOf("\\tmp\\") >= 0 ||
                   lowerPath.EndsWith("\\temp") ||
                   lowerPath.EndsWith("\\tmp");
        }

        private static bool IsCacheLocation(string lowerPath)
        {
            if (string.IsNullOrEmpty(lowerPath)) return false;
            return lowerPath.IndexOf("\\cache\\") >= 0 ||
                   lowerPath.IndexOf("\\caches\\") >= 0 ||
                   lowerPath.IndexOf("\\code cache\\") >= 0 ||
                   lowerPath.IndexOf("\\shadercache\\") >= 0 ||
                   lowerPath.IndexOf("\\shader-cache\\") >= 0;
        }

        private static bool IsUserContentPath(string lowerPath)
        {
            if (string.IsNullOrEmpty(lowerPath)) return false;
            return lowerPath.IndexOf("\\desktop\\") >= 0 ||
                   lowerPath.IndexOf("\\documents\\") >= 0 ||
                   lowerPath.IndexOf("\\downloads\\") >= 0 ||
                   lowerPath.IndexOf("\\pictures\\") >= 0 ||
                   lowerPath.IndexOf("\\videos\\") >= 0 ||
                   lowerPath.IndexOf("\\music\\") >= 0 ||
                   lowerPath.IndexOf("\\onedrive\\") >= 0;
        }

        private static bool IsNeverAutoDeleteExtension(string ext)
        {
            return ext == ".exe" || ext == ".dll" || ext == ".sys" || ext == ".msi" ||
                   ext == ".msp" || ext == ".ocx" || ext == ".drv" || ext == ".bat" ||
                   ext == ".cmd" || ext == ".ps1" || ext == ".reg" || ext == ".ini" ||
                   ext == ".config" || ext == ".db" || ext == ".sqlite" || ext == ".pst" ||
                   ext == ".ost" || ext == ".doc" || ext == ".docx" || ext == ".xls" ||
                   ext == ".xlsx" || ext == ".ppt" || ext == ".pptx" || ext == ".pdf" ||
                   ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".mp4" ||
                   ext == ".zip" || ext == ".rar" || ext == ".7z" || ext == ".iso";
        }

        private static bool IsInstallerOrUserDataExtension(string ext)
        {
            return IsNeverAutoDeleteExtension(ext) ||
                   ext == ".pak" || ext == ".dat" || ext == ".bin" || ext == ".vhd" ||
                   ext == ".vhdx" || ext == ".wim";
        }

        private static int AgeDays(DateTime date)
        {
            if (date == DateTime.MinValue) return 0;
            double days = (DateTime.Now - date).TotalDays;
            if (days < 0) return 0;
            if (days > int.MaxValue) return int.MaxValue;
            return (int)days;
        }

        private static string NormalizeExtension(string extension)
        {
            return string.IsNullOrEmpty(extension) ? "" : extension.ToLowerInvariant();
        }

        private static string Lower(string value)
        {
            return string.IsNullOrEmpty(value) ? "" : value.ToLowerInvariant();
        }
    }

    // =====================================================================
    // BÖLÜM: ADAPTİF PUANLAMA/SKORLAMA MODELİ (AdaptiveScoringModel)
    // =====================================================================
    // Amacı  : Her bir dosyanın silinmeye veya arşivlenmeye ne kadar uygun olduğunu
    //          gösteren 0 ile 100 arasında ağırlıklı bir "Gereksizlik Skoru" üretir.
    // Metot  : İstatistiki ağırlıklandırma (weighted sum) modeli kullanılır:
    //          - Boyut Skoru (%34)  : Dosya boyutu büyüdükçe logaritmik artış.
    //          - Yaş Skoru (%24)    : Son değişiklik tarihinin eskiliğine göre doğrusal artış.
    //          - Tip/İpucu (%24)    : Uzantı .tmp/.bak/.log ise doğrudan tetiklenir.
    //          - Konum Skoru (%18)  : Temp veya recycle bin gibi geçici dizinlerde olma durumu.
    //          - Koruma Cezası (-%28): Windows veya Program Files altındaysa skor düşürülür.
    // =====================================================================
    public class AdaptiveScoringModel
    {
        private static readonly double SizeScoreLogDenominator = Math.Log((5.0 * 1024.0 * 1024.0) + 1.0);

        public int Score(FileInfo file)
        {
            return Score(file.Length, file.LastWriteTime, file.Extension, file.Name, file.FullName, DateTime.Now.AddYears(-1));
        }

        public int Score(long size, DateTime lastWriteTime, string extension, string name, DateTime oldFileThreshold)
        {
            return Score(size, lastWriteTime, extension, name, "", oldFileThreshold);
        }

        public int Score(long size, DateTime lastWriteTime, string extension, string name, string fullPath, DateTime oldFileThreshold)
        {
            double kb = Math.Max(1.0, size / 1024.0);
            double sizeScore = Math.Min(1.0, Math.Log(kb + 1.0) / SizeScoreLogDenominator);
            double ageScore = lastWriteTime < oldFileThreshold
                ? Math.Min(1.0, Math.Max(0.0, (oldFileThreshold - lastWriteTime).TotalDays) / 365.0)
                : 0.0;
            double locationScore = CleanupRules.LocationScore(fullPath);
            double cleanupScore = CleanupRules.HasCleanupHint(extension, name, fullPath, locationScore) ? 1.0 : 0.0;
            double protectedPenalty = CleanupRules.IsProtectedSystemPath(fullPath) ? 1.0 : 0.0;

            double score = (sizeScore * 34.0) +
                           (ageScore * 24.0) +
                           (cleanupScore * 24.0) +
                           (locationScore * 18.0) -
                           (protectedPenalty * 28.0);

            if (score < 0.0) score = 0.0;
            if (score > 100.0) score = 100.0;
            return (int)Math.Round(score);
        }
    }

    /// <summary>
    /// Disk tarama ağacındaki bir klasör düğümünü temsil eden veri yapısı.
    /// </summary>
    public class DirectoryNode
    {
        public string Name; public string Path; public long Size; public long AllocatedSize;
        public int FileCount; public bool FilesArePartial;
        public List<FileNode> Files = new List<FileNode>();
        public List<DirectoryNode> SubDirectories = new List<DirectoryNode>();
    }

    /// <summary>
    /// Disk tarama ağacındaki bir dosya yaprağını temsil eden veri yapısı.
    /// </summary>
    public class FileNode
    {
        public string Name; public string FullPath; public long Size;
        public string DirectoryPath;
        public long AllocatedSize; public long CountedSize; public long CountedAllocatedSize;
        public int Score; public DateTime LastModified; public string Extension;
        public bool SharedHardLink; public uint HardLinkCount; public string FileIdKey;
    }

    public class DuplicateGroup
    {
        public string Name;
        public long Size;
        public List<FileNode> Files = new List<FileNode>();
        public long WastedSize
        {
            get { return Size * Math.Max(0, Files.Count - 1); }
        }
    }

    public class TreemapItem
    {
        public string Label;
        public long Size;
        public DirectoryNode Directory;
    }

    public class TreemapTile
    {
        public Rectangle Bounds;
        public string Label;
        public long Size;
        public DirectoryNode Directory;
    }

    // =====================================================================
    // BÖLÜM: UYGULAMA GİRİŞ NOKTASI VE BEKLENMEYEN HATA YAKALAYICILAR
    // =====================================================================
    // Amacı  : Programın ana başlatıcısıdır (Main). Ayrıca runtime (çalışma zamanı)
    //          sırasında oluşabilecek kritik veya thread bazlı beklenmeyen hataları
    //          (unhandled exceptions) yakalayarak uygulamanın aniden kapanmasını
    //          engeller ve hata dökümünü local log dosyasına kaydeder.
    // =====================================================================
    static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası. Single Thread Apartment (STAThread) modelini uygular.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Hata yakalama delegasyonlarının atanması
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
            {
                HandleUnexpectedException(e.Exception, true);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                HandleUnexpectedException(e.ExceptionObject as Exception, false);
            };
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Çalışma zamanı hatalarını kullanıcıya bildirir ve log dosyasına yazılmasını sağlar.
        /// </summary>
        private static void HandleUnexpectedException(Exception ex, bool canContinue)
        {
            try { WriteCrashLog(ex); } catch { }
            string message = ex == null ? "Bilinmeyen bir hata oluştu." : ex.Message;
            string text = canContinue
                ? "Beklenmeyen bir durum yakalandı. Uygulama devam etmeyi deneyecek.\n\n" + message
                : "Beklenmeyen bir kritik durum oluştu. Uygulama kapanabilir.\n\n" + message;
            try
            {
                MessageBox.Show(text, "Advanced Disk Analyzer", MessageBoxButtons.OK,
                    canContinue ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
            }
            catch { }
        }

        /// <summary>
        /// Yakalanan hatanın detaylarını (Stack Trace dahil) Yerel Uygulama Verileri
        /// (LocalAppData/AdvancedDiskAnalyzer/Logs/crash.log) dizinine yazar.
        /// </summary>
        private static void WriteCrashLog(Exception ex)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AdvancedDiskAnalyzer", "Logs");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "crash.log");
            using (StreamWriter writer = new StreamWriter(file, true, new UTF8Encoding(true)))
            {
                writer.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                writer.WriteLine(ex == null ? "Bilinmeyen hata" : ex.ToString());
                writer.WriteLine();
            }
        }
    }
}
