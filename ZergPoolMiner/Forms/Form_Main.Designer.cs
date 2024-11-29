namespace ZergPoolMiner
{
    partial class Form_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonStartMining = new System.Windows.Forms.Button();
            this.labelServiceLocation = new System.Windows.Forms.Label();
            this.comboBoxRegion = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelGlobalRateText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelGlobalRateValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBTCDayText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBTCDayValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceBTCValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceBTCCode = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceDollarText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceDollarValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_power6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.linkLabelCheckStats = new System.Windows.Forms.LinkLabel();
            this.labelWorkerName = new System.Windows.Forms.Label();
            this.textBoxWorkerName = new System.Windows.Forms.TextBox();
            this.buttonStopMining = new System.Windows.Forms.Button();
            this.buttonBenchmark = new System.Windows.Forms.Button();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.buttonLogo = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.flowLayoutPanelRates = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_NotProfitable = new System.Windows.Forms.Label();
            this.buttonChart = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.labelWallet = new System.Windows.Forms.Label();
            this.textBoxWallet = new System.Windows.Forms.TextBox();
            this.label_Uptime = new System.Windows.Forms.Label();
            this.labelPayoutCurrency = new System.Windows.Forms.Label();
            this.textBoxPayoutCurrency = new System.Windows.Forms.TextBox();
            this.linkLabelNewVersion = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelTreshold = new System.Windows.Forms.Label();
            this.textBoxTreshold = new System.Windows.Forms.TextBox();
            this.devicesListViewEnableControl1 = new ZergPoolMiner.Forms.Components.DevicesListViewEnableControl();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStartMining
            // 
            this.buttonStartMining.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartMining.Location = new System.Drawing.Point(647, 147);
            this.buttonStartMining.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStartMining.Name = "buttonStartMining";
            this.buttonStartMining.Size = new System.Drawing.Size(105, 23);
            this.buttonStartMining.TabIndex = 6;
            this.buttonStartMining.Text = "&Start";
            this.buttonStartMining.UseVisualStyleBackColor = false;
            this.buttonStartMining.EnabledChanged += new System.EventHandler(this.buttonStartMining_EnabledChanged);
            this.buttonStartMining.Click += new System.EventHandler(this.ButtonStartMining_Click);
            this.buttonStartMining.Paint += new System.Windows.Forms.PaintEventHandler(this.buttonStartMining_Paint);
            // 
            // labelServiceLocation
            // 
            this.labelServiceLocation.AutoSize = true;
            this.labelServiceLocation.Location = new System.Drawing.Point(7, 68);
            this.labelServiceLocation.Name = "labelServiceLocation";
            this.labelServiceLocation.Size = new System.Drawing.Size(44, 13);
            this.labelServiceLocation.TabIndex = 99;
            this.labelServiceLocation.Text = "Region:";
            // 
            // comboBoxRegion
            // 
            this.comboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRegion.FormattingEnabled = true;
            this.comboBoxRegion.Location = new System.Drawing.Point(63, 64);
            this.comboBoxRegion.Name = "comboBoxRegion";
            this.comboBoxRegion.Size = new System.Drawing.Size(124, 21);
            this.comboBoxRegion.TabIndex = 0;
            this.comboBoxRegion.SelectedIndexChanged += new System.EventHandler(this.comboBoxLocation_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.AllowMerge = false;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelGlobalRateText,
            this.toolStripStatusLabelGlobalRateValue,
            this.toolStripStatusLabelBTCDayText,
            this.toolStripStatusLabelBTCDayValue,
            this.toolStripStatusLabelBalanceText,
            this.toolStripStatusLabelBalanceBTCValue,
            this.toolStripStatusLabelBalanceBTCCode,
            this.toolStripStatusLabelBalanceDollarText,
            this.toolStripStatusLabelBalanceDollarValue,
            this.toolStripStatusLabel_power1,
            this.toolStripStatusLabel_power2,
            this.toolStripStatusLabel_power3,
            this.toolStripStatusLabel_power4,
            this.toolStripStatusLabel_power5,
            this.toolStripStatusLabel_power6});
            this.statusStrip1.Location = new System.Drawing.Point(0, 293);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(764, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            this.statusStrip1.MouseHover += new System.EventHandler(this.statusStrip1_MouseHover);
            // 
            // toolStripStatusLabelGlobalRateText
            // 
            this.toolStripStatusLabelGlobalRateText.Margin = new System.Windows.Forms.Padding(6, 3, 0, 2);
            this.toolStripStatusLabelGlobalRateText.Name = "toolStripStatusLabelGlobalRateText";
            this.toolStripStatusLabelGlobalRateText.Size = new System.Drawing.Size(67, 17);
            this.toolStripStatusLabelGlobalRateText.Text = "Global rate:";
            // 
            // toolStripStatusLabelGlobalRateValue
            // 
            this.toolStripStatusLabelGlobalRateValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelGlobalRateValue.Name = "toolStripStatusLabelGlobalRateValue";
            this.toolStripStatusLabelGlobalRateValue.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabelGlobalRateValue.Text = "0.00000000";
            // 
            // toolStripStatusLabelBTCDayText
            // 
            this.toolStripStatusLabelBTCDayText.Name = "toolStripStatusLabelBTCDayText";
            this.toolStripStatusLabelBTCDayText.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabelBTCDayText.Text = "BTC/Day";
            // 
            // toolStripStatusLabelBTCDayValue
            // 
            this.toolStripStatusLabelBTCDayValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelBTCDayValue.Margin = new System.Windows.Forms.Padding(6, 3, 0, 2);
            this.toolStripStatusLabelBTCDayValue.Name = "toolStripStatusLabelBTCDayValue";
            this.toolStripStatusLabelBTCDayValue.Size = new System.Drawing.Size(31, 17);
            this.toolStripStatusLabelBTCDayValue.Text = "0.00";
            // 
            // toolStripStatusLabelBalanceText
            // 
            this.toolStripStatusLabelBalanceText.Name = "toolStripStatusLabelBalanceText";
            this.toolStripStatusLabelBalanceText.Size = new System.Drawing.Size(97, 17);
            this.toolStripStatusLabelBalanceText.Text = "$/Day     Balance:";
            // 
            // toolStripStatusLabelBalanceBTCValue
            // 
            this.toolStripStatusLabelBalanceBTCValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelBalanceBTCValue.Name = "toolStripStatusLabelBalanceBTCValue";
            this.toolStripStatusLabelBalanceBTCValue.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabelBalanceBTCValue.Text = "0.00000000";
            // 
            // toolStripStatusLabelBalanceBTCCode
            // 
            this.toolStripStatusLabelBalanceBTCCode.Name = "toolStripStatusLabelBalanceBTCCode";
            this.toolStripStatusLabelBalanceBTCCode.Size = new System.Drawing.Size(26, 17);
            this.toolStripStatusLabelBalanceBTCCode.Text = "BTC";
            // 
            // toolStripStatusLabelBalanceDollarText
            // 
            this.toolStripStatusLabelBalanceDollarText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelBalanceDollarText.Name = "toolStripStatusLabelBalanceDollarText";
            this.toolStripStatusLabelBalanceDollarText.Size = new System.Drawing.Size(31, 17);
            this.toolStripStatusLabelBalanceDollarText.Text = "0.00";
            // 
            // toolStripStatusLabelBalanceDollarValue
            // 
            this.toolStripStatusLabelBalanceDollarValue.Name = "toolStripStatusLabelBalanceDollarValue";
            this.toolStripStatusLabelBalanceDollarValue.Size = new System.Drawing.Size(16, 17);
            this.toolStripStatusLabelBalanceDollarValue.Text = "...";
            // 
            // toolStripStatusLabel_power1
            // 
            this.toolStripStatusLabel_power1.Name = "toolStripStatusLabel_power1";
            this.toolStripStatusLabel_power1.Size = new System.Drawing.Size(43, 17);
            this.toolStripStatusLabel_power1.Text = "Power:";
            // 
            // toolStripStatusLabel_power2
            // 
            this.toolStripStatusLabel_power2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel_power2.Name = "toolStripStatusLabel_power2";
            this.toolStripStatusLabel_power2.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusLabel_power2.Text = "0";
            // 
            // toolStripStatusLabel_power3
            // 
            this.toolStripStatusLabel_power3.Name = "toolStripStatusLabel_power3";
            this.toolStripStatusLabel_power3.Size = new System.Drawing.Size(28, 17);
            this.toolStripStatusLabel_power3.Text = "W.h";
            // 
            // toolStripStatusLabel_power4
            // 
            this.toolStripStatusLabel_power4.Name = "toolStripStatusLabel_power4";
            this.toolStripStatusLabel_power4.Size = new System.Drawing.Size(41, 17);
            this.toolStripStatusLabel_power4.Text = ", Total:";
            this.toolStripStatusLabel_power4.MouseHover += new System.EventHandler(this.toolStripStatusLabel_power4_MouseHover);
            // 
            // toolStripStatusLabel_power5
            // 
            this.toolStripStatusLabel_power5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel_power5.Name = "toolStripStatusLabel_power5";
            this.toolStripStatusLabel_power5.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusLabel_power5.Text = "0";
            // 
            // toolStripStatusLabel_power6
            // 
            this.toolStripStatusLabel_power6.Name = "toolStripStatusLabel_power6";
            this.toolStripStatusLabel_power6.Size = new System.Drawing.Size(18, 17);
            this.toolStripStatusLabel_power6.Text = "W";
            // 
            // linkLabelCheckStats
            // 
            this.linkLabelCheckStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelCheckStats.AutoSize = true;
            this.linkLabelCheckStats.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelCheckStats.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.linkLabelCheckStats.Location = new System.Drawing.Point(647, 45);
            this.linkLabelCheckStats.Name = "linkLabelCheckStats";
            this.linkLabelCheckStats.Size = new System.Drawing.Size(94, 13);
            this.linkLabelCheckStats.TabIndex = 9;
            this.linkLabelCheckStats.TabStop = true;
            this.linkLabelCheckStats.Text = "Check stats online";
            this.linkLabelCheckStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelCheckStats.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelCheckStats_LinkClicked);
            // 
            // labelWorkerName
            // 
            this.labelWorkerName.AutoSize = true;
            this.labelWorkerName.Location = new System.Drawing.Point(397, 15);
            this.labelWorkerName.Name = "labelWorkerName";
            this.labelWorkerName.Size = new System.Drawing.Size(76, 13);
            this.labelWorkerName.TabIndex = 99;
            this.labelWorkerName.Text = "Worker Name:";
            // 
            // textBoxWorkerName
            // 
            this.textBoxWorkerName.Location = new System.Drawing.Point(479, 12);
            this.textBoxWorkerName.Name = "textBoxWorkerName";
            this.textBoxWorkerName.ReadOnly = true;
            this.textBoxWorkerName.Size = new System.Drawing.Size(100, 20);
            this.textBoxWorkerName.TabIndex = 2;
            // 
            // buttonStopMining
            // 
            this.buttonStopMining.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStopMining.BackColor = System.Drawing.SystemColors.Control;
            this.buttonStopMining.Location = new System.Drawing.Point(647, 174);
            this.buttonStopMining.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStopMining.Name = "buttonStopMining";
            this.buttonStopMining.Size = new System.Drawing.Size(105, 23);
            this.buttonStopMining.TabIndex = 7;
            this.buttonStopMining.Text = "St&op";
            this.buttonStopMining.UseVisualStyleBackColor = false;
            this.buttonStopMining.EnabledChanged += new System.EventHandler(this.buttonStopMining_EnabledChanged);
            this.buttonStopMining.Click += new System.EventHandler(this.ButtonStopMining_Click);
            this.buttonStopMining.Paint += new System.Windows.Forms.PaintEventHandler(this.buttonStopMining_Paint);
            // 
            // buttonBenchmark
            // 
            this.buttonBenchmark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBenchmark.Location = new System.Drawing.Point(647, 93);
            this.buttonBenchmark.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.buttonBenchmark.Name = "buttonBenchmark";
            this.buttonBenchmark.Size = new System.Drawing.Size(105, 23);
            this.buttonBenchmark.TabIndex = 4;
            this.buttonBenchmark.Text = "&Benchmark";
            this.buttonBenchmark.UseVisualStyleBackColor = true;
            this.buttonBenchmark.Click += new System.EventHandler(this.ButtonBenchmark_Click);
            // 
            // buttonSettings
            // 
            this.buttonSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSettings.Location = new System.Drawing.Point(647, 120);
            this.buttonSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(105, 23);
            this.buttonSettings.TabIndex = 5;
            this.buttonSettings.Text = "S&ettings";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.ButtonSettings_Click);
            // 
            // buttonLogo
            // 
            this.buttonLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonLogo.FlatAppearance.BorderSize = 0;
            this.buttonLogo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLogo.Location = new System.Drawing.Point(647, 3);
            this.buttonLogo.Margin = new System.Windows.Forms.Padding(0);
            this.buttonLogo.Name = "buttonLogo";
            this.buttonLogo.Size = new System.Drawing.Size(105, 35);
            this.buttonLogo.TabIndex = 11;
            this.buttonLogo.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.buttonLogo.UseMnemonic = false;
            this.buttonLogo.UseVisualStyleBackColor = true;
            this.buttonLogo.Click += new System.EventHandler(this.ButtonLogo_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.NotifyIcon1_DoubleClick);
            // 
            // flowLayoutPanelRates
            // 
            this.flowLayoutPanelRates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelRates.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelRates.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanelRates.Name = "flowLayoutPanelRates";
            this.flowLayoutPanelRates.Size = new System.Drawing.Size(737, 40);
            this.flowLayoutPanelRates.TabIndex = 107;
            this.flowLayoutPanelRates.WrapContents = false;
            this.flowLayoutPanelRates.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanelRates_Paint);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label_NotProfitable);
            this.groupBox1.Controls.Add(this.flowLayoutPanelRates);
            this.groupBox1.Location = new System.Drawing.Point(10, 233);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(743, 59);
            this.groupBox1.TabIndex = 108;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Group/Device Rates:";
            this.groupBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBox1_Paint);
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label_NotProfitable
            // 
            this.label_NotProfitable.AutoSize = true;
            this.label_NotProfitable.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_NotProfitable.ForeColor = System.Drawing.Color.Red;
            this.label_NotProfitable.Location = new System.Drawing.Point(6, 0);
            this.label_NotProfitable.Name = "label_NotProfitable";
            this.label_NotProfitable.Size = new System.Drawing.Size(366, 24);
            this.label_NotProfitable.TabIndex = 110;
            this.label_NotProfitable.Text = "CURRENTLY MINING NOT PROFITABLE.";
            // 
            // buttonChart
            // 
            this.buttonChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChart.FlatAppearance.BorderSize = 0;
            this.buttonChart.Location = new System.Drawing.Point(647, 201);
            this.buttonChart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonChart.Name = "buttonChart";
            this.buttonChart.Size = new System.Drawing.Size(105, 23);
            this.buttonChart.TabIndex = 8;
            this.buttonChart.Text = "Profit chart";
            this.buttonChart.UseVisualStyleBackColor = true;
            this.buttonChart.Click += new System.EventHandler(this.ButtonChart_Click);
            // 
            // labelWallet
            // 
            this.labelWallet.AutoSize = true;
            this.labelWallet.Location = new System.Drawing.Point(7, 15);
            this.labelWallet.Name = "labelWallet";
            this.labelWallet.Size = new System.Drawing.Size(40, 13);
            this.labelWallet.TabIndex = 112;
            this.labelWallet.Text = "Wallet:";
            // 
            // textBoxWallet
            // 
            this.textBoxWallet.Location = new System.Drawing.Point(63, 12);
            this.textBoxWallet.Name = "textBoxWallet";
            this.textBoxWallet.ReadOnly = true;
            this.textBoxWallet.Size = new System.Drawing.Size(319, 20);
            this.textBoxWallet.TabIndex = 111;
            // 
            // label_Uptime
            // 
            this.label_Uptime.AutoSize = true;
            this.label_Uptime.Location = new System.Drawing.Point(200, 67);
            this.label_Uptime.Name = "label_Uptime";
            this.label_Uptime.Size = new System.Drawing.Size(43, 13);
            this.label_Uptime.TabIndex = 115;
            this.label_Uptime.Text = "Uptime:";
            // 
            // labelPayoutCurrency
            // 
            this.labelPayoutCurrency.AutoSize = true;
            this.labelPayoutCurrency.Location = new System.Drawing.Point(7, 41);
            this.labelPayoutCurrency.Name = "labelPayoutCurrency";
            this.labelPayoutCurrency.Size = new System.Drawing.Size(88, 13);
            this.labelPayoutCurrency.TabIndex = 117;
            this.labelPayoutCurrency.Text = "Payout Currency:";
            // 
            // textBoxPayoutCurrency
            // 
            this.textBoxPayoutCurrency.Location = new System.Drawing.Point(105, 38);
            this.textBoxPayoutCurrency.Name = "textBoxPayoutCurrency";
            this.textBoxPayoutCurrency.ReadOnly = true;
            this.textBoxPayoutCurrency.Size = new System.Drawing.Size(82, 20);
            this.textBoxPayoutCurrency.TabIndex = 116;
            // 
            // linkLabelNewVersion
            // 
            this.linkLabelNewVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelNewVersion.AutoSize = true;
            this.linkLabelNewVersion.Location = new System.Drawing.Point(276, 1);
            this.linkLabelNewVersion.Margin = new System.Windows.Forms.Padding(3, 1, 6, 0);
            this.linkLabelNewVersion.Name = "linkLabelNewVersion";
            this.linkLabelNewVersion.Size = new System.Drawing.Size(23, 13);
            this.linkLabelNewVersion.TabIndex = 118;
            this.linkLabelNewVersion.TabStop = true;
            this.linkLabelNewVersion.Text = "link";
            this.linkLabelNewVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelNewVersion.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.linkLabelNewVersion);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(459, 62);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(305, 26);
            this.flowLayoutPanel1.TabIndex = 119;
            // 
            // labelTreshold
            // 
            this.labelTreshold.AutoSize = true;
            this.labelTreshold.Location = new System.Drawing.Point(200, 41);
            this.labelTreshold.Name = "labelTreshold";
            this.labelTreshold.Size = new System.Drawing.Size(91, 13);
            this.labelTreshold.TabIndex = 121;
            this.labelTreshold.Text = "Payment treshold:";
            // 
            // textBoxTreshold
            // 
            this.textBoxTreshold.Location = new System.Drawing.Point(297, 38);
            this.textBoxTreshold.Name = "textBoxTreshold";
            this.textBoxTreshold.ReadOnly = true;
            this.textBoxTreshold.Size = new System.Drawing.Size(85, 20);
            this.textBoxTreshold.TabIndex = 120;
            // 
            // devicesListViewEnableControl1
            // 
            this.devicesListViewEnableControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesListViewEnableControl1.BackColor = System.Drawing.SystemColors.Control;
            this.devicesListViewEnableControl1.BenchmarkCalculation = null;
            this.devicesListViewEnableControl1.FirstColumnText = "Enabled";
            this.devicesListViewEnableControl1.IsInBenchmark = false;
            this.devicesListViewEnableControl1.IsMining = false;
            this.devicesListViewEnableControl1.Location = new System.Drawing.Point(13, 93);
            this.devicesListViewEnableControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.devicesListViewEnableControl1.Name = "devicesListViewEnableControl1";
            this.devicesListViewEnableControl1.SaveToGeneralConfig = false;
            this.devicesListViewEnableControl1.Size = new System.Drawing.Size(630, 129);
            this.devicesListViewEnableControl1.TabIndex = 109;
            this.devicesListViewEnableControl1.Load += new System.EventHandler(this.devicesListViewEnableControl1_Load);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 315);
            this.Controls.Add(this.labelTreshold);
            this.Controls.Add(this.textBoxTreshold);
            this.Controls.Add(this.devicesListViewEnableControl1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.buttonLogo);
            this.Controls.Add(this.labelPayoutCurrency);
            this.Controls.Add(this.textBoxPayoutCurrency);
            this.Controls.Add(this.label_Uptime);
            this.Controls.Add(this.labelWallet);
            this.Controls.Add(this.textBoxWallet);
            this.Controls.Add(this.buttonChart);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonSettings);
            this.Controls.Add(this.buttonBenchmark);
            this.Controls.Add(this.buttonStopMining);
            this.Controls.Add(this.labelWorkerName);
            this.Controls.Add(this.textBoxWorkerName);
            this.Controls.Add(this.linkLabelCheckStats);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.comboBoxRegion);
            this.Controls.Add(this.labelServiceLocation);
            this.Controls.Add(this.buttonStartMining);
            this.Enabled = false;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(720, 354);
            this.Name = "Form_Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Miner Legacy";
            this.Activated += new System.EventHandler(this.Form_Main_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form_Main_Shown);
            this.ResizeBegin += new System.EventHandler(this.Form_Main_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form_Main_ResizeEnd);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form_Main_Paint);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelServiceLocation;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.LinkLabel linkLabelCheckStats;
        private System.Windows.Forms.Label labelWorkerName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGlobalRateValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceText;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceBTCValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceBTCCode;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGlobalRateText;
        private System.Windows.Forms.Button buttonBenchmark;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBTCDayText;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBTCDayValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceDollarText;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ComboBox comboBoxRegion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_NotProfitable;
        private System.Windows.Forms.Button buttonChart;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label labelWallet;
        private System.Windows.Forms.TextBox textBoxWallet;
        private Forms.Components.DevicesListViewEnableControl devicesListViewEnableControl1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceDollarValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power3;
        public System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRates;
        public System.Windows.Forms.TextBox textBoxWorkerName;
        public System.Windows.Forms.Button buttonLogo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_power6;
        public System.Windows.Forms.Button buttonStopMining;
        public System.Windows.Forms.Button buttonStartMining;
        private System.Windows.Forms.Label labelPayoutCurrency;
        private System.Windows.Forms.TextBox textBoxPayoutCurrency;
        public System.Windows.Forms.Label label_Uptime;
        private System.Windows.Forms.LinkLabel linkLabelNewVersion;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label labelTreshold;
        public System.Windows.Forms.TextBox textBoxTreshold;
    }
}



