// Advanced Disk Analyzer v1.7 - Modern Edition

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
using System.Text;
using System.Globalization;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Specialized;

namespace AdvancedDiskAnalyzer
{
    public static class Theme
    {
        public static bool IsDark = true;

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
        public static Color LightSubText   = Color.FromArgb(88, 88, 128);
        public static Color LightAccent    = Color.FromArgb(38, 104, 190);
        public static Color LightAccent2   = Color.FromArgb(92, 105, 135);
        public static Color LightSuccess   = Color.FromArgb(28, 155, 85);
        public static Color LightDanger    = Color.FromArgb(205, 38, 58);
        public static Color LightWarning   = Color.FromArgb(195, 125, 0);
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

    public class MainForm : Form
    {
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
        private Panel toolbarPanel;
        private Panel piePanel;
        private Panel treemapPanel;
        private Panel aiHost;
        private Panel aiPanel;
        private TabControl tabControl;
        private ProgressBar progressBar;
        private Button scanButton;
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

        // Ag surucusu bilgisi
        private bool isNetworkDrive = false;
        private string networkDriveInfo = "";

        // Ag surucusu bilgi banner'i
        private Panel networkBanner;
        private Label networkBannerLabel;

        private AdaptiveScoringModel scoringModel = new AdaptiveScoringModel();
        private bool isSSD = false;
        private Dictionary<string, bool> driveTypeCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private DirectoryNode rootNode = null;
        private int totalFilesFound = 0;
        private long totalBytesScanned = 0;
        private System.Windows.Forms.Timer uiTimer;
        private System.Windows.Forms.Timer filterTimer;

        private readonly object queueLock = new object();
        private Queue<FileNode> pendingFiles = new Queue<FileNode>();
        private int liveQueued = 0;
        private bool liveListLimited = false;
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
        private bool ultraFastMode = true;
        private DateTime scoreOldFileThreshold = DateTime.Now.AddYears(-1);

        private const string AppVersion = "v1.7 Beta";
        private const string EulaRegistryPath = @"Software\AdvancedDiskAnalyzer";
        private const string EulaStampDate = "2026-05-07";
        private const string PurchaseUrl = "https://advanced-disk-analyzer.com/pricing";
        private const string EnterpriseContactUrl = "mailto:sales@advanced-disk-analyzer.com?subject=Advanced%20Disk%20Analyzer%20Enterprise";
        private const int LiveListItemLimit = 4000;
        private const int UltraLiveListItemLimit = 1600;
        private const int DisplayFileLimit = 10000;
        private readonly string[] listColumnTitles = new string[]
        {
            "Dosya Adı", "Boyut", "Skor", "Tarih", "Tur", "Konum"
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

            filterLabel.Left = 28;
            filterLabel.Top = 26;
            filterBox.Left = 68;
            filterBox.Top = 21;
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
            file.DropDownItems.Add(MenuCommand("CSV Dısarı Aktar", CsvButton_Click, Keys.Control | Keys.E));
            file.DropDownItems.Add(MenuCommand("PDF Rapor Al", PdfReportBtn_Click, Keys.Control | Keys.P));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(MenuCommand("çıkış", delegate { this.Close(); }, Keys.Alt | Keys.F4));

            ToolStripMenuItem view = MenuRoot("Görünüm");
            view.DropDownItems.Add(MenuCommand("Grafik Paneli", delegate { tabControl.SelectedIndex = 0; }, Keys.Control | Keys.D1));
            view.DropDownItems.Add(MenuCommand("Treemap Paneli", delegate { tabControl.SelectedIndex = 1; }, Keys.Control | Keys.D2));
            view.DropDownItems.Add(MenuCommand("AI öneriler", delegate { tabControl.SelectedIndex = 2; }, Keys.Control | Keys.D3));
            view.DropDownItems.Add(new ToolStripSeparator());
            view.DropDownItems.Add(MenuCommand("Temayı Değiştir", ThemeButton_Click, Keys.Control | Keys.T));

            ToolStripMenuItem tools = MenuRoot("Araçlar");
            tools.DropDownItems.Add(MenuCommand("Top 100 Büyük Dosya", TopFilesButton_Click, Keys.F6));
            tools.DropDownItems.Add(MenuCommand("Kopyaları Bul", DuplicatesButton_Click, Keys.F7));
            tools.DropDownItems.Add(MenuCommand("Listeyi Yenile", delegate { RefreshCurrentView(); }, Keys.F5));
            tools.DropDownItems.Add(new ToolStripSeparator());
            ultraFastScanMenuItem = MenuCommand("Ultra Hızlı Tarama", delegate { }, Keys.Control | Keys.U);
            ultraFastScanMenuItem.CheckOnClick = true;
            ultraFastScanMenuItem.Checked = ultraFastMode;
            ultraFastScanMenuItem.CheckedChanged += ToggleUltraFastScan_Click;
            tools.DropDownItems.Add(ultraFastScanMenuItem);
            tools.DropDownItems.Add(MenuCommand("Tarama Geçmişini Aç", OpenScanHistory_Click, Keys.Control | Keys.G));

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
            statusLabel.Text = ultraFastMode
                ? "Ultra hızlı tarama açık: UI akışı sınırlı, tarama öncelikli."
                : "Standart tarama açık: daha fazla canlı liste gösterilir.";
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
            titleLabel.Text = "Disk Analyzer Pro";
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
            filterLabel.Text = "ARA";
            filterLabel.Location = new Point(28, 26);
            filterLabel.Size = new Size(34, 18);
            filterLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            toolbarPanel.Controls.Add(filterLabel);

            filterBox = new TextBox();
            filterBox.Location = new Point(68, 21);
            filterBox.Size = new Size(265, 24);
            filterBox.BorderStyle = BorderStyle.FixedSingle;
            filterBox.Font = new Font("Segoe UI", 9);
            filterBox.TextChanged += FilterBox_TextChanged;
            toolbarPanel.Controls.Add(filterBox);

            topFilesButton = new Button();
            topFilesButton.Text = "TOP 100";
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
            statusLabel.Text = "Klasor secmek icin Yeni Tarama butonuna basin.";
            toolbarPanel.Controls.Add(statusLabel);

            liveCountLabel = new Label();
            liveCountLabel.Location = new Point(800, 50);
            liveCountLabel.Width = 300;
            liveCountLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            liveCountLabel.Text = "";
            toolbarPanel.Controls.Add(liveCountLabel);
            LayoutToolbar();

            // --- Ağ Sürücüsü Banner (basta gizli) ---
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
            statTotal.Location = new Point(15, 7); statTotal.Width = 220;
            statTotal.Font = new Font("Courier New", 8);
            statsBar.Controls.Add(statTotal);

            statFiles = new Label();
            statFiles.Location = new Point(245, 7); statFiles.Width = 200;
            statFiles.Font = new Font("Courier New", 8);
            statsBar.Controls.Add(statFiles);

            statTime = new Label();
            statTime.Location = new Point(455, 7); statTime.Width = 200;
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
            Button navDashboard = CreateSidebarNavButton("Dashboard", 12, true);
            navDashboard.Click += delegate { if (rootNode != null) { BuildPieData(rootNode); piePanel.Tag = rootNode; currentVisualNode = rootNode; tabControl.SelectedIndex = 0; piePanel.Invalidate(); } };
            Button navFiles = CreateSidebarNavButton("Dosyalar", 56, false);
            navFiles.Click += delegate { treeView.Focus(); };
            Button navAnalytics = CreateSidebarNavButton("Analizler", 100, false);
            navAnalytics.Click += delegate { tabControl.SelectedIndex = 2; };
            Button navSettings = CreateSidebarNavButton("Hesap ve Plan", 144, false);
            navSettings.Click += AccountButton_Click;
            sidebarNavPanel.Controls.Add(navDashboard);
            sidebarNavPanel.Controls.Add(navFiles);
            sidebarNavPanel.Controls.Add(navAnalytics);
            sidebarNavPanel.Controls.Add(navSettings);

            sidebarFooterPanel = new Panel();
            sidebarFooterPanel.Dock = DockStyle.Bottom;
            sidebarFooterPanel.Height = 126;
            scanButton.Location = new Point(20, 20);
            scanButton.Size = new Size(258, 44);
            sidebarFooterPanel.Controls.Add(scanButton);
            Label footerHelp = new Label();
            footerHelp.Text = "Support";
            footerHelp.Location = new Point(24, 78);
            footerHelp.Size = new Size(110, 20);
            footerHelp.Font = new Font("Segoe UI", 8);
            footerHelp.Tag = "sidebar-muted";
            sidebarFooterPanel.Controls.Add(footerHelp);
            Label footerProfile = new Label();
            footerProfile.Text = "Profile";
            footerProfile.Location = new Point(150, 78);
            footerProfile.Size = new Size(110, 20);
            footerProfile.Font = new Font("Segoe UI", 8);
            footerProfile.Tag = "sidebar-muted";
            sidebarFooterPanel.Controls.Add(footerProfile);

            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.Font = new Font("Segoe UI", 9);
            treeView.BorderStyle = BorderStyle.None;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.AfterExpand += delegate { RefreshModernScrollBars(); };
            treeView.AfterCollapse += delegate { RefreshModernScrollBars(); };

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

            TabPage aiTab = new TabPage("  AI öneriler  ");
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
            dashboardStripPanel.Controls.Add(CreateMetricCard("Toplam Boyut", "Hazir", out summaryTotalLabel), 0, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Dosya Sayisi", "0", out summaryFilesLabel), 1, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Sure", "-", out summaryTimeLabel), 2, 0);
            dashboardStripPanel.Controls.Add(CreateMetricCard("Konum", "-", out summaryPathLabel), 3, 0);

