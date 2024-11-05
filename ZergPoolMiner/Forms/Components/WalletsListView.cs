using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZergPoolMiner.Configs;

namespace ZergPoolMiner.Forms.Components
{
    public partial class WalletsListView : UserControl
    {
        //public IWalletsListView ComunicationInterface { get; set; }
        private static int _SubItembIndex = 0;
        private static char _keyPressed;
        public WalletsListView()
        {
            InitializeComponent();
            this.listViewWallets.ColumnWidthChanging += new ColumnWidthChangingEventHandler(listViewWallets_ColumnWidthChanging);
            listViewWallets.DoubleBuffer();
            WalletsListView.colorListViewHeader(ref listViewWallets, Form_Main._backColor, Form_Main._textColor);

            // callback initializations
            listViewWallets.ItemSelectionChanged += listViewWallets_ItemSelectionChanged;
            listViewWallets.MultiSelect = false;
            listViewWallets.FullRowSelect = true;
        }
        public void WalletsListInit()
        {
            listViewWallets.BeginUpdate();
            listViewWallets.Items.Clear();

            Font fontRegular = new Font(this.Font, FontStyle.Regular);
            Font fontBold = new Font(this.Font, FontStyle.Bold);

            foreach (var wal in Wallets.Wallets.WalletDataList)
            {
                var lvi = new ListViewItem();
                lvi.Checked = wal.Use;
                lvi.SubItems.Add(wal.Coin);
                lvi.SubItems.Add(wal.Treshold.ToString());
                lvi.SubItems.Add(wal.Wallet);
                lvi.SubItems.Add(wal.ID);
                lvi.Font = fontRegular;
                listViewWallets.Items.Add(lvi);
            }
            listViewWallets.EndUpdate();
        }
        public void InitLocale()
        {
            var _backColor = Form_Main._backColor;
            var _foreColor = Form_Main._foreColor;
            var _textColor = Form_Main._textColor;

            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                listViewWallets.BackColor = _backColor;
                listViewWallets.ForeColor = _textColor;
                this.BackColor = _backColor;
            }
            else
            {
                listViewWallets.BackColor = SystemColors.ControlLightLight;
                listViewWallets.ForeColor = _textColor;
                this.BackColor = SystemColors.ControlLightLight;
            }
            listViewWallets.Columns[0].Text = International.GetText("WalletListView_Enabled");
            listViewWallets.Columns[1].Text = International.GetText("WalletListView_Coin");
            listViewWallets.Columns[2].Text = International.GetText("WalletListView_Treshold");
            listViewWallets.Columns[3].Text = International.GetText("WalletListView_Wallet");
            listViewWallets.Columns[4].Text = International.GetText("WalletListView_Worker");
        }

