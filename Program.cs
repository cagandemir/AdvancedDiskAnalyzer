// Advanced Disk Analyzer v1.6 - Modern Edition

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

namespace AdvancedDiskAnalyzer
{
    public static class Theme
    {
        public static bool IsDark = true;

        public static Color DarkBg        = Color.FromArgb(13, 13, 23);
        public static Color DarkSurface   = Color.FromArgb(22, 22, 36);
        public static Color DarkCard      = Color.FromArgb(32, 32, 52);
        public static Color DarkBorder    = Color.FromArgb(55, 55, 85);
        public static Color DarkText      = Color.FromArgb(228, 228, 245);
        public static Color DarkSubText   = Color.FromArgb(130, 130, 165);
        public static Color DarkAccent    = Color.FromArgb(90, 205, 255);
        public static Color DarkAccent2   = Color.FromArgb(175, 95, 255);
        public static Color DarkSuccess   = Color.FromArgb(75, 215, 135);
        public static Color DarkDanger    = Color.FromArgb(255, 75, 95);
        public static Color DarkWarning   = Color.FromArgb(255, 185, 55);
        public static Color DarkHighlight = Color.FromArgb(38, 58, 88);

        public static Color LightBg        = Color.FromArgb(242, 244, 255);
        public static Color LightSurface   = Color.FromArgb(255, 255, 255);
        public static Color LightCard      = Color.FromArgb(232, 236, 255);
        public static Color LightBorder    = Color.FromArgb(195, 202, 230);
        public static Color LightText      = Color.FromArgb(18, 18, 38);
        public static Color LightSubText   = Color.FromArgb(88, 88, 128);
        public static Color LightAccent    = Color.FromArgb(45, 125, 215);
        public static Color LightAccent2   = Color.FromArgb(125, 55, 205);
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

    public class MainForm : Form
    {
        private TreeView treeView;
        private ListView listView;
        private Panel toolbarPanel;
        private Panel piePanel;
        private TabPage pieTab, aiTab;
        private Panel aiPanel;
        private ThemedTabControl tabControl;
        private ProgressBar progressBar;
        private Button scanButton;
        private Button themeButton;
        private Label statusLabel;
        private Label liveCountLabel;
        private ContextMenuStrip listContextMenu;
        private Panel statsBar;
        private Label statTotal, statFiles, statTime;

        private AdaptiveScoringModel scoringModel = new AdaptiveScoringModel();
        private bool isSSD = false;
        private DirectoryNode rootNode = null;
        private int totalFilesFound = 0;
        private System.Windows.Forms.Timer uiTimer;

        private readonly object queueLock = new object();
        private Queue<FileNode> pendingFiles = new Queue<FileNode>();

        private int hoveredSlice = -1;
        private List<KeyValuePair<string, long>> currentPieSlices = new List<KeyValuePair<string, long>>();
        private List<float[]> sliceAngles = new List<float[]>();

        // Siralama
        private int sortColumn = -1;
        private bool sortAscending = true;