            listBodyPanel = new Panel();
            listBodyPanel.Dock = DockStyle.Fill;

            listView = new SmoothListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = false;
            listView.OwnerDraw = true;
            listView.Font = new Font("Segoe UI", 9);
            listView.BorderStyle = BorderStyle.None;
            listView.Columns.Add(listColumnTitles[0], 270);
            listView.Columns.Add(listColumnTitles[1], 90);
            listView.Columns.Add(listColumnTitles[2], 55);
            listView.Columns.Add(listColumnTitles[3], 115);
            listView.Columns.Add(listColumnTitles[4], 65);
            listView.Columns.Add(listColumnTitles[5], 280);
            listView.ColumnClick += ListView_ColumnClick;
            listView.DrawColumnHeader += ListView_DrawColumnHeader;
            listView.DrawItem += ListView_DrawItem;
            listView.DrawSubItem += ListView_DrawSubItem;
            listView.SizeChanged += delegate { AdjustListColumns(); UpdateListNativeMasks(); };
            listView.ColumnWidthChanged += delegate { UpdateListNativeMasks(); };
            listNativeVerticalMask = CreateNativeScrollbarMask();
            listNativeHorizontalMask = CreateNativeScrollbarMask();
            listView.Controls.Add(listNativeVerticalMask);
            listView.Controls.Add(listNativeHorizontalMask);
            AdjustListColumns();
            UpdateListNativeMasks();

            listScrollBar = new ModernScrollBar();
            listScrollBar.Attach(listView);
            listBodyPanel.Controls.Add(listView);
            listBodyPanel.Controls.Add(listScrollBar);
            listHost.Controls.Add(listBodyPanel);
            listHost.Controls.Add(dashboardStripPanel);
            listScrollBar.BringToFront();

            listContextMenu = new ContextMenuStrip();
            ToolStripMenuItem openItem   = new ToolStripMenuItem("  Aç (Explorer)");
            ToolStripMenuItem copyItem   = new ToolStripMenuItem("  yolu Kopyala");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("  Sil");
            openItem.Click   += ContextMenu_Open;
            copyItem.Click   += ContextMenu_CopyPath;
            deleteItem.Click += ContextMenu_Delete;
            listContextMenu.Items.Add(openItem);
            listContextMenu.Items.Add(copyItem);
            listContextMenu.Items.Add(new ToolStripSeparator());
            listContextMenu.Items.Add(deleteItem);
            listView.ContextMenuStrip = listContextMenu;

            // Ekleme sırası önemli: banner toolbar'ın altında görünmeli
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
            string root = "";
            try { root = Path.GetPathRoot(path); } catch { }
            string name = "";
            try { name = Path.GetFileName(path.TrimEnd('\\')); } catch { }
            if (!string.IsNullOrEmpty(root) && !string.IsNullOrEmpty(name))
            {
                string compact = root + "..." + Path.DirectorySeparatorChar + name;
                if (compact.Length <= maxLength) return compact;
            }
            return "..." + path.Substring(Math.Max(0, path.Length - maxLength + 3));
        }

