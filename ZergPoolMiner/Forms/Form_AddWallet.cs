using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZergPoolMiner.Configs;

namespace ZergPoolMiner.Forms
{
    public partial class Form_AddWallet : Form
    {
        Components.WalletsListView _walletsListView1;

        public Form_AddWallet(Components.WalletsListView walletsListView1)
        {
            _walletsListView1 = walletsListView1;
            InitializeComponent();
            label_Wallet.Text = International.GetText("Form_Add_wallet_walletlabel");
            label_Payout_Currency.Text = International.GetText("Form_Add_wallet_coinlabel");
            labelPayoutThreshold.Text = International.GetText("Form_Add_wallet_PayoutThreshold");
            label_Worker.Text = International.GetText("Form_Add_wallet_workerlabel");
            buttonSave.Text = International.GetText("Button_Save");
            buttonCancel.Text = International.GetText("Button_Cancel");

            textBox_PayoutThreshold.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
            linkLabel1.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabel1.Text = "https://zergpool.com/payouts";
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                if (ConfigManager.GeneralConfig.ColorProfileIndex == 14)
                {
                    linkLabel1.LinkColor = Color.DarkOliveGreen;
                    linkLabel1.ActiveLinkColor = Color.DarkCyan;
                }

                this.BackColor = Form_Main._backColor;
                this.ForeColor = Form_Main._foreColor;

                textBox_Wallet.BackColor = Form_Main._backColor;
                textBox_Wallet.ForeColor = Form_Main._foreColor;
                textBox_Wallet.BorderStyle = BorderStyle.FixedSingle;

                textBox_PayoutThreshold.BackColor = Form_Main._backColor;
                textBox_PayoutThreshold.ForeColor = Form_Main._foreColor;
                textBox_PayoutThreshold.BorderStyle = BorderStyle.FixedSingle;

                textBox_worker.BackColor = Form_Main._backColor;
                textBox_worker.ForeColor = Form_Main._foreColor;
                textBox_worker.BorderStyle = BorderStyle.FixedSingle;

                comboBox_Coin.BackColor = Form_Main._backColor;
                comboBox_Coin.ForeColor = Form_Main._textColor;
                comboBox_Coin.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;

                buttonCancel.BackColor = Form_Main._backColor;
                buttonCancel.ForeColor = Form_Main._textColor;
                buttonCancel.FlatStyle = FlatStyle.Flat;
                buttonCancel.FlatAppearance.BorderColor = Form_Main._textColor;
                buttonCancel.FlatAppearance.BorderSize = 1;

                buttonSave.BackColor = Form_Main._backColor;
                buttonSave.ForeColor = Form_Main._foreColor;
                buttonSave.FlatStyle = FlatStyle.Flat;
                buttonSave.FlatAppearance.BorderColor = Form_Main._textColor;
                buttonSave.FlatAppearance.BorderSize = 1;
            }

            Wallets.Wallets.InitCoinsList();
            comboBox_Coin.Items.Clear();
            foreach (var coin in Wallets.Wallets.CoinsList)
            {
                comboBox_Coin.Items.Add(coin.Coin + " - " + coin.Name);
            }
            comboBox_Coin.SelectedItem = "BTC - Bitcoin";
            textBox_worker.Text = "ForkFixWorker";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string coin = comboBox_Coin.Text.Substring(0, comboBox_Coin.Text.IndexOf(" -")).Trim();
            double treshold = 0;
            double.TryParse(textBox_PayoutThreshold.Text, out treshold);
            _walletsListView1.AddWallet(coin, treshold, textBox_Wallet.Text, textBox_worker.Text);
            this.Close();
        }

        private void comboBox_Coin_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_PayoutThreshold.Text = Wallets.Wallets.CoinsList[comboBox_Coin.SelectedIndex].Treshold.ToString();
        }

        private void comboBox_Coin_DrawItem(object sender, DrawItemEventArgs e)
        {
            comboBox_DrawItem(sender, e);
        }
        public void comboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cmb = (ComboBox)sender;
            if (cmb == null) return;

            e.DrawBackground();

            // change background color
            var bc = new SolidBrush(Form_Main._backColor);
            var fc = new SolidBrush(Form_Main._foreColor);
            var wc = new SolidBrush(Form_Main._windowColor);
            var gr = new SolidBrush(Color.Gray);
            var red = new SolidBrush(Color.Red);
            e.Graphics.FillRectangle(bc, e.Bounds);
            //e.Graphics.FillRectangle(((e.State & DrawItemState.Selected) > 0) ? red : bc, e.Bounds);

            // change foreground color
            Brush brush = ((e.State & DrawItemState.Selected) > 0) ? fc : gr;
            //brush = ((e.State & DrawItemState.Focus) > 0) ? gr : fc;

            if (e.Index >= 0)
            {
                e.Graphics.DrawString(cmb.Items[e.Index].ToString(), cmb.Font, brush, e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://zergpool.com/payouts");
        }
    }
}
