namespace ZergPoolMiner
{
    partial class Form_Loading
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
            this.LoadText = new System.Windows.Forms.Label();
            this.label_LoadingText = new System.Windows.Forms.Label();
            this.progressBar2 = new ProgressBarSample.TextProgressBar();
            this.SuspendLayout();
            //
            // LoadText
            //
            this.LoadText.AutoSize = true;
            this.LoadText.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LoadText.Location = new System.Drawing.Point(9, 51);
            this.LoadText.Name = "LoadText";
            this.LoadText.Size = new System.Drawing.Size(283, 13);
            this.LoadText.TabIndex = 2;
            this.LoadText.Text = "                                                                                 " +
    "           ";
            //
            // label_LoadingText
            //
            this.label_LoadingText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_LoadingText.AutoSize = true;
            this.label_LoadingText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_LoadingText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label_LoadingText.Location = new System.Drawing.Point(84, 9);
            this.label_LoadingText.Name = "label_LoadingText";
            this.label_LoadingText.Size = new System.Drawing.Size(136, 13);
            this.label_LoadingText.TabIndex = 0;
            this.label_LoadingText.Text = "Loading, please wait...";
            this.label_LoadingText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // progressBar2
            //
            this.progressBar2.CustomText = "";
            this.progressBar2.Location = new System.Drawing.Point(12, 28);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(0);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.ProgressColor = System.Drawing.SystemColors.ControlDark;
            this.progressBar2.Size = new System.Drawing.Size(286, 23);
            this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar2.TabIndex = 397;
            this.progressBar2.TextColor = System.Drawing.Color.Black;
            this.progressBar2.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.progressBar2.VisualMode = ProgressBarSample.ProgressBarDisplayMode.Percentage;
            //
            // Form_Loading
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(310, 76);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.LoadText);
            this.Controls.Add(this.label_LoadingText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Loading";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form_Loading";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.Form_Loading_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label LoadText;
        private System.Windows.Forms.Label label_LoadingText;
        private ProgressBarSample.TextProgressBar progressBar2;
    }
}