        // =====================================================================
        // EULA / GIZLILIK GUVENCESI
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
                MessageBox.Show("Gizlilik onayı registry'e yazılamadı. Uygulama bu oturumda devam edecek.",
                    "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        // AG SURUCUSU ALGILAMA
        // =====================================================================

        /// <summary>
        /// Verilen path'in ağ sürücüsü (mapped drive veya UNC) olup olmadığını dondurur.
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
        /// Ag surucusu ise UNC hedefini bulmaya calisir, banner metnini hazirlar.
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
        /// Banner'i göster veya gizle, renk ve metin ayarla.
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
        // TEMA
        // =====================================================================

        private void ThemeButton_Click(object sender, EventArgs e)
        {
            Theme.IsDark = !Theme.IsDark;
            themeButton.Text = Theme.IsDark ? "AYDINLIK" : "KARANLIK";
            ApplyTheme();
            piePanel.Invalidate();
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

            themeButton.BackColor = Theme.Card;
            themeButton.ForeColor = Theme.SubText;
            themeButton.FlatAppearance.BorderColor = Theme.Border;

            filterBox.BackColor = Theme.Card;
            filterBox.ForeColor = Theme.Text;

            topFilesButton.BackColor = Theme.Card;
            topFilesButton.ForeColor = Theme.SubText;
            topFilesButton.FlatAppearance.BorderColor = Theme.Border;
            duplicatesButton.BackColor = Theme.Card;
            duplicatesButton.ForeColor = Theme.SubText;
            duplicatesButton.FlatAppearance.BorderColor = Theme.Border;
            csvButton.BackColor = Theme.Card;
            csvButton.ForeColor = Theme.SubText;
            csvButton.FlatAppearance.BorderColor = Theme.Border;
            bool signedIn = !string.IsNullOrEmpty(OnlineLicenseClient.GetSavedToken());
            accountButton.BackColor = signedIn ? Theme.Card : Theme.Surface;
            accountButton.ForeColor = signedIn ? Theme.Success : Theme.SubText;
            accountButton.FlatAppearance.BorderColor = signedIn ? Theme.Success : Theme.Border;
            licenseButton.BackColor = LicenseManager.HasPaidPlan(currentLicense) ? Theme.Success : Theme.Card;
            licenseButton.ForeColor = LicenseManager.HasPaidPlan(currentLicense)
                ? (Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White)
                : Theme.SubText;
            licenseButton.FlatAppearance.BorderColor = LicenseManager.HasPaidPlan(currentLicense) ? Theme.Success : Theme.Border;
            licenseLabel.BackColor = Theme.Bg;
            licenseLabel.ForeColor = LicenseManager.IsEnterprise(currentLicense) ? Theme.Success : Theme.SubText;

            statusLabel.ForeColor  = Theme.SubText;
            liveCountLabel.ForeColor = Theme.Success;
            ApplySidebarTheme();

            // Banner rengi güncelle
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
                    }
                    else if (tag == "nav-active")
                    {
                        button.BackColor = Theme.Highlight;
                        button.ForeColor = Theme.Text;
                        button.FlatAppearance.BorderColor = Theme.Accent;
                        button.FlatAppearance.BorderSize = 1;
                    }
                    else if (tag == "nav")
                    {
                        button.BackColor = Theme.Surface;
                        button.ForeColor = Theme.SubText;
                        button.FlatAppearance.BorderColor = Theme.Surface;
                        button.FlatAppearance.BorderSize = 0;
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
                licenseLabel.Text = currentLicense.PlanName.ToUpperInvariant();

            if (sidebarLicenseLabel != null)
                sidebarLicenseLabel.Text = "Plan: " + currentLicense.PlanName;

            if (licenseButton != null)
                licenseButton.Text = "PLAN";

            if (this.IsHandleCreated)
                ApplyTheme();
        }

        private void LicenseButton_Click(object sender, EventArgs e)
        {
            using (LicenseForm form = new LicenseForm(currentLicense, PurchaseUrl, EnterpriseContactUrl))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    currentLicense = LicenseManager.Load();
                    RefreshLicenseUi();
                    statusLabel.Text = "Lisans durumu: " + currentLicense.PlanName;
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

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabControl.TabPages[e.Index];
            bool sel = (e.Index == tabControl.SelectedIndex);
            Color bg   = sel ? Theme.Accent  : Theme.Card;
            Color fg   = sel ? (Theme.IsDark ? Color.FromArgb(10,10,20) : Color.White) : Theme.SubText;
            using (SolidBrush b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);
            e.Graphics.DrawString(page.Text,
                new Font("Segoe UI", 9, sel ? FontStyle.Bold : FontStyle.Regular),
                new SolidBrush(fg), e.Bounds.X + 5, e.Bounds.Y + 5);
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
            // SubItem çizimi tüm satırı kontrol ediyor.
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color alt = Theme.IsDark ? Color.FromArgb(17, 22, 30) : Color.FromArgb(250, 251, 253);
            Color bg = e.Item.Selected ? Theme.Highlight : (e.ItemIndex % 2 == 0 ? Theme.Surface : alt);
            using (SolidBrush brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (Pen pen = new Pen(Color.FromArgb(Theme.IsDark ? 34 : 220, Theme.Border)))
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top + 3, e.Bounds.Right - 1, e.Bounds.Bottom - 3);

            Color fg = e.Item.Selected ? Theme.Text : e.Item.ForeColor;
            if (fg == Color.Empty) fg = Theme.Text;
            Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);

            if (e.ColumnIndex == 2)
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
            if (listView == null || listView.Columns.Count < 6) return;
            int chrome = Math.Max(14, SystemInformation.VerticalScrollBarWidth);
            int available = Math.Max(620, listView.ClientSize.Width - chrome - 6);

            int sizeW = 104;
            int scoreW = 56;
            int dateW = 126;
            int typeW = 74;
            int nameW = Math.Max(220, Math.Min(360, (int)(available * 0.34)));
            int locationW = available - (nameW + sizeW + scoreW + dateW + typeW);

            if (locationW < 220)
            {
                int need = 220 - locationW;
                nameW = Math.Max(180, nameW - need);
                locationW = available - (nameW + sizeW + scoreW + dateW + typeW);
            }
            locationW = Math.Max(180, locationW);

            SetColumnWidth(0, nameW);
            SetColumnWidth(1, sizeW);
            SetColumnWidth(2, scoreW);
            SetColumnWidth(3, dateW);
            SetColumnWidth(4, typeW);
            SetColumnWidth(5, locationW);
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
            listNativeVerticalMask.Bounds = new Rectangle(Math.Max(0, listView.ClientSize.Width - w), 0, w, listView.ClientSize.Height);
            listNativeHorizontalMask.Bounds = new Rectangle(0, Math.Max(0, listView.ClientSize.Height - h), listView.ClientSize.Width, h);
            listNativeVerticalMask.BringToFront();
            listNativeHorizontalMask.BringToFront();
            SmoothListView smooth = listView as SmoothListView;
            if (smooth != null) smooth.HideChrome();
        }

        private int GetCurrentLiveListLimit()
        {
            return ultraFastMode ? UltraLiveListItemLimit : LiveListItemLimit;
        }