        // Logo base64 olarak gomuldu - disarida logo.ico dosyasi gerekmez
        private const string LogoBase64 = "AAABAAEAEBAAAAAAIABwAwAAFgAAAIlQTkcNChoKAAAADUlIRFIAAAAQAAAAEAgGAAAAH/P/YQAAAzdJREFUeJxVk19o1WUYx7/P876/89vZHza3Nec8p3m2eea2oK3alOoELiosbTU9Xok3QX9AEeZNajC9SHBKF1kUBEUQhR68SEoqSJC6yBsTE2FuM1DnHM7YPH/2O7/f+75PF0dhPvC9eXg+D3wfni/hYWWzp1Uut9MSMSYmbO/k5K8vlAoLHYBDXXz19FPrX/1j70G6vnIWK2EA+Owr6dr61ic/9PWPFlPpjCQ7hyTZOSSp9Evy9MDO/PY3P//m6xOSXMlgfFwYAA4c+Xfzxsy7/7Uk+6SxtUOa1qy3zW3dpqK0XbWmQ9pSz8jmzNjckX2TGx+xBICOfyrrTp165++Z6fP1nhePiLUHcRAABMBZAxIS60IjTF5fasvCjhePDeybqJllZiU/nTtw7NbNi/WejkcQ8VwUwlkDsRbORIg1NMNf3Uo1res8pX0zPftn87mLH58gkOj9h6e6fzyzZ8REgWhdpdmvgvLjICYQK4gIqtvaQ7+pxdpyGTIlunD3hszev/r2ofevdevbty8Nh0E+Rqycc5b9unpUN7eBlQasgYiDv+qJkLyqKFiYhyk+qGXlSam8FJubv/KaDsKllDFlEFgAACKI8osgEbGKlwUO0Y2lWicOJizDLBdArK2xAYrR4gYNKAFB8PBkxAw4B3EODhaAQBwJERErDRCDxMGyw91EQDoWr53R2qcAQgSCOAevoRGsPfKtqYZASGuy1sDcm6ssFIGK+VC1NVM6mRr8/fJfDUF+8Y6vtCemVKDi3E0QVY4IgMRZiAhsGIBIiXOGdbzONCV6fiMQIbP1w2+n//lltwlLERF54hxAXHkCoOJOKj1iMkSsE8++cvbS2ZMjDBHOvP7RwUTXpnvOWU9EIvZ8sFIgroiVAns+iDky5UC3pJ/LP7/78Jg4Szw+Ljj6Qc3s4PCebZ39b8zrWLUXBQVYEzqxxoo11prQmnIRROQlBoaX+rfvHz052jCTzZ7mx8J06LtS+8u7jn/fs2lHsb0nI2u7hmRt16A82ZOR7qGR5cyuo2fGfpb0SuaRy8ciuveLO723rl8YLj24vwEE8usapxK9mfNfvtd+RZxDNptVuVzOAsD/pD6Rbe47gGEAAAAASUVORK5CYII=";

        public MainForm()
        {
            try
            {
                byte[] icoBytes = Convert.FromBase64String(LogoBase64);
                using (var ms = new System.IO.MemoryStream(icoBytes))
                    this.Icon = new Icon(ms);
            }
            catch { }
            InitializeUI();
            ApplyTheme();
            // Form handle hazir olduktan sonra scrollbar'i uygula
            this.HandleCreated += (s, e) => ApplyTheme();
        }

