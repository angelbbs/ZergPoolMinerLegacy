namespace ZergPoolMiner.Forms.Components {
    partial class GroupProfitControl {
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.groupBoxMinerGroup = new System.Windows.Forms.GroupBox();
            this.labelBTCRateIndicator = new System.Windows.Forms.Label();
            this.richTextBoxSpeedValue = new System.Windows.Forms.TextBox();
            this.button_restart = new System.Windows.Forms.Button();
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxMinerGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMinerGroup
            // 
            this.groupBoxMinerGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMinerGroup.Controls.Add(this.button_restart);
            this.groupBoxMinerGroup.Controls.Add(this.labelBTCRateIndicator);
            this.groupBoxMinerGroup.Controls.Add(this.richTextBoxSpeedValue);
            this.groupBoxMinerGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxMinerGroup.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMinerGroup.Name = "groupBoxMinerGroup";
            this.groupBoxMinerGroup.Padding = new System.Windows.Forms.Padding(3, 3, 8, 3);
            this.groupBoxMinerGroup.Size = new System.Drawing.Size(540, 40);
            this.groupBoxMinerGroup.TabIndex = 108;
            this.groupBoxMinerGroup.TabStop = false;
            this.groupBoxMinerGroup.Text = "Mining Devices { N/A } ";
            this.groupBoxMinerGroup.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBoxMinerGroup_Paint);
            // 
            // labelBTCRateIndicator
            // 
            this.labelBTCRateIndicator.AutoSize = true;
            this.labelBTCRateIndicator.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelBTCRateIndicator.Location = new System.Drawing.Point(499, 16);
            this.labelBTCRateIndicator.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.labelBTCRateIndicator.Name = "labelBTCRateIndicator";
            this.labelBTCRateIndicator.Size = new System.Drawing.Size(33, 13);
            this.labelBTCRateIndicator.TabIndex = 106;
            this.labelBTCRateIndicator.Text = "Rate:";
            this.labelBTCRateIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelBTCRateIndicator.Click += new System.EventHandler(this.labelBTCRateIndicator_Click);
            // 
            // richTextBoxSpeedValue
            // 
            this.richTextBoxSpeedValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxSpeedValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxSpeedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBoxSpeedValue.Location = new System.Drawing.Point(10, 18);
            this.richTextBoxSpeedValue.Name = "richTextBoxSpeedValue";
            this.richTextBoxSpeedValue.ReadOnly = true;
            this.richTextBoxSpeedValue.Size = new System.Drawing.Size(239, 13);
            this.richTextBoxSpeedValue.TabIndex = 115;
            this.richTextBoxSpeedValue.Enter += new System.EventHandler(this.richTextBoxSpeedValue_Enter);
            // 
            // button_restart
            // 
            this.button_restart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_restart.FlatAppearance.BorderSize = 0;
            this.button_restart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_restart.Image = global::ZergPoolMiner.Properties.Resources.Refresh_hot_bw;
            this.button_restart.Location = new System.Drawing.Point(522, 2);
            this.button_restart.Margin = new System.Windows.Forms.Padding(1);
            this.button_restart.Name = "button_restart";
            this.button_restart.Size = new System.Drawing.Size(20, 20);
            this.button_restart.TabIndex = 114;
            this.button_restart.Tag = "";
            this.toolTip2.SetToolTip(this.button_restart, "Restart miner");
            this.button_restart.UseVisualStyleBackColor = false;
            this.button_restart.Click += new System.EventHandler(this.buttonBTC_restart);
            this.button_restart.MouseLeave += new System.EventHandler(this.button_restart_MouseLeave);
            this.button_restart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_restart_MouseMove);
            // 
            // GroupProfitControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.groupBoxMinerGroup);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.Name = "GroupProfitControl";
            this.Size = new System.Drawing.Size(548, 43);
            this.groupBoxMinerGroup.ResumeLayout(false);
            this.groupBoxMinerGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMinerGroup;
        private System.Windows.Forms.Label labelBTCRateIndicator;
        private System.Windows.Forms.Button button_restart;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.TextBox richTextBoxSpeedValue;
    }
}
