
namespace ZergPoolMiner.Forms.Components
{
    partial class WalletsListView
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.listViewWallets = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewWallets
            // 
            this.listViewWallets.CheckBoxes = true;
            this.listViewWallets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewWallets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewWallets.FullRowSelect = true;
            this.listViewWallets.GridLines = true;
            this.listViewWallets.HideSelection = false;
            this.listViewWallets.Location = new System.Drawing.Point(0, 0);
            this.listViewWallets.MultiSelect = false;
            this.listViewWallets.Name = "listViewWallets";
            this.listViewWallets.Size = new System.Drawing.Size(589, 86);
            this.listViewWallets.TabIndex = 0;
            this.listViewWallets.UseCompatibleStateImageBehavior = false;
            this.listViewWallets.View = System.Windows.Forms.View.Details;
            this.listViewWallets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listViewWallets_ItemCheck);
            this.listViewWallets.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listViewWallets_MouseDown);
            this.listViewWallets.Resize += new System.EventHandler(this.listViewWallets_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Use";
            this.columnHeader1.Width = 26;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Coin";
            this.columnHeader2.Width = 82;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Treshold";
            this.columnHeader3.Width = 95;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Wallet";
            this.columnHeader4.Width = 251;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Worker";
            this.columnHeader5.Width = 103;
            // 
            // WalletsListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listViewWallets);
            this.Name = "WalletsListView";
            this.Size = new System.Drawing.Size(589, 86);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListView listViewWallets;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}