        private void InitializeUI()
        {
            this.Text = "Advanced Disk Analyzer v1.6";
            this.Size = new Size(1680, 980);
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            toolbarPanel = new Panel();
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 62;

            Label titleLabel = new Label();
            titleLabel.Text = "DISK ANALYZER";
            titleLabel.Font = new Font("Courier New", 15, FontStyle.Bold);
            titleLabel.Location = new Point(20, 16);
            titleLabel.AutoSize = true;
            toolbarPanel.Controls.Add(titleLabel);

            Label versionLabel = new Label();
            versionLabel.Text = "v1.6";
            versionLabel.Font = new Font("Courier New", 9);
            versionLabel.Location = new Point(200, 22);
            versionLabel.AutoSize = true;
            toolbarPanel.Controls.Add(versionLabel);

            scanButton = new Button();
            scanButton.Text = "TARA";
            scanButton.Location = new Point(290, 14);
            scanButton.Size = new Size(110, 34);
            scanButton.FlatStyle = FlatStyle.Flat;
            scanButton.FlatAppearance.BorderSize = 0;
            scanButton.Font = new Font("Courier New", 9, FontStyle.Bold);
            scanButton.Cursor = Cursors.Hand;
            scanButton.Click += ScanButton_Click;
            toolbarPanel.Controls.Add(scanButton);

            themeButton = new Button();
            themeButton.Text = "AYDINLIK";
            themeButton.Location = new Point(415, 14);
            themeButton.Size = new Size(110, 34);
            themeButton.FlatStyle = FlatStyle.Flat;
            themeButton.FlatAppearance.BorderSize = 1;
            themeButton.Font = new Font("Courier New", 8, FontStyle.Bold);
            themeButton.Cursor = Cursors.Hand;
            themeButton.Click += ThemeButton_Click;
            toolbarPanel.Controls.Add(themeButton);

            statusLabel = new Label();
            statusLabel.Location = new Point(545, 20);
            statusLabel.Width = 620;
            statusLabel.Font = new Font("Courier New", 8);
            statusLabel.Text = "Klasor secmek icin TARA butonuna basin.";
            toolbarPanel.Controls.Add(statusLabel);

            liveCountLabel = new Label();
            liveCountLabel.Location = new Point(1300, 20);
            liveCountLabel.Width = 320;
            liveCountLabel.Font = new Font("Courier New", 9, FontStyle.Bold);
            liveCountLabel.Text = "";
            toolbarPanel.Controls.Add(liveCountLabel);

            statsBar = new Panel();
            statsBar.Dock = DockStyle.Bottom;
            statsBar.Height = 28;

            statTotal = new Label();
            statTotal.Location = new Point(15, 7); statTotal.Width = 220;
            statTotal.Font = new Font("Courier New", 8);
            statTotal.BackColor = Color.Transparent;
            statsBar.Controls.Add(statTotal);

            statFiles = new Label();
            statFiles.Location = new Point(245, 7); statFiles.Width = 200;
            statFiles.Font = new Font("Courier New", 8);
            statFiles.BackColor = Color.Transparent;
            statsBar.Controls.Add(statFiles);

            statTime = new Label();
            statTime.Location = new Point(455, 7); statTime.Width = 200;
            statTime.Font = new Font("Courier New", 8);
            statTime.BackColor = Color.Transparent;
            statsBar.Controls.Add(statTime);

            progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Bottom;
            progressBar.Height = 4;

            treeView = new TreeView();
            treeView.Dock = DockStyle.Left;
            treeView.Width = 300;
            treeView.Font = new Font("Segoe UI", 9);
            treeView.BorderStyle = BorderStyle.None;
            treeView.AfterSelect += TreeView_AfterSelect;

            tabControl = new ThemedTabControl();
            tabControl.Dock = DockStyle.Right;
            tabControl.Width = 460;
            tabControl.Font = new Font("Segoe UI", 9);
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;


            pieTab = new TabPage("  Grafik  ");
            piePanel = new Panel();
            piePanel.Dock = DockStyle.Fill;
            piePanel.Paint += PiePanel_Paint;
            piePanel.MouseMove += PiePanel_MouseMove;
            piePanel.MouseLeave += PiePanel_MouseLeave;
            pieTab.Controls.Add(piePanel);

            aiTab = new TabPage("  AI Oneriler  ");
            aiPanel = new Panel();
            aiPanel.Dock = DockStyle.Fill;
            aiPanel.AutoScroll = true;
            aiTab.Controls.Add(aiPanel);

            tabControl.TabPages.Add(pieTab);
            tabControl.TabPages.Add(aiTab);

            listView = new ThemedListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = false;
            listView.Font = new Font("Segoe UI", 9);
            listView.BorderStyle = BorderStyle.None;
            listView.Columns.Add("Dosya Adi", 270);
            listView.Columns.Add("Boyut", 90);
            listView.Columns.Add("Skor", 55);
            listView.Columns.Add("Tarih", 115);
            listView.Columns.Add("Tur", 65);
            listView.Columns.Add("Konum", 280);

            listContextMenu = new ContextMenuStrip();
            ToolStripMenuItem openItem   = new ToolStripMenuItem("  Ac (Explorer)");
            ToolStripMenuItem copyItem   = new ToolStripMenuItem("  Yolu Kopyala");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("  Sil");
            openItem.Click   += ContextMenu_Open;
            copyItem.Click   += ContextMenu_CopyPath;
            deleteItem.Click += ContextMenu_Delete;
            listContextMenu.Items.Add(openItem);
            listContextMenu.Items.Add(copyItem);
            listContextMenu.Items.Add(new ToolStripSeparator());
            listContextMenu.Items.Add(deleteItem);
            listView.ColumnClick += ListView_ColumnClick;
            listView.ContextMenuStrip = listContextMenu;

            this.Controls.Add(listView);
            this.Controls.Add(treeView);
            this.Controls.Add(tabControl);
            this.Controls.Add(progressBar);
            this.Controls.Add(statsBar);
            this.Controls.Add(toolbarPanel);

            uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 120;
            uiTimer.Tick += UiTimer_Tick;
        }

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