        // =====================================================================
        // UI TIMER
        // =====================================================================

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            int liveLimit = GetCurrentLiveListLimit();
            liveCountLabel.Text = liveListLimited
                ? string.Format("{0:N0} dosya  |  {1} canli liste {2:N0}+ ile sinirli", totalFilesFound, ultraFastMode ? "ultra" : "standart", liveLimit)
                : string.Format("{0:N0} dosya", totalFilesFound);
            if (summaryFilesLabel != null)
                summaryFilesLabel.Text = string.Format("{0:N0}", totalFilesFound);
            if (summaryTotalLabel != null)
                summaryTotalLabel.Text = FormatSize(Interlocked.Read(ref totalBytesScanned));

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
                foreach (FileNode f in batch)
                {
                    if (listView.Items.Count >= liveLimit)
                    {
                        liveListLimited = true;
                        break;
                    }
                    listView.Items.Add(CreateFileListItem(f));
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
        // TARAMA
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
                scanButton.Text = "IPTAL...";
                statusLabel.Text = "Tarama iptal ediliyor...";
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string selectedPath = dialog.SelectedPath;
            selectedScanPath = selectedPath;

            // --- AG SURUCUSU KONTROLU ---
            isNetworkDrive = IsNetworkPath(selectedPath);
            if (isNetworkDrive)
            {
                if (!EnsureFeature(LicenseFeature.NetworkScan, "Ağ sürücüsü taraması"))
                    return;

                networkDriveInfo = GetNetworkDriveDetail(selectedPath);
                string bannerMsg = "AG SURUCUSU  |  " + networkDriveInfo +
                                   "  |  Yavas mod aktif — tarama daha uzun surebilir";
                ShowNetworkBanner(true, bannerMsg);

                // Kullaniciya bilgi ver, onay al
                DialogResult confirm = MessageBox.Show(
                    "Ag sürücüsü seçildi:\n" + networkDriveInfo +
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

                // Ag modunda SSD paralelligi kullanma
                isSSD = false;
            }
            else
            {
                ShowNetworkBanner(false);
                networkDriveInfo = "";
            }

            CancellationTokenSource localCancel = new CancellationTokenSource();
            scanCancelSource = localCancel;
            CancellationToken token = localCancel.Token;
            Stopwatch sw = new Stopwatch();

            try
            {
                scanInProgress = true;
                scanButton.Enabled = true;
                scanButton.Text = "IPTAL";

                // --- NORMAL TARAMA AKISI ---
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
                UpdateSummaryCards("Taraniyor", "0", "-", selectedPath);

                progressBar.Style = ProgressBarStyle.Marquee;
                uiTimer.Start();

                // Ag surucusu degilse surucu tipini tespit et
                if (!isNetworkDrive)
                    DetectDriveType(selectedPath);
                else
                    statusLabel.Text = "Ag sürücüsü - yavaş mod (2 iş parçacığı)";
                if (ultraFastMode && !isNetworkDrive)
                    statusLabel.Text += "  |  Ultra hızlı mod";

                sw.Start();
                DirectoryNode scannedRoot = await Task.Run(() => FastScan(selectedPath, token), token);
                token.ThrowIfCancellationRequested();
                sw.Stop();
                rootNode = scannedRoot;

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
                    ? "Tamamlandi  [Ag Surucusu: " + networkDriveInfo + "]"
                    : "Tamamlandi";
                statTotal.Text       = "Toplam: " + FormatSize(rootNode.Size);
                statFiles.Text       = string.Format("{0:N0} dosya", totalFilesFound);
                statTime.Text        = "Sure: " + sw.Elapsed.TotalSeconds.ToString("F1") + "sn";
                UpdateSummaryCards(FormatSize(rootNode.Size), string.Format("{0:N0}", totalFilesFound), sw.Elapsed.TotalSeconds.ToString("F1") + " sn", selectedPath);
                WriteScanHistory(selectedPath, rootNode.Size, totalFilesFound, sw.Elapsed, isNetworkDrive, networkDriveInfo);
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                statusLabel.Text = "Tarama iptal edildi. Listede tarama anına kadar bulunan dosyalar kaldı.";
                statFiles.Text = string.Format("{0:N0} dosya bulundu", totalFilesFound);
                statTime.Text = "Sure: " + sw.Elapsed.TotalSeconds.ToString("F1") + "sn";
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

        private DirectoryNode FastScan(string path, CancellationToken token)
        {
            return FastScan(path, 0, token);
        }

        private DirectoryNode FastScan(string path, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            DirectoryNode node = new DirectoryNode();
            node.Name = string.IsNullOrEmpty(Path.GetFileName(path)) ? path : Path.GetFileName(path);
            node.Path = path;
            try
            {
                foreach (FileInfo info in SafeEnumerateFileInfos(path))
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        FileNode f = new FileNode();
                        f.Name = info.Name; f.FullPath = info.FullName;
                        f.Size = info.Length; f.LastModified = info.LastWriteTime;
                        f.Extension = info.Extension.ToLowerInvariant();
                        f.Score = scoringModel.Score(info.Length, info.LastWriteTime, f.Extension, info.Name, scoreOldFileThreshold);
                        node.Files.Add(f); node.Size += info.Length;
                        Interlocked.Increment(ref totalFilesFound);
                        Interlocked.Add(ref totalBytesScanned, info.Length);
                        QueueLiveFile(f);
                    }
                    catch { }
                }

                DirectoryInfo[] subDirs = SafeEnumerateDirectoryInfos(path);

                // Ag sürücüsü: max 2 thread (ağ tıkanmaması için)
                // SSD: tam paralel (CPU sayısı kadar)
                // HDD: 2 thread (kafa çarpışması önleme)
                int deg = GetScanDegree(depth);
                bool useParallel = ShouldParallelize(subDirs.Length, depth);

                DirectoryNode[] children = new DirectoryNode[subDirs.Length];
                if (useParallel)
                {
                    Parallel.For(0, subDirs.Length, new ParallelOptions { MaxDegreeOfParallelism = deg, CancellationToken = token }, i =>
                    {
                        token.ThrowIfCancellationRequested();
                        children[i] = FastScan(subDirs[i].FullName, depth + 1, token);
                    });
                }
                else
                {
                    for (int i = 0; i < subDirs.Length; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        children[i] = FastScan(subDirs[i].FullName, depth + 1, token);
                    }
                }

                foreach (DirectoryNode child in children)
                {
                    if (child == null) continue;
                    node.SubDirectories.Add(child);
                    node.Size += child.Size;
                }
            }
            catch (OperationCanceledException) { throw; }
            catch { }
            return node;
        }

        private FileInfo[] SafeEnumerateFileInfos(string path)
        {
            try { return new DirectoryInfo(path).GetFiles(); }
            catch { return new FileInfo[0]; }
        }

        private DirectoryInfo[] SafeEnumerateDirectoryInfos(string path)
        {
            try { return new DirectoryInfo(path).GetDirectories(); }
            catch { return new DirectoryInfo[0]; }
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

        private void QueueLiveFile(FileNode file)
        {
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

        private TreeNode BuildTreeNode(DirectoryNode node)
        {
            TreeNode tn = new TreeNode(node.Name + "  (" + FormatSize(node.Size) + ")");
            tn.Tag = node;
            foreach (DirectoryNode sub in node.SubDirectories.OrderByDescending(s => s.Size))
                tn.Nodes.Add(BuildTreeNode(sub));
            return tn;
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

                if (sortColumn == 1)
                    result = ParseSize(va).CompareTo(ParseSize(vb));
                else if (sortColumn == 2)
                {
                    int ia = 0, ib = 0;
                    int.TryParse(va, out ia);
                    int.TryParse(vb, out ib);
                    result = ia.CompareTo(ib);
                }
                else if (sortColumn == 3)
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
            foreach (ListViewItem item in items)
                listView.Items.Add(item);
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
            statusLabel.Text = string.IsNullOrEmpty(filter) ? "Liste yenileniyor..." : "Filtre uygulanıyor...";

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
                : string.Format("{0:N0} eşleşme", files.Count);
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
            statusLabel.Text = "Top 100 en buyuk dosya";
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
            statusLabel.Text = string.Format("{0:N0} olasi kopya dosya listelendi", files.Count);
        }

        private void CsvButton_Click(object sender, EventArgs e)
        {
            if (rootNode == null)
            {
                MessageBox.Show("önce bir tarama yapın.", "CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "CSV Export";
            dialog.Filter = "CSV dosyasi (*.csv)|*.csv";
            dialog.FileName = "AdvancedDiskAnalyzer_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                List<FileNode> all = new List<FileNode>();
                CollectAllFiles(rootNode, all);
                using (StreamWriter writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Name,SizeBytes,Size,Score,LastModified,Extension,FullPath");
                    foreach (FileNode f in all.OrderByDescending(f => f.Size))
                    {
                        writer.WriteLine(string.Join(",", new string[]
                        {
                            Csv(f.Name),
                            f.Size.ToString(CultureInfo.InvariantCulture),
                            Csv(FormatSize(f.Size)),
                            f.Score.ToString(CultureInfo.InvariantCulture),
                            Csv(f.LastModified.ToString("yyyy-MM-dd")),
                            Csv(f.Extension),
                            Csv(f.FullPath)
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

        private void WriteScanHistory(string scanPath, long totalBytes, int fileCount, TimeSpan duration, bool networkScan, string networkInfo)
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
                        fileCount.ToString(CultureInfo.InvariantCulture),
                        duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),
                        Csv(networkScan ? "Network" : "Local"),
                        Csv(networkInfo),
                        Csv(ultraFastMode ? "UltraFast" : "Standard"),
                        Csv(currentLicense != null ? currentLicense.PlanName : "Free"),
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
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("Timestamp,User,Machine,ScanPath,TotalBytes,TotalSize,FileCount,DurationSeconds,ScanType,NetworkInfo,Mode,Plan,Version");
                }
            }
        }

        private async void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DirectoryNode node = e.Node.Tag as DirectoryNode;
            if (node == null) return;

            int version = Interlocked.Increment(ref treeSelectionVersion);
            currentSelectedDirectory = node;
            currentVisualNode = node;
            ClearFilterBoxSilently();
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
            statusLabel.Text = node.Name + "  |  " + string.Format("{0:N0}", files.Count) +
                (files.Count > DisplayFileLimit ? " dosya, ilk " + string.Format("{0:N0}", DisplayFileLimit) + " gösteriliyor" : " dosya listelendi");
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
                for (int i = index; i < end; i++)
                    listView.Items.Add(CreateFileListItem(files[i]));
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
            item.SubItems.Add(file.Score.ToString());
            item.SubItems.Add(file.LastModified.ToString("yyyy-MM-dd"));
            item.SubItems.Add(file.Extension);
            item.SubItems.Add(Path.GetDirectoryName(file.FullPath));
            item.Tag = file.FullPath;

            item.BackColor = Theme.Surface;
            item.ForeColor = Theme.Text;
            return item;
        }

        // =====================================================================
        // SAG TIK MENU
        // =====================================================================

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
            string name = Path.GetFileName(path);
            if (MessageBox.Show("Silinsin mi?\n\n" + name, "Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    long freed = File.Exists(path) ? new FileInfo(path).Length : 0;
                    File.Delete(path);
                    HashSet<string> deleted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    deleted.Add(path);
                    ApplyDeletedFilesToModel(deleted, freed);
                    statusLabel.Text = name + " silindi. Kazanc: " + FormatSize(freed);
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }

        // =====================================================================
        // PASTA GRAFIK
        // =====================================================================

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

            g.DrawString("Boyut Dağılımı",
                new Font("Segoe UI", 11, FontStyle.Bold),
                new SolidBrush(Theme.Text), new PointF(16, 14));

            if (currentPieSlices.Count == 0)
            {
                g.DrawString("Henuz tarama yapilmadi.", new Font("Segoe UI", 9),
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

            if (hoveredSlice >= 0 && hoveredSlice < currentPieSlices.Count)
            {
                var sl = currentPieSlices[hoveredSlice];
                long tot = currentPieSlices.Sum(s => s.Value);
                double pct = sl.Value / (double)tot * 100.0;
                string tip = sl.Key + "\n" + FormatSize(sl.Value) + "  (" + pct.ToString("F1") + "%)";
                Font tipFont = new Font("Segoe UI", 9, FontStyle.Bold);
                SizeF ts = g.MeasureString(tip, tipFont);
                int tx = cx - (int)ts.Width / 2, ty = cy - (int)ts.Height / 2;
                Rectangle tr = new Rectangle(tx - 10, ty - 6, (int)ts.Width + 20, (int)ts.Height + 12);
                using (SolidBrush bg2 = new SolidBrush(Color.FromArgb(215, Theme.Card)))
                    g.FillRectangle(bg2, tr);
                g.DrawRectangle(new Pen(pieColors[hoveredSlice % pieColors.Length], 2), tr);
                g.DrawString(tip, tipFont, new SolidBrush(Theme.Text), tx, ty);
            }

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

            if (dist <= pieSize / 2.0f + 10)
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

        // =====================================================================
        // TREEMAP
        // =====================================================================

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

        // =====================================================================
        // AI ONERILER
        // =====================================================================

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

            Button csvReportBtn = CreateAiButton("CSV Export", 154, y, 132, Theme.Accent2);
            csvReportBtn.Click += CsvButton_Click;
            aiPanel.Controls.Add(csvReportBtn);

            Button dupReportBtn = CreateAiButton("Kopyalari Goster", 296, y, 146, Theme.Warning);
            dupReportBtn.Click += DuplicatesButton_Click;
            aiPanel.Controls.Add(dupReportBtn);
            y += 48;

            // Ag surucusu ise ozel not goster
            if (isNetworkDrive)
            {
                AddAiSection(ref y, "AĞ SÜRÜCÜSÜ TARAMASI  |  " + networkDriveInfo, Theme.Warning);
                AddAiNote(ref y, "Silme işlemi ağ üzerinden yapılacaktır. Dikkatli olun.");
                y += 6;
            }

            var deletable = allFiles
                .Where(f => f.Extension == ".tmp" || f.Extension == ".log" ||
                            f.Extension == ".bak" || f.Extension == ".old" || f.Name.StartsWith("~"))
                .OrderByDescending(f => f.Size).ToList();
            long deletableSize = deletable.Sum(f => f.Size);
            deletableFilePaths = deletable.Select(f => f.FullPath).ToList();

            if (deletable.Count > 0)
            {
                AddAiSection(ref y, "SİLİNEBİLECEK  /  " + deletable.Count + " dosya  /  " + FormatSize(deletableSize) + " kazanc", Theme.Danger);

                Button deleteAllBtn = new Button();
                deleteAllBtn.Text = LicenseManager.HasFeature(currentLicense, LicenseFeature.BulkDelete)
                    ? "Tümünü Sil  -  " + FormatSize(deletableSize) + " Alan Aç"
                    : "Tümünü Sil  (Enterprise)";
                deleteAllBtn.Location = new Point(12, y);
                deleteAllBtn.Size = new Size(420, 34);
                deleteAllBtn.BackColor = Theme.Danger;
                deleteAllBtn.ForeColor = Color.White;
                deleteAllBtn.FlatStyle = FlatStyle.Flat;
                deleteAllBtn.FlatAppearance.BorderSize = 0;
                deleteAllBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                deleteAllBtn.Cursor = Cursors.Hand;
                deleteAllBtn.Click += DeleteAllBtn_Click;
                aiPanel.Controls.Add(deleteAllBtn);
                y += 42;

                foreach (var f in deletable.Take(12))
                    AddAiRow(ref y, f.Name, FormatSize(f.Size), Theme.Danger);
                if (deletable.Count > 12)
                    AddAiNote(ref y, "... ve " + (deletable.Count - 12) + " dosya daha");
                y += 10;
            }

            var bigOld = allFiles
                .Where(f => f.Size > 50 * 1024 * 1024 && f.LastModified < DateTime.Now.AddYears(-1))
                .OrderByDescending(f => f.Size).Take(10).ToList();

            if (bigOld.Count > 0)
            {
                AddAiSection(ref y, "BÜYÜK & ESKİ  /  " + bigOld.Count + " adet  /  50MB+ ve 1 yıl+", Theme.Warning);
                foreach (var f in bigOld)
                    AddAiRow(ref y, f.Name, FormatSize(f.Size) + "  " + f.LastModified.ToString("yyyy-MM-dd"), Theme.Warning);
                y += 10;
            }

            List<DuplicateGroup> duplicateGroups = BuildDuplicateGroups(allFiles);
            long duplicateWaste = duplicateGroups.Sum(g => g.WastedSize);
            if (duplicateGroups.Count > 0)
            {
                AddAiSection(ref y, "OLASI KOPYALAR  /  " + duplicateGroups.Count + " grup  /  " + FormatSize(duplicateWaste) + " tekrar alan", Theme.Warning);
                foreach (DuplicateGroup group in duplicateGroups.Take(8))
                {
                    AddAiRow(ref y, group.Name,
                        group.Files.Count + " adet  /  " + FormatSize(group.WastedSize),
                        Theme.Warning);
                }
                if (duplicateGroups.Count > 8)
                    AddAiNote(ref y, "... ve " + (duplicateGroups.Count - 8) + " kopya grubu daha");
                y += 10;
            }

            var topLargest = allFiles.OrderByDescending(f => f.Size).Take(6).ToList();
            if (topLargest.Count > 0)
            {
                AddAiSection(ref y, "TOP ALAN HIRSIZLARI", Theme.Accent2);
                foreach (FileNode f in topLargest)
                    AddAiRow(ref y, f.Name, FormatSize(f.Size), Theme.Accent2);
                y += 10;
            }

            var bigDirs = root.SubDirectories.OrderByDescending(d => d.Size).Take(6).ToList();
            if (bigDirs.Count > 0)
            {
                AddAiSection(ref y, "EN BUYUK KLASORLER", Theme.Accent);
                foreach (var d in bigDirs)
                {
                    double pct = d.Size / (double)root.Size * 100.0;
                    AddAiRow(ref y, d.Name, FormatSize(d.Size) + "  %" + pct.ToString("F1"), Theme.Accent);
                }
                y += 10;
            }

            AddAiSection(ref y, "ÖZET", Theme.Success);
            AddAiRow(ref y, "Toplam boyut", FormatSize(root.Size), Theme.Success);
            AddAiRow(ref y, "Toplam dosya", string.Format("{0:N0}", allFiles.Count), Theme.Success);
            AddAiRow(ref y, "Temizlenebilir", FormatSize(deletableSize), Theme.Success);
            AddAiRow(ref y, "Tekrar alan", FormatSize(duplicateWaste), Theme.Success);
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
            aiPanel.Controls.Add(nl);
            Label vl = new Label();
            vl.AutoSize = false; vl.Width = 165; vl.Height = 20; vl.Location = new Point(295, y);
            vl.Text = value; vl.Font = new Font("Segoe UI", 9, FontStyle.Bold); vl.ForeColor = accent;
            aiPanel.Controls.Add(vl);
            y += 22;
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
                MessageBox.Show("önce bir klasör taraması yapın.", "PDF Rapor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "PDF Rapor Kaydet";
            dialog.Filter = "PDF dosyası (*.pdf)|*.pdf";
            dialog.FileName = "AdvancedDiskAnalyzer_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf";
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
                        g.DrawString("Grafik için veri yok.", font, brush, 34, 80);
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

        private void DeleteAllBtn_Click(object sender, EventArgs e)
        {
            if (!EnsureFeature(LicenseFeature.BulkDelete, "Toplu silme"))
                return;

            int count = deletableFilePaths.Count; if (count == 0) return;

            string warning = isNetworkDrive
                ? count + " dosya AĞ SÜRÜCÜSÜNDEN silinecek. Geri alınamaz!\n\nSunucu: " + networkDriveInfo
                : count + " dosya silinecek. Geri alınamaz!";

            if (MessageBox.Show(warning, "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            int deleted = 0; long freed = 0;
            HashSet<string> deletedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in deletableFilePaths.ToList())
            {
                try
                {
                    long sz = File.Exists(path) ? new FileInfo(path).Length : 0;
                    File.Delete(path);
                    deleted++;
                    freed += sz;
                    deletedPaths.Add(path);
                }
                catch { }
            }
            deletableFilePaths.Clear();
            if (deletedPaths.Count > 0)
                ApplyDeletedFilesToModel(deletedPaths, freed);
            string msg = deleted + " dosya silindi, " + FormatSize(freed) + " kazanıldı.";
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
                totalFilesFound = CountFiles(rootNode);
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

                statTotal.Text = "Toplam: " + FormatSize(rootNode.Size);
                statFiles.Text = string.Format("{0:N0} dosya", totalFilesFound);
                UpdateSummaryCards(FormatSize(rootNode.Size), string.Format("{0:N0}", totalFilesFound), null, selectedScanPath);
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

        private long RemoveDeletedFilesFromTree(DirectoryNode node, HashSet<string> deletedPaths)
        {
            if (node == null) return 0;
            long size = 0;
            for (int i = node.Files.Count - 1; i >= 0; i--)
            {
                FileNode file = node.Files[i];
                if (file != null && deletedPaths.Contains(file.FullPath))
                    node.Files.RemoveAt(i);
                else if (file != null)
                    size += file.Size;
            }

            foreach (DirectoryNode sub in node.SubDirectories)
                size += RemoveDeletedFilesFromTree(sub, deletedPaths);

            node.Size = size;
            return size;
        }

        private int CountFiles(DirectoryNode node)
        {
            if (node == null) return 0;
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
        // SURUCU TIPI ALGILAMA
        // =====================================================================

        private void DetectDriveType(string path)
        {
            try
            {
                string root = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(root)) root = path;
                bool cached;
                if (driveTypeCache.TryGetValue(root, out cached))
                {
                    isSSD = cached;
                    statusLabel.Text = isSSD ? "M.2/SSD - Tam paralel mod" : "HDD - Güvenli mod";
                    return;
                }

                isSSD = false;
                ManagementObjectSearcher s = new ManagementObjectSearcher("SELECT MediaType FROM Win32_DiskDrive");
                foreach (ManagementObject d in s.Get())
                {
                    string mt = d["MediaType"] as string;
                    if (mt != null && mt.Contains("SSD")) { isSSD = true; break; }
                }
                driveTypeCache[root] = isSSD;
                statusLabel.Text = isSSD ? "M.2/SSD - Tam paralel mod" : "HDD - Güvenli mod";
            }
            catch
            {
                isSSD = false;
                statusLabel.Text = "Sürücü tipi bilinmiyor";
            }
        }

        private string FormatSize(long size)
        {
            if (size > 1024L * 1024 * 1024) return (size / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
            if (size > 1024 * 1024)         return (size / (1024.0 * 1024)).ToString("F2") + " MB";
            if (size > 1024)                return (size / 1024.0).ToString("F2") + " KB";
            return size + " B";
        }
    }

    // =====================================================================
    // MODERN SCROLLBAR
    // =====================================================================

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
            if (m.Msg == WM_PAINT || m.Msg == WM_SIZE || m.Msg == WM_NCPAINT ||
                m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL || m.Msg == WM_MOUSEHWHEEL)
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

            for (int i = 0; i < this.TabPages.Count; i++)
            {
                Rectangle r = this.GetTabRect(i);
                bool selected = i == this.SelectedIndex;
                Color bg = selected ? Theme.Accent : Theme.Card;
                Color fg = selected ? (Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White) : Theme.SubText;

                using (SolidBrush brush = new SolidBrush(bg))
                    e.Graphics.FillRectangle(brush, r);
                using (Pen pen = new Pen(selected ? Theme.Accent : Theme.Border))
                    e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);

                TextRenderer.DrawText(e.Graphics, this.TabPages[i].Text.Trim(), this.Font, r, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            using (Pen pen = new Pen(Theme.Border))
                e.Graphics.DrawRectangle(pen, content.X, content.Y, content.Width - 1, content.Height - 1);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.Invalidate();
        }
    }

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
                int result = DwmSetWindowAttribute(handle, 20, ref value, sizeof(int));
                if (result != 0)
                    DwmSetWindowAttribute(handle, 19, ref value, sizeof(int));
            }
            catch { }
        }
    }

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
    // OFFLINE LISANS ALTYAPISI
    // =====================================================================

    public enum LicensePlan
    {
        Free,
        Pro,
        Enterprise
    }

    public enum LicenseFeature
    {
        PdfReport,
        NetworkScan,
        BulkDelete,
        CustomEula
    }

    public class LicenseState
    {
        public LicensePlan Plan = LicensePlan.Free;
        public string PlanName = "Free";
        public string Company = "Free Kullanıcı";
        public string Email = "";
        public DateTime? Expires = null;
        public int Seats = 1;
        public string HardwareId = "";
        public bool IsValid = false;
        public string RawKey = "";
        public string Message = "Free plan aktif.";
    }

    public static class LicenseManager
    {
        private const string LicenseRegistryPath = @"Software\AdvancedDiskAnalyzer\License";
        private const string PublicKeyXml =
            "<RSAKeyValue><Modulus>rJdXjF5+pUzOTHWnIdekvB85+OJ5TvhLygVyCXV1EllnmTYsHHLLmo9f9OfFTMZMyQG/Lo6IHUkYoXP1uq5P8Vc3Gd9GFcnj0HaLHaYJueNlDIjj3YFxJrA99ntyYdi+cM36++t057tTBujqEDncP4zgX1T029IgNewt7+F5bTWGzSJU/hKBHFxmTh/0LbiwQRxf/qj5qKUX4O0bZDYiFKgU1noPCIynDDzxnOoIDS0i9FysYBHkJeFwz0nMA81YmOtacCp9Rlg5M3+aCFqAk4j1di+WUgjAb2m3WMn07+Y58qqHRR7Cs5OHVAyed6QXvo7QFtTm2RNiCGQarM4W0Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

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

        public static void Remove()
        {
            try { Registry.CurrentUser.DeleteSubKeyTree(LicenseRegistryPath); }
            catch { }
        }

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
            return feature == LicenseFeature.PdfReport ? "Pro" : "Enterprise";
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
                message = "EULA özelleştirme Enterprise planda kullanılabilir.";
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
                    message = "Lisans süresi dolmus.";
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
                message = "Lisans bu cihaz icin üretilmemis.";
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
            if (plan == LicensePlan.Enterprise) return "Enterprise";
            return "Free";
        }

        private static LicenseState FreeState()
        {
            LicenseState state = new LicenseState();
            state.Plan = LicensePlan.Free;
            state.PlanName = "Free";
            state.Company = "Free Kullanıcı";
            state.HardwareId = GetHardwareId();
            state.IsValid = false;
            state.Message = "Free plan aktif.";
            return state;
        }
    }

    public static class OnlineLicenseClient
    {
        private const string RegistryPath = @"Software\AdvancedDiskAnalyzer\License";
        private const string DefaultServerUrl = "http://localhost:8765";

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
                        message = "Sunucu lisans anahtari döndürmedi.";
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
            title.Text = "Advanced Disk Analyzer Account";
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

            Button logout = NeutralButton("çıkıs", 290, 302, 88);
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
                MessageBox.Show("Hesap olusturuldu ve giris yapildi.", "Hesap", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            statusLabel.Text = string.IsNullOrEmpty(email) ? "Oturum acik degil" : "Oturum acik: " + email;
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
            heading.Text = title + " icin " + LicenseManager.RequiredPlanName(feature) + " gerekir";
            heading.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            heading.Location = new Point(24, 24);
            heading.Size = new Size(460, 30);
            heading.ForeColor = Theme.Text;
            this.Controls.Add(heading);

            Label body = new Label();
            body.Text = "Mevcut plan: " + (current == null ? "Free" : current.PlanName) +
                "\n\nPro: PDF rapor\nEnterprise: ağ sürücüsü, toplu silme, kurumsal EULA";
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

            AddPlanColumn(30, 104, "Free", "Temel analiz\nTreemap\nKopya görünümü\nCSV export");
            AddPlanColumn(286, 104, "Pro", "Free özellikleri\nPDF rapor\nProfesyonel çıktı\nOffline lisans");
            AddPlanColumn(542, 104, "Enterprise", "Pro özellikleri\nAğ sürücüsü\nToplu silme\nKurumsal EULA");

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
            companyBox.Text = current.Company == "Free Kullanıcı" ? "" : current.Company;
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

            Button activate = SmallButton("Key Etkinleştir", 276, 402, 120);
            activate.Click += Activate_Click;
            this.Controls.Add(activate);

            Button remove = SmallButton("Free", 406, 402, 64);
            remove.Click += delegate
            {
                LicenseManager.Remove();
                current = LicenseManager.Load();
                keyBox.Text = "";
                RefreshStatus();
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(remove);

            Button buyPro = SmallButton("Enterprise", 484, 402, 96);
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
            eulaLabel.Text = "Enterprise EULA metni";
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
            heading.ForeColor = title == "Enterprise" ? Theme.Success : Theme.Accent;
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
                MessageBox.Show(message, "Online Lisans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            keyBox.Text = licenseKey;
            if (LicenseManager.Install(licenseKey, out message))
            {
                current = LicenseManager.Load();
                RefreshStatus();
                eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
                MessageBox.Show(message, "Online Lisans", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(message, "Online Lisans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(message, "Online Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            keyBox.Text = licenseKey;
            if (LicenseManager.Install(licenseKey, out message))
            {
                current = LicenseManager.Load();
                RefreshStatus();
                eulaBox.Enabled = LicenseManager.HasFeature(current, LicenseFeature.CustomEula);
                MessageBox.Show(message, "Online Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(message, "Online Aktivasyon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SaveEula_Click(object sender, EventArgs e)
        {
            string message;
            if (LicenseManager.SaveEnterpriseEulaText(current, eulaBox.Text, out message))
                MessageBox.Show(message, "Enterprise EULA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(message, "Enterprise EULA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void RefreshStatus()
        {
            string expires = current.Expires.HasValue ? current.Expires.Value.ToString("yyyy-MM-dd") : "Perpetual";
            statusLabel.Text = "Plan: " + current.PlanName + "  |  Firma: " + current.Company + "  |  Bitiş: " + expires;
            statusLabel.ForeColor = LicenseManager.HasPaidPlan(current) ? Theme.Success : Theme.SubText;
            string savedEmail = OnlineLicenseClient.GetSavedEmail();
            if (accountLabel != null)
                accountLabel.Text = string.IsNullOrEmpty(savedEmail) ? "Hesap yok" : "Oturum açık: " + savedEmail;
        }

        private void OpenUrl(string url)
        {
            try { Process.Start(url); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Bağlantı", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }


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
    // ILK ACILIS GIZLILIK EKRANI
    // =====================================================================

    public class PrivacyConsentForm : Form
    {
        public PrivacyConsentForm(string version, string stampDate, string customEulaText)
        {
            this.Text = "Gizlilik Guvencesi";
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
                "- Bu yazilim tamamen çevrimdışı çalışır.",
                "- Hiçbir dosya adı, boyut veya içerik dış sunuculara gönderilmez.",
                "- Geliştirici hiçbir kullanıcı verisine erişemez."
                "- Açık kaynak kodlu bir yazılım olduğundan dolayı bunların hepsi kontrol edilebilir.
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
    // HARICI KUTUPHANESIZ PDF RAPOR
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
                .Where(f => f.Extension == ".tmp" || f.Extension == ".log" ||
                            f.Extension == ".bak" || f.Extension == ".old" || f.Name.StartsWith("~"))
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
            WriteLine("Dosya sayısı: " + string.Format("{0:N0}", allFiles.Count), 10, false);
            WriteLine("Temizlenebilir alan: " + FormatSize(deletableSize), 10, false);
            if (isNetworkDrive)
                WriteLine("Ağ sürücüsü / sunucu: " + networkInfo, 10, true);

            AddChart();
            AddFileTable("En Büyük Dosyalar", topLargest, "Dosya bulunamadı.");
            AddFileTable("Silinebilecek Dosyalar", deletable.Take(30).ToList(), "Silinebilecek dosya bulunamadı.");
            AddFileTable("Büyük ve Eski Dosyalar", bigOld.Take(30).ToList(), "Büyük ve eski dosya bulunamadı.");
            AddFileTable("Olası Kopyalar", duplicateFiles.Take(45).ToList(), "Olası kopya bulunamadı.");
            AddDirectoryTable("En Buyuk Klasorler", bigDirs, root.Size);
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
            DrawTextAt(430, y, Truncate(Path.GetDirectoryName(f.FullPath), 24), 8, false);
            y -= 15;
        }

        private void AddDirectoryTable(string title, List<DirectoryNode> dirs, long rootSize)
        {
            AddSection(title);
            if (dirs.Count == 0)
            {
                WriteLine("Klasor verisi bulunamadi.", 9, false);
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
    // MODELLER
    // =====================================================================

    public class AdaptiveScoringModel
    {
        public int Score(FileInfo file)
        {
            return Score(file.Length, file.LastWriteTime, file.Extension, file.Name, DateTime.Now.AddYears(-1));
        }

        public int Score(long size, DateTime lastWriteTime, string extension, string name, DateTime oldFileThreshold)
        {
            string ext = extension == null ? "" : extension.ToLowerInvariant();
            double sz  = Math.Min(1.0, size / (500.0 * 1024 * 1024));
            double age = lastWriteTime < oldFileThreshold ? 1.0 : 0.0;
            double tmp = (ext == ".tmp" || ext == ".log" ||
                          ext == ".bak" || ext == ".old" ||
                          (!string.IsNullOrEmpty(name) && name.StartsWith("~"))) ? 1.0 : 0.0;
            return (int)(((sz * 0.4) + (age * 0.4) + (tmp * 0.2)) * 100);
        }
    }

    public class DirectoryNode
    {
        public string Name; public string Path; public long Size;
        public List<FileNode> Files = new List<FileNode>();
        public List<DirectoryNode> SubDirectories = new List<DirectoryNode>();
    }

    public class FileNode
    {
        public string Name; public string FullPath; public long Size;
        public int Score; public DateTime LastModified; public string Extension;
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

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
