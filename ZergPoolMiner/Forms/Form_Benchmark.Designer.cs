namespace ZergPoolMiner.Forms {
    partial class Form_Benchmark {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.StartStopBtn = new System.Windows.Forms.Button();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.groupBoxBenchmarkProgress = new System.Windows.Forms.GroupBox();
            this.labelBenchmarkSteps = new System.Windows.Forms.Label();
            this.progressBarBenchmarkSteps = new System.Windows.Forms.ProgressBar();
            this.radioButton_SelectedUnbenchmarked = new System.Windows.Forms.RadioButton();
            this.radioButton_RE_SelectedUnbenchmarked = new System.Windows.Forms.RadioButton();
            this.checkBox_StartMiningAfterBenchmark = new System.Windows.Forms.CheckBox();
            this.algorithmsListView1 = new ZergPoolMiner.Forms.Components.AlgorithmsListView();
            this.devicesListViewEnableControl1 = new ZergPoolMiner.Forms.Components.DevicesListViewEnableControl();
            this.benchmarkOptions1 = new ZergPoolMiner.Forms.Components.BenchmarkOptions();
            this.checkBoxHideUnused = new System.Windows.Forms.CheckBox();
            this.label_profile = new System.Windows.Forms.Label();
            this.comboBox_profile = new System.Windows.Forms.ComboBox();
            this.groupBoxBenchmarkProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartStopBtn
            // 
            this.StartStopBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartStopBtn.BackColor = System.Drawing.SystemColors.Control;
            this.StartStopBtn.Location = new System.Drawing.Point(474, 476);
            this.StartStopBtn.Name = "StartStopBtn";
            this.StartStopBtn.Size = new System.Drawing.Size(117, 23);
            this.StartStopBtn.TabIndex = 100;
            this.StartStopBtn.Text = "&Start";
            this.StartStopBtn.UseVisualStyleBackColor = false;
            this.StartStopBtn.Click += new System.EventHandler(this.StartStopBtn_Click);
            // 
            // CloseBtn
            // 
            this.CloseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseBtn.Location = new System.Drawing.Point(597, 476);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(75, 23);
            this.CloseBtn.TabIndex = 101;
            this.CloseBtn.Text = "&Close";
            this.CloseBtn.UseVisualStyleBackColor = true;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // groupBoxBenchmarkProgress
            // 
            this.groupBoxBenchmarkProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxBenchmarkProgress.Controls.Add(this.labelBenchmarkSteps);
            this.groupBoxBenchmarkProgress.Controls.Add(this.progressBarBenchmarkSteps);
            this.groupBoxBenchmarkProgress.Location = new System.Drawing.Point(189, 453);
            this.groupBoxBenchmarkProgress.Name = "groupBoxBenchmarkProgress";
            this.groupBoxBenchmarkProgress.Size = new System.Drawing.Size(269, 47);
            this.groupBoxBenchmarkProgress.TabIndex = 108;
            this.groupBoxBenchmarkProgress.TabStop = false;
            this.groupBoxBenchmarkProgress.Text = "Benchmark progress status:";
            this.groupBoxBenchmarkProgress.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBoxBenchmarkProgress_Paint);
            // 
            // labelBenchmarkSteps
            // 
            this.labelBenchmarkSteps.AutoSize = true;
            this.labelBenchmarkSteps.Location = new System.Drawing.Point(6, 24);
            this.labelBenchmarkSteps.Name = "labelBenchmarkSteps";
            this.labelBenchmarkSteps.Size = new System.Drawing.Size(116, 13);
            this.labelBenchmarkSteps.TabIndex = 109;
            this.labelBenchmarkSteps.Text = "Benchmark step (0/10)";
            // 
            // progressBarBenchmarkSteps
            // 
            this.progressBarBenchmarkSteps.Location = new System.Drawing.Point(162, 16);
            this.progressBarBenchmarkSteps.Name = "progressBarBenchmarkSteps";
            this.progressBarBenchmarkSteps.Size = new System.Drawing.Size(101, 23);
            this.progressBarBenchmarkSteps.TabIndex = 108;
            // 
            // radioButton_SelectedUnbenchmarked
            // 
            this.radioButton_SelectedUnbenchmarked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_SelectedUnbenchmarked.AutoSize = true;
            this.radioButton_SelectedUnbenchmarked.Checked = true;
            this.radioButton_SelectedUnbenchmarked.Location = new System.Drawing.Point(12, 407);
            this.radioButton_SelectedUnbenchmarked.Name = "radioButton_SelectedUnbenchmarked";
            this.radioButton_SelectedUnbenchmarked.Size = new System.Drawing.Size(260, 17);
            this.radioButton_SelectedUnbenchmarked.TabIndex = 110;
            this.radioButton_SelectedUnbenchmarked.TabStop = true;
            this.radioButton_SelectedUnbenchmarked.Text = "Benchmark Selected Unbenchmarked Algorithms ";
            this.radioButton_SelectedUnbenchmarked.UseVisualStyleBackColor = true;
            this.radioButton_SelectedUnbenchmarked.CheckedChanged += new System.EventHandler(this.RadioButton_SelectedUnbenchmarked_CheckedChanged_1);
            // 
            // radioButton_RE_SelectedUnbenchmarked
            // 
            this.radioButton_RE_SelectedUnbenchmarked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_RE_SelectedUnbenchmarked.AutoSize = true;
            this.radioButton_RE_SelectedUnbenchmarked.Location = new System.Drawing.Point(12, 430);
            this.radioButton_RE_SelectedUnbenchmarked.Name = "radioButton_RE_SelectedUnbenchmarked";
            this.radioButton_RE_SelectedUnbenchmarked.Size = new System.Drawing.Size(192, 17);
            this.radioButton_RE_SelectedUnbenchmarked.TabIndex = 110;
            this.radioButton_RE_SelectedUnbenchmarked.Text = "Benchmark All Selected Algorithms ";
            this.radioButton_RE_SelectedUnbenchmarked.UseVisualStyleBackColor = true;
            this.radioButton_RE_SelectedUnbenchmarked.CheckedChanged += new System.EventHandler(this.RadioButton_RE_SelectedUnbenchmarked_CheckedChanged);
            // 
            // checkBox_StartMiningAfterBenchmark
            // 
            this.checkBox_StartMiningAfterBenchmark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_StartMiningAfterBenchmark.AutoSize = true;
            this.checkBox_StartMiningAfterBenchmark.Location = new System.Drawing.Point(350, 432);
            this.checkBox_StartMiningAfterBenchmark.Name = "checkBox_StartMiningAfterBenchmark";
            this.checkBox_StartMiningAfterBenchmark.Size = new System.Drawing.Size(161, 17);
            this.checkBox_StartMiningAfterBenchmark.TabIndex = 111;
            this.checkBox_StartMiningAfterBenchmark.Text = "Start mining after benchmark";
            this.checkBox_StartMiningAfterBenchmark.UseVisualStyleBackColor = true;
            this.checkBox_StartMiningAfterBenchmark.CheckedChanged += new System.EventHandler(this.CheckBox_StartMiningAfterBenchmark_CheckedChanged);
            // 
            // algorithmsListView1
            // 
            this.algorithmsListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.algorithmsListView1.BackColor = System.Drawing.SystemColors.Control;
            this.algorithmsListView1.BenchmarkCalculation = null;
            //this.algorithmsListView1.ComunicationInterface = null;
            this.algorithmsListView1.IsInBenchmark = false;
            this.algorithmsListView1.Location = new System.Drawing.Point(12, 190);
            this.algorithmsListView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.algorithmsListView1.Name = "algorithmsListView1";
            this.algorithmsListView1.Size = new System.Drawing.Size(660, 209);
            this.algorithmsListView1.TabIndex = 109;
            this.algorithmsListView1.Load += new System.EventHandler(this.algorithmsListView1_Load);
            // 
            // devicesListViewEnableControl1
            // 
            this.devicesListViewEnableControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesListViewEnableControl1.BackColor = System.Drawing.SystemColors.Control;
            this.devicesListViewEnableControl1.BenchmarkCalculation = null;
            this.devicesListViewEnableControl1.FirstColumnText = "Benckmark";
            this.devicesListViewEnableControl1.IsInBenchmark = false;
            this.devicesListViewEnableControl1.IsMining = false;
            this.devicesListViewEnableControl1.Location = new System.Drawing.Point(12, 15);
            this.devicesListViewEnableControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.devicesListViewEnableControl1.Name = "devicesListViewEnableControl1";
            this.devicesListViewEnableControl1.SaveToGeneralConfig = false;
            this.devicesListViewEnableControl1.Size = new System.Drawing.Size(456, 164);
            this.devicesListViewEnableControl1.TabIndex = 0;
            this.devicesListViewEnableControl1.Load += new System.EventHandler(this.devicesListViewEnableControl1_Load);
            // 
            // benchmarkOptions1
            // 
            this.benchmarkOptions1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.benchmarkOptions1.Location = new System.Drawing.Point(474, 15);
            this.benchmarkOptions1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.benchmarkOptions1.Name = "benchmarkOptions1";
            this.benchmarkOptions1.Size = new System.Drawing.Size(208, 140);
            this.benchmarkOptions1.TabIndex = 106;
            // 
            // checkBoxHideUnused
            // 
            this.checkBoxHideUnused.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxHideUnused.AutoSize = true;
            this.checkBoxHideUnused.ForeColor = System.Drawing.SystemColors.ControlText;
            this.checkBoxHideUnused.Location = new System.Drawing.Point(350, 409);
            this.checkBoxHideUnused.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBoxHideUnused.Name = "checkBoxHideUnused";
            this.checkBoxHideUnused.Size = new System.Drawing.Size(136, 17);
            this.checkBoxHideUnused.TabIndex = 401;
            this.checkBoxHideUnused.Text = "Hide unused algorithms";
            this.checkBoxHideUnused.UseVisualStyleBackColor = true;
            this.checkBoxHideUnused.CheckedChanged += new System.EventHandler(this.checkBoxHideUnused_CheckedChanged);
            // 
            // label_profile
            // 
            this.label_profile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_profile.AutoSize = true;
            this.label_profile.Location = new System.Drawing.Point(11, 477);
            this.label_profile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_profile.Name = "label_profile";
            this.label_profile.Size = new System.Drawing.Size(36, 13);
            this.label_profile.TabIndex = 403;
            this.label_profile.Text = "Profile";
            // 
            // comboBox_profile
            // 
            this.comboBox_profile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox_profile.BackColor = System.Drawing.SystemColors.Control;
            this.comboBox_profile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_profile.FormattingEnabled = true;
            this.comboBox_profile.Items.AddRange(new object[] {
            "Default"});
            this.comboBox_profile.Location = new System.Drawing.Point(64, 474);
            this.comboBox_profile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_profile.Name = "comboBox_profile";
            this.comboBox_profile.Size = new System.Drawing.Size(99, 21);
            this.comboBox_profile.TabIndex = 402;
            this.comboBox_profile.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_profile_DrawItem);
            this.comboBox_profile.SelectedIndexChanged += new System.EventHandler(this.comboBox_profile_SelectedIndexChanged);
            // 
            // Form_Benchmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 511);
            this.Controls.Add(this.label_profile);
            this.Controls.Add(this.comboBox_profile);
            this.Controls.Add(this.checkBoxHideUnused);
            this.Controls.Add(this.checkBox_StartMiningAfterBenchmark);
            this.Controls.Add(this.radioButton_RE_SelectedUnbenchmarked);
            this.Controls.Add(this.radioButton_SelectedUnbenchmarked);
            this.Controls.Add(this.algorithmsListView1);
            this.Controls.Add(this.groupBoxBenchmarkProgress);
            this.Controls.Add(this.benchmarkOptions1);
            this.Controls.Add(this.StartStopBtn);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.devicesListViewEnableControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 470);
            this.Name = "Form_Benchmark";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Benchmark";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBenchmark_New_FormClosing);
            this.ResizeBegin += new System.EventHandler(this.Form_Benchmark_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form_Benchmark_ResizeEnd);
            this.groupBoxBenchmarkProgress.ResumeLayout(false);
            this.groupBoxBenchmarkProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Components.DevicesListViewEnableControl devicesListViewEnableControl1;
        private System.Windows.Forms.Button StartStopBtn;
        private System.Windows.Forms.Button CloseBtn;
        private System.Windows.Forms.GroupBox groupBoxBenchmarkProgress;
        private System.Windows.Forms.Label labelBenchmarkSteps;
        private System.Windows.Forms.ProgressBar progressBarBenchmarkSteps;
        private Components.AlgorithmsListView algorithmsListView1;
        private System.Windows.Forms.RadioButton radioButton_SelectedUnbenchmarked;
        private System.Windows.Forms.RadioButton radioButton_RE_SelectedUnbenchmarked;
        private System.Windows.Forms.CheckBox checkBox_StartMiningAfterBenchmark;
        private Components.BenchmarkOptions benchmarkOptions1;
        private System.Windows.Forms.CheckBox checkBoxHideUnused;
        private System.Windows.Forms.Label label_profile;
        private System.Windows.Forms.ComboBox comboBox_profile;
    }
}