            // --- Toolbar ---
            toolbarPanel.BackColor = Theme.Surface;
            foreach (Control c in toolbarPanel.Controls)
            {
                c.BackColor = Theme.Surface;
                c.ForeColor = Theme.SubText;
            }
            toolbarPanel.Controls[0].ForeColor = Theme.Accent;   // baslik
            toolbarPanel.Controls[1].ForeColor = Theme.Accent2;  // versiyon

            scanButton.BackColor = Theme.Accent;
            scanButton.ForeColor = Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White;
            scanButton.FlatAppearance.BorderColor = Theme.Accent;

            themeButton.BackColor = Theme.Card;
            themeButton.ForeColor = Theme.SubText;
            themeButton.FlatAppearance.BorderColor = Theme.Border;

            statusLabel.ForeColor    = Theme.SubText;
            liveCountLabel.ForeColor = Theme.Success;

            // --- Stats bar ---
            statsBar.BackColor  = Theme.Surface;
            statTotal.BackColor = Theme.Surface; statTotal.ForeColor = Theme.SubText;
            statFiles.BackColor = Theme.Surface; statFiles.ForeColor = Theme.SubText;
            statTime.BackColor  = Theme.Surface; statTime.ForeColor  = Theme.SubText;

            // --- TreeView ---
            treeView.BackColor = Theme.Surface;
            treeView.ForeColor = Theme.Text;
            treeView.LineColor = Theme.Border;

            // --- ListView: arka plan + mevcut satirlari yeniden renklendir ---
            listView.BackColor = Theme.Surface;
            listView.ForeColor = Theme.Text;
            foreach (ListViewItem item in listView.Items)
            {
                int score = 0;
                if (item.SubItems.Count > 2)
                    int.TryParse(item.SubItems[2].Text, out score);

                if (score >= 70)
                    item.BackColor = Theme.IsDark ? Color.FromArgb(58, 18, 26) : Color.FromArgb(255, 218, 222);
                else if (score >= 40)
                    item.BackColor = Theme.IsDark ? Color.FromArgb(52, 42, 12) : Color.FromArgb(255, 246, 212);
                else
                    item.BackColor = Theme.Surface;

                item.ForeColor = Theme.Text;
            }

            // --- Tab & paneller ---
            tabControl.BackColor = Theme.Surface;
            pieTab.BackColor     = Theme.Bg;
            piePanel.BackColor   = Theme.Bg;
            aiTab.BackColor      = Theme.Surface;
            aiPanel.BackColor    = Theme.Surface;

            // AI panel icindeki label'larin arkaplanini guncelle
            foreach (Control c in aiPanel.Controls)
            {
                if (c is Label) c.BackColor = Theme.Surface;
                if (c is Panel) c.BackColor = Theme.Surface; // ayirici cizgiler
            }

            // --- Context menu ---
            listContextMenu.BackColor = Theme.Card;
            listContextMenu.ForeColor = Theme.Text;
            foreach (ToolStripItem item in listContextMenu.Items)
            {
                item.BackColor = Theme.Card;
                item.ForeColor = item.Text.Contains("Sil") ? Theme.Danger : Theme.Text;
            }

            // Scrollbar dark mode (Windows 10+)
            DarkScrollbar.Apply(listView.Handle, Theme.IsDark);
            DarkScrollbar.Apply(aiPanel.Handle, Theme.IsDark);
            DarkScrollbar.Apply(treeView.Handle, Theme.IsDark);

            // Grafigi yeniden ciz
            piePanel.Invalidate();
            tabControl.Invalidate();
            statsBar.Invalidate();
            this.Refresh();
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

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            liveCountLabel.Text = string.Format("{0:N0} dosya", totalFilesFound);

            List<FileNode> batch = new List<FileNode>();
            lock (queueLock)
            {
                while (pendingFiles.Count > 0 && batch.Count < 400)
                    batch.Add(pendingFiles.Dequeue());
            }
            if (batch.Count == 0) return;