        public void SaveWallets()
        {
            try
            {
                Wallets.Wallets.WalletDataList.Clear();
                foreach (ListViewItem lvi in listViewWallets.Items)
                {
                    Wallets.Wallets.WalletData wallet = new Wallets.Wallets.WalletData();
                    wallet.Use = lvi.Checked;
                    wallet.Coin = lvi.SubItems[1].Text;
                    double.TryParse(lvi.SubItems[2].Text, out var _treshold);
                    wallet.Treshold = _treshold;
                    wallet.Wallet = lvi.SubItems[3].Text;
                    wallet.ID = lvi.SubItems[4].Text;
                    Wallets.Wallets.WalletDataList.Add(wallet);
                }
                string json = JsonConvert.SerializeObject(Wallets.Wallets.WalletDataList, Formatting.Indented);
                Helpers.WriteAllTextWithBackup("configs\\wallets.json", json);
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("SaveWallets", ex.ToString());
            }
            string currency = "";
            double treshold = 0d;
            string _wallet = "";
            string worker = "";
            Form_Main._demoMode = true;
            foreach (var wal in Wallets.Wallets.WalletDataList)
            {
                if (wal.Use)
                {
                    Form_Main._demoMode = false;
                    currency = wal.Coin;
                    treshold = wal.Treshold;
                    _wallet = wal.Wallet;
                    worker = wal.ID;
                    break;
                }
            }
            ConfigManager.GeneralConfig.Wallet = _wallet.Trim();
            ConfigManager.GeneralConfig.WorkerName = worker.Trim();
            ConfigManager.GeneralConfig.PayoutCurrency = currency.Trim();
            ConfigManager.GeneralConfig.PayoutCurrencyTreshold = treshold;
        }
        public void DeleteWallet()
        {
            foreach (ListViewItem lvi in listViewWallets.SelectedItems)
            {
                listViewWallets.BeginUpdate();
                var wal = lvi.SubItems[1].Text + ": " +
                    lvi.SubItems[2].Text + "." +
                    lvi.SubItems[3].Text;
                var dialogRes = MessageBox.Show(string.Format(International.GetText("Form_Settings_Del_wallet"),
                wal), "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogRes == DialogResult.No)
                {
                    return;
                }
                listViewWallets.Items.Remove(lvi);
                if (listViewWallets.Items.Count > 0) listViewWallets.Items[0].EnsureVisible();
                listViewWallets.Update();
                listViewWallets.Refresh();
                listViewWallets.EndUpdate();
            }
        }
        public void AddWallet(string coin, double treshold, string wallet, string worker)
        {
            var lvi = new ListViewItem();
            lvi.Checked = false;
            lvi.SubItems.Add(coin);
            lvi.SubItems.Add(treshold.ToString());
            lvi.SubItems.Add(wallet);
            lvi.SubItems.Add(worker);
            listViewWallets.Items.Add(lvi);
        }

        private void listViewWallets_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        void listViewWallets_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            listViewWallets.BeginUpdate();
            if (e.ColumnIndex <= 1 || e.ColumnIndex == 4)
            {
                e.NewWidth = this.listViewWallets.Columns[e.ColumnIndex].Width;
                e.Cancel = true;
            }
            if (e.ColumnIndex == 2)
            {
                ResizeAutoSizeColumn(listViewWallets, 3);
            }
            listViewWallets.EndUpdate();
        }
        static private void ResizeAutoSizeColumn(ListView listView, int autoSizeColumnIndex)
        {
            // Do some rudimentary (parameter) validation.
            if (listView == null) throw new ArgumentNullException("listView");
            if (listView.View != View.Details || listView.Columns.Count <= 0 || autoSizeColumnIndex < 0) return;
            if (autoSizeColumnIndex >= listView.Columns.Count)
                throw new IndexOutOfRangeException("Parameter autoSizeColumnIndex is outside the range of column indices in the ListView.");

            // Sum up the width of all columns except the auto-resizing one.
            int otherColumnsWidth = 0;
            foreach (ColumnHeader header in listView.Columns)
                if (header.Index != autoSizeColumnIndex)
                    otherColumnsWidth += header.Width;

            // Calculate the (possibly) new width of the auto-resizable column.
            int autoSizeColumnWidth = listView.ClientRectangle.Width - otherColumnsWidth;

            // Finally set the new width of the auto-resizing column, if it has changed.
            if (listView.Columns[autoSizeColumnIndex].Width != autoSizeColumnWidth)
                listView.Columns[autoSizeColumnIndex].Width = autoSizeColumnWidth;
        }
        private static void headerDraw(object sender, DrawListViewColumnHeaderEventArgs e, Color backColor, Color foreColor)
        {
            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            using (SolidBrush foreBrush = new SolidBrush(foreColor))
            {
                StringFormat sf = new StringFormat();
                if ((e.ColumnIndex == 0))
                {
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Near;
                }
                else
                {
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                }
                e.Graphics.DrawString(e.Header.Text, e.Font, foreBrush, e.Bounds, sf);
            }
        }

