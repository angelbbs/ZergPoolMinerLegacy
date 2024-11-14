
namespace ZergPoolMiner.Forms
{
    partial class Form_AddWallet
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
            this.label_Payout_Currency = new System.Windows.Forms.Label();
            this.comboBox_Coin = new System.Windows.Forms.ComboBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBox_Wallet = new System.Windows.Forms.TextBox();
            this.label_Wallet = new System.Windows.Forms.Label();
            this.textBox_worker = new System.Windows.Forms.TextBox();
            this.label_Worker = new System.Windows.Forms.Label();
            this.labelPayoutThreshold = new System.Windows.Forms.Label();
            this.textBox_PayoutThreshold = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label_Payout_Currency
            // 
            this.label_Payout_Currency.AutoSize = true;
            this.label_Payout_Currency.Location = new System.Drawing.Point(9, 40);
            this.label_Payout_Currency.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Payout_Currency.Name = "label_Payout_Currency";
            this.label_Payout_Currency.Size = new System.Drawing.Size(85, 13);
            this.label_Payout_Currency.TabIndex = 373;
            this.label_Payout_Currency.Text = "Payout Currency";
            // 
            // comboBox_Coin
            // 
            this.comboBox_Coin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Coin.FormattingEnabled = true;
            this.comboBox_Coin.Location = new System.Drawing.Point(104, 37);
            this.comboBox_Coin.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_Coin.Name = "comboBox_Coin";
            this.comboBox_Coin.Size = new System.Drawing.Size(196, 21);
            this.comboBox_Coin.TabIndex = 372;
            this.comboBox_Coin.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_Coin_DrawItem);
            this.comboBox_Coin.SelectedIndexChanged += new System.EventHandler(this.comboBox_Coin_SelectedIndexChanged);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSave.Location = new System.Drawing.Point(12, 121);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(97, 23);
            this.buttonSave.TabIndex = 386;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.Location = new System.Drawing.Point(145, 121);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(97, 23);
            this.buttonCancel.TabIndex = 387;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBox_Wallet
            // 
            this.textBox_Wallet.Location = new System.Drawing.Point(66, 11);
            this.textBox_Wallet.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_Wallet.Name = "textBox_Wallet";
            this.textBox_Wallet.Size = new System.Drawing.Size(507, 20);
            this.textBox_Wallet.TabIndex = 388;
            // 
            // label_Wallet
            // 
            this.label_Wallet.AutoSize = true;
            this.label_Wallet.Location = new System.Drawing.Point(9, 14);
            this.label_Wallet.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Wallet.Name = "label_Wallet";
            this.label_Wallet.Size = new System.Drawing.Size(37, 13);
            this.label_Wallet.TabIndex = 389;
            this.label_Wallet.Text = "Wallet";
            // 
            // textBox_worker
            // 
            this.textBox_worker.Location = new System.Drawing.Point(104, 64);
            this.textBox_worker.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_worker.Name = "textBox_worker";
            this.textBox_worker.Size = new System.Drawing.Size(196, 20);
            this.textBox_worker.TabIndex = 390;
            // 
            // label_Worker
            // 
            this.label_Worker.AutoSize = true;
            this.label_Worker.Location = new System.Drawing.Point(9, 67);
            this.label_Worker.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Worker.Name = "label_Worker";
            this.label_Worker.Size = new System.Drawing.Size(73, 13);
            this.label_Worker.TabIndex = 391;
            this.label_Worker.Text = "Worker Name";
            // 
            // labelPayoutThreshold
            // 
            this.labelPayoutThreshold.AutoSize = true;
            this.labelPayoutThreshold.Location = new System.Drawing.Point(331, 40);
            this.labelPayoutThreshold.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPayoutThreshold.Name = "labelPayoutThreshold";
            this.labelPayoutThreshold.Size = new System.Drawing.Size(86, 13);
            this.labelPayoutThreshold.TabIndex = 392;
            this.labelPayoutThreshold.Text = "Payout threshold";
            // 
            // textBox_PayoutThreshold
            // 
            this.textBox_PayoutThreshold.Location = new System.Drawing.Point(421, 37);
            this.textBox_PayoutThreshold.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_PayoutThreshold.Name = "textBox_PayoutThreshold";
            this.textBox_PayoutThreshold.Size = new System.Drawing.Size(152, 20);
            this.textBox_PayoutThreshold.TabIndex = 393;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(418, 67);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(55, 13);
            this.linkLabel1.TabIndex = 394;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "linkLabel1";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Form_AddWallet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 156);
            this.ControlBox = false;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.textBox_PayoutThreshold);
            this.Controls.Add(this.labelPayoutThreshold);
            this.Controls.Add(this.textBox_worker);
            this.Controls.Add(this.label_Worker);
            this.Controls.Add(this.textBox_Wallet);
            this.Controls.Add(this.label_Wallet);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label_Payout_Currency);
            this.Controls.Add(this.comboBox_Coin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form_AddWallet";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Add wallet";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Payout_Currency;
        private System.Windows.Forms.ComboBox comboBox_Coin;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBox_Wallet;
        private System.Windows.Forms.Label label_Wallet;
        private System.Windows.Forms.TextBox textBox_worker;
        private System.Windows.Forms.Label label_Worker;
        private System.Windows.Forms.Label labelPayoutThreshold;
        private System.Windows.Forms.TextBox textBox_PayoutThreshold;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}