            listView.BeginUpdate();
            foreach (FileNode f in batch)
            {
                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(FormatSize(f.Size));
                item.SubItems.Add(f.Score.ToString());
                item.SubItems.Add(f.LastModified.ToString("yyyy-MM-dd"));
                item.SubItems.Add(f.Extension);
                item.SubItems.Add(Path.GetDirectoryName(f.FullPath));
                item.Tag = f.FullPath;

                if (f.Score >= 70)
                    item.BackColor = Theme.IsDark ? Color.FromArgb(58, 18, 26) : Color.FromArgb(255, 218, 222);
                else if (f.Score >= 40)
                    item.BackColor = Theme.IsDark ? Color.FromArgb(52, 42, 12) : Color.FromArgb(255, 246, 212);
                else
                    item.BackColor = Theme.Surface;
                item.ForeColor = Theme.Text;
                listView.Items.Add(item);
            }
            listView.EndUpdate();
        }

        private async void ScanButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            scanButton.Enabled = false;
            treeView.Nodes.Clear();
            listView.Items.Clear();
            aiPanel.Controls.Clear();
            totalFilesFound = 0;
            lock (queueLock) { pendingFiles.Clear(); }
            currentPieSlices.Clear(); sliceAngles.Clear(); hoveredSlice = -1;
            statTotal.Text = ""; statFiles.Text = ""; statTime.Text = "";

            progressBar.Style = ProgressBarStyle.Marquee;
            uiTimer.Start();
            DetectDriveType(dialog.SelectedPath);
            statusLabel.Text = "Taraniyor...";

            Stopwatch sw = Stopwatch.StartNew();
            rootNode = await Task.Run(() => FastScan(dialog.SelectedPath));
            sw.Stop();

            uiTimer.Stop();
            UiTimer_Tick(null, null);

            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.Nodes.Add(BuildTreeNode(rootNode));
            if (treeView.Nodes.Count > 0) treeView.Nodes[0].Expand();
            treeView.EndUpdate();

            BuildPieData(rootNode);
            piePanel.Tag = rootNode;
            piePanel.Invalidate();
            GenerateAIRecommendations(rootNode);

            progressBar.Style = ProgressBarStyle.Blocks;
            scanButton.Enabled = true;
            liveCountLabel.Text  = string.Format("{0:N0} dosya", totalFilesFound);
            statusLabel.Text     = "Tamamlandi";
            statTotal.Text       = "Toplam: " + FormatSize(rootNode.Size);
            statFiles.Text       = string.Format("{0:N0} dosya", totalFilesFound);
            statTime.Text        = "Sure: " + sw.Elapsed.TotalSeconds.ToString("F1") + "sn";
        }