        private static void bodyDraw(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                using (SolidBrush backBrush = new SolidBrush(Form_Main._backColor))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }
            }
            else
            {
                using (SolidBrush backBrush = new SolidBrush(SystemColors.ControlLightLight))
                {
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                }
            }
        }
        public static void colorListViewHeader(ref ListView list, Color backColor, Color foreColor)
        {
            list.OwnerDraw = true;
            list.DrawColumnHeader +=
            new DrawListViewColumnHeaderEventHandler
            (
            (sender, e) => headerDraw(sender, e, backColor, foreColor)
            );
            list.DrawItem += new DrawListViewItemEventHandler(bodyDraw);

        }

        private void listViewWallets_Resize(object sender, EventArgs e)
        {
            listViewWallets.BeginUpdate();
            ResizeAutoSizeColumn(listViewWallets, 3);
            listViewWallets.EndUpdate();
        }

        private void listViewWallets_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_SubItembIndex > 0) return;
            foreach (ListViewItem lvi in listViewWallets.Items)
            {
                if (lvi is object)
                {
                    lvi.Checked = false;
                }
            }
        }

        private void listViewWallets_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewWallets.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                var CurrentSubItem = item.GetSubItemAt(e.X, e.Y);
                int SubItembIndex = item.SubItems.IndexOf(CurrentSubItem);
                _SubItembIndex = SubItembIndex;
                if (e.Clicks > 1)
                {
                    var c = item.Checked;
                    if (SubItembIndex == 1) item.Checked = !c;
                    if (SubItembIndex < 2) return;

                    TextBox tbox = new TextBox();
                    this.Controls.Add(tbox);
                    int x_cord = 0;
                    for (int i = 0; i < SubItembIndex; i++)
                        x_cord += listViewWallets.Columns[i].Width;

                    tbox.Width = listViewWallets.Columns[SubItembIndex].Width;
                    tbox.Height = listViewWallets.GetItemRect(0).Height - 2;
                    tbox.Left = x_cord;
                    tbox.Top = item.Position.Y;
                    tbox.Text = item.SubItems[SubItembIndex].Text;
                    tbox.TextAlign = listViewWallets.Columns[SubItembIndex].TextAlign;
                    tbox.Leave += DisposeTextBox;
                    tbox.KeyPress += TextBoxKeyPress;
                    listViewWallets.Controls.Add(tbox);
                    tbox.Focus();
                    tbox.SelectAll();
                    item.Checked = !c;
                }
            }
        }

        private void DisposeTextBox(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            if (_keyPressed != 27 && _keyPressed != 8)
            {
                foreach (ListViewItem lvi in listViewWallets.SelectedItems)
                {
                    lvi.SubItems[_SubItembIndex].Text = tb.Text;
                    if (_SubItembIndex == 2)
                    {
                        var c = Wallets.Wallets.CoinsList.First(item => item.Coin.Equals(lvi.SubItems[1].Text));
                        double.TryParse(tb.Text, out double treshold);
                        if (treshold < c.Treshold)
                        {
                            MessageBox.Show(International.GetText("Form_Settings_TresholdError") + c.Treshold.ToString(),
                                International.GetText("Error_with_Exclamation"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            lvi.SubItems[_SubItembIndex].Text = c.Treshold.ToString();
                            tb.Text = c.Treshold.ToString();
                        }
                    }
                }
                
            }
            tb.Dispose();
        }
        private static bool IsHandleZero(KeyPressEventArgs e, string checkText)
        {
            return !char.IsControl(e.KeyChar) && checkText.Length > 0 && checkText[0] == '0';
        }
        private static bool DoubleInvalid(char c)
        {
            return !char.IsControl(c) && !char.IsDigit(c) && (c != '.');
        }
        private void TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            char inputChar = e.KeyChar;

            if (_SubItembIndex == 2)
            {
                // allow only one zero
                if (sender is TextBox textBox)
                {
                    var checkText = textBox.Text;
                    if (e.KeyChar != '.' && textBox.SelectionLength != textBox.Text.Length && IsHandleZero(e, checkText) &&
                        !checkText.Contains("."))
                    {
                        e.Handled = true;
                        return;
                    }
                }
                if (DoubleInvalid(e.KeyChar))
                {
                    e.Handled = true;
                }
                // only allow one decimal point
                if ((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf('.') > -1))
                {
                    e.Handled = true;
                }
            }

            _keyPressed = inputChar;

            if ((inputChar <= 32 || inputChar >= 127) &&
                inputChar != 8)
            {
                e.Handled = true;
            }

            if (inputChar == 13)
            {
                DisposeTextBox((sender as TextBox), null);
            }
            if (inputChar == 27)
                DisposeTextBox((sender as TextBox), null);
        }
    }
}