        private DirectoryNode FastScan(string path)
        {
            DirectoryNode node = new DirectoryNode();
            node.Name = string.IsNullOrEmpty(Path.GetFileName(path)) ? path : Path.GetFileName(path);
            node.Path = path;
            try
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        FileNode f = new FileNode();
                        f.Name = info.Name; f.FullPath = file;
                        f.Size = info.Length; f.LastModified = info.LastWriteTime;
                        f.Extension = info.Extension.ToLower();
                        f.Score = scoringModel.Score(info);
                        node.Files.Add(f); node.Size += info.Length;
                        Interlocked.Increment(ref totalFilesFound);
                        lock (queueLock) pendingFiles.Enqueue(f);
                    }
                    catch { }
                }
                string[] subDirs = Directory.GetDirectories(path);
                int deg = isSSD ? Environment.ProcessorCount : 2;
                DirectoryNode[] children = new DirectoryNode[subDirs.Length];
                Parallel.For(0, subDirs.Length, new ParallelOptions { MaxDegreeOfParallelism = deg }, i =>
                {
                    try { children[i] = FastScan(subDirs[i]); } catch { }
                });
                foreach (DirectoryNode child in children)
                {
                    if (child == null) continue;
                    node.SubDirectories.Add(child);
                    node.Size += child.Size;
                }
            }
            catch { }
            return node;
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
            if (listView.Items.Count == 0) return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in listView.Items)
                items.Add(item);

            items.Sort((a, b) =>
            {
                string va = a.SubItems.Count > sortColumn ? a.SubItems[sortColumn].Text : "";
                string vb = b.SubItems.Count > sortColumn ? b.SubItems[sortColumn].Text : "";

                int result = 0;

                // Kolon 1=Boyut, 2=Skor sayisal karsilastirma
                if (sortColumn == 1) // Boyut
                {
                    result = CompareSizes(va, vb);
                }
                else if (sortColumn == 2) // Skor
                {
                    int ia = 0, ib = 0;
                    int.TryParse(va, out ia); int.TryParse(vb, out ib);
                    result = ia.CompareTo(ib);
                }
                else if (sortColumn == 3) // Tarih
                {
                    DateTime da = DateTime.MinValue, db = DateTime.MinValue;
                    DateTime.TryParse(va, out da); DateTime.TryParse(vb, out db);
                    result = da.CompareTo(db);
                }
                else // Metin (Dosya Adi, Tur, Konum)
                {
                    result = string.Compare(va, vb, StringComparison.OrdinalIgnoreCase);
                }

                return sortAscending ? result : -result;
            });

            listView.BeginUpdate();
            listView.Items.Clear();
            foreach (ListViewItem item in items)
                listView.Items.Add(item);
            listView.EndUpdate();

            // Sutun basligina ok goster
            UpdateSortArrow();
        }

        private int CompareSizes(string a, string b)
        {
            double va = ParseSize(a), vb = ParseSize(b);
            return va.CompareTo(vb);
        }

        private double ParseSize(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            s = s.Trim();
            try
            {
                if (s.EndsWith(" GB")) return double.Parse(s.Replace(" GB", "").Trim().Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture) * 1024 * 1024 * 1024;
                if (s.EndsWith(" MB")) return double.Parse(s.Replace(" MB", "").Trim().Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture) * 1024 * 1024;
                if (s.EndsWith(" KB")) return double.Parse(s.Replace(" KB", "").Trim().Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture) * 1024;
                if (s.EndsWith(" B"))  return double.Parse(s.Replace(" B", "").Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }
            return 0;
        }

        private void UpdateSortArrow()
        {
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                string baseText = listView.Columns[i].Text;
                // Eski oku temizle
                if (baseText.EndsWith(" ▲") || baseText.EndsWith(" ▼"))
                    baseText = baseText.Substring(0, baseText.Length - 2);
                listView.Columns[i].Text = i == sortColumn
                    ? baseText + (sortAscending ? " ▲" : " ▼")
                    : baseText;
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DirectoryNode node = e.Node.Tag as DirectoryNode;
            if (node == null) return;
            listView.BeginUpdate(); listView.Items.Clear();
            foreach (FileNode file in node.Files.OrderByDescending(f => f.Score))
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.SubItems.Add(FormatSize(file.Size));
                item.SubItems.Add(file.Score.ToString());
                item.SubItems.Add(file.LastModified.ToString("yyyy-MM-dd"));
                item.SubItems.Add(file.Extension);
                item.SubItems.Add(Path.GetDirectoryName(file.FullPath));
                item.Tag = file.FullPath;
                if (file.Score >= 70)      item.BackColor = Theme.IsDark ? Color.FromArgb(58,18,26) : Color.FromArgb(255,218,222);
                else if (file.Score >= 40) item.BackColor = Theme.IsDark ? Color.FromArgb(52,42,12) : Color.FromArgb(255,246,212);
                else                       item.BackColor = Theme.Surface;
                item.ForeColor = Theme.Text;
                listView.Items.Add(item);
            }
            listView.EndUpdate();
            BuildPieData(node);
            piePanel.Tag = node;
            piePanel.Invalidate();
        }

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
                try { File.Delete(path); listView.SelectedItems[0].Remove(); statusLabel.Text = name + " silindi."; }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }

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

            // Hover tooltip ortada
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

            // Legend
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

        private List<string> deletableFilePaths = new List<string>();

        private void GenerateAIRecommendations(DirectoryNode root)
        {
            aiPanel.Controls.Clear();
            deletableFilePaths.Clear();
            List<FileNode> allFiles = new List<FileNode>();
            CollectAllFiles(root, allFiles);
            int y = 14;

            var deletable = allFiles
                .Where(f => f.Extension == ".tmp" || f.Extension == ".log" ||
                            f.Extension == ".bak" || f.Extension == ".old" || f.Name.StartsWith("~"))
                .OrderByDescending(f => f.Size).ToList();
            long deletableSize = deletable.Sum(f => f.Size);
            deletableFilePaths = deletable.Select(f => f.FullPath).ToList();

            if (deletable.Count > 0)
            {
                AddAiSection(ref y, "SILINEBILECEK  /  " + deletable.Count + " dosya  /  " + FormatSize(deletableSize) + " kazanc", Theme.Danger);

                Button deleteAllBtn = new Button();
                deleteAllBtn.Text = "Tumunu Sil  -  " + FormatSize(deletableSize) + " Alan Ac";
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
                AddAiSection(ref y, "BUYUK & ESKI  /  " + bigOld.Count + " adet  /  50MB+ ve 1 yil+", Theme.Warning);
                foreach (var f in bigOld)
                    AddAiRow(ref y, f.Name, FormatSize(f.Size) + "  " + f.LastModified.ToString("yyyy-MM-dd"), Theme.Warning);
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

            AddAiSection(ref y, "OZET", Theme.Success);
            AddAiRow(ref y, "Toplam boyut", FormatSize(root.Size), Theme.Success);
            AddAiRow(ref y, "Toplam dosya", string.Format("{0:N0}", allFiles.Count), Theme.Success);
            AddAiRow(ref y, "Temizlenebilir", FormatSize(deletableSize), Theme.Success);

            tabControl.SelectedIndex = 1;
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

        private void DeleteAllBtn_Click(object sender, EventArgs e)
        {
            int count = deletableFilePaths.Count; if (count == 0) return;
            if (MessageBox.Show(count + " dosya silinecek. Geri alinamaz!",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            int deleted = 0; long freed = 0;
            foreach (string path in deletableFilePaths)
            {
                try { long sz = new FileInfo(path).Length; File.Delete(path); deleted++; freed += sz; }
                catch { }
            }
            deletableFilePaths.Clear();
            string msg = deleted + " dosya silindi, " + FormatSize(freed) + " kazanildi.";
            MessageBox.Show(msg, "Tamamlandi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            statusLabel.Text = msg;
            if (rootNode != null) GenerateAIRecommendations(rootNode);
        }

        private void CollectAllFiles(DirectoryNode node, List<FileNode> result)
        {
            result.AddRange(node.Files);
            foreach (var sub in node.SubDirectories) CollectAllFiles(sub, result);
        }

        private void DetectDriveType(string path)
        {
            try
            {
                ManagementObjectSearcher s = new ManagementObjectSearcher("SELECT MediaType FROM Win32_DiskDrive");
                foreach (ManagementObject d in s.Get())
                {
                    string mt = d["MediaType"] as string;
                    if (mt != null && mt.Contains("SSD")) { isSSD = true; break; }
                }
                statusLabel.Text = isSSD ? "M.2/SSD - Tam paralel mod" : "HDD - Guvenli mod";
            }
            catch { statusLabel.Text = "Surucu tipi bilinmiyor"; }
        }

        private string FormatSize(long size)
        {
            if (size > 1024L * 1024 * 1024) return (size / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
            if (size > 1024 * 1024)         return (size / (1024.0 * 1024)).ToString("F2") + " MB";
            if (size > 1024)                return (size / 1024.0).ToString("F2") + " KB";
            return size + " B";
        }
    }

    public class AdaptiveScoringModel
    {
        public int Score(FileInfo file)
        {
            double sz  = Math.Min(1.0, file.Length / (500.0 * 1024 * 1024));
            double age = file.LastWriteTime < DateTime.Now.AddYears(-1) ? 1.0 : 0.0;
            double tmp = (file.Extension == ".tmp" || file.Extension == ".log" ||
                          file.Extension == ".bak" || file.Extension == ".old") ? 1.0 : 0.0;
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



    // ===== KARANLIK SCROLLBAR (Windows 10 build 1903+) =====
    public static class DarkScrollbar
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void Apply(IntPtr handle, bool dark)
        {
            if (handle == IntPtr.Zero) return;
            int value = dark ? 1 : 0;
            try
            {
                // Windows 10 20H1 ve sonrasi
                int result = DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
                if (result != 0)
                    // Eski Windows 10 icin fallback
                    DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
            }
            catch { }
        }
    }

    // ===== TEMALI TAB CONTROL: arka plan sisteme bagli kalmiyor =====
    public class ThemedTabControl : TabControl
    {
        public ThemedTabControl()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Tum arka plani tema rengiyle doldur
            e.Graphics.Clear(Theme.Surface);

            // Tab butonlarini ciz
            for (int i = 0; i < this.TabCount; i++)
            {
                Rectangle r = this.GetTabRect(i);
                bool sel = (i == this.SelectedIndex);
                Color bg = sel ? Theme.Accent : Theme.Card;
                Color fg = sel ? (Theme.IsDark ? Color.FromArgb(10, 10, 20) : Color.White) : Theme.SubText;

                using (SolidBrush b = new SolidBrush(bg))
                    e.Graphics.FillRectangle(b, r);

                TextRenderer.DrawText(e.Graphics, this.TabPages[i].Text,
                    new Font("Segoe UI", 9, sel ? FontStyle.Bold : FontStyle.Regular),
                    r, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // Secili tab iceriginin arka planini ciz
            if (this.SelectedTab != null)
            {
                Rectangle content = this.SelectedTab.Bounds;
                // TabPage bounds biraz dis tarafa tasabilir, DisplayRectangle daha guvenli
                Rectangle display = this.DisplayRectangle;
                using (SolidBrush b = new SolidBrush(Theme.Surface))
                    e.Graphics.FillRectangle(b, display);
            }
        }
    }

    // ===== TEMALI LISTVIEW: header rengi tema ile degisir =====
    public class ThemedListView : ListView
    {
        private const int WM_NOTIFY        = 0x004E;
        private const int WM_ERASEBKGND    = 0x0014;
        private const int HDN_FIRST        = -300;
        private const int NM_CUSTOMDRAW    = -12;
        private const int CDDS_PREPAINT    = 0x00000001;
        private const int CDDS_ITEMPREPAINT = 0x00010001;
        private const int CDRF_NOTIFYITEMDRAW = 0x00000020;
        private const int CDRF_SKIPDEFAULT = 0x00000004;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct NMHDR { public IntPtr hwndFrom; public IntPtr idFrom; public int code; }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct NMCUSTOMDRAW
        {
            public NMHDR hdr; public int dwDrawStage; public IntPtr hdc;
            public RECT rc; public IntPtr dwItemSpec; public int uItemState; public IntPtr lItemlParam;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }

        public ThemedListView()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.OwnerDraw = true;
            this.DrawColumnHeader += OnDrawColumnHeader;
            this.DrawItem         += OnDrawItem;
            this.DrawSubItem      += OnDrawSubItem;
        }

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // Arka plan
            using (SolidBrush bg = new SolidBrush(Theme.Surface))
                e.Graphics.FillRectangle(bg, e.Bounds);

            // Alt ve sag cizgi (ayirici)
            using (Pen border = new Pen(Theme.Border, 1))
            {
                e.Graphics.DrawLine(border, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(border, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            // Baslik metni
            TextRenderer.DrawText(e.Graphics, e.Header.Text,
                new Font("Segoe UI", 9, FontStyle.Bold),
                new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height),
                Theme.Accent,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // Arka plan rengi
            Color bg = e.Item.BackColor;
            using (SolidBrush b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);

            // Secili satir vurgusu
            if ((e.ItemState & ListViewItemStates.Selected) != 0)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(60, Theme.Accent)))
                    e.Graphics.FillRectangle(b, e.Bounds);
            }

            // Metin
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                this.Font,
                new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                e.Item.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }
}
