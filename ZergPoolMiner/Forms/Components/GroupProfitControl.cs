using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;

namespace ZergPoolMiner.Forms.Components
{
    public partial class GroupProfitControl : UserControl
    {
        public GroupProfitControl()
        {
            InitializeComponent();

            toolTip2.SetToolTip(button_restart, International.GetText("Form_Main_RestartMiner"));
            toolTip2.AutomaticDelay = 500;
            toolTip2.AutoPopDelay = 2000;
            groupBoxMinerGroup.ForeColor = Form_Main._foreColor;
            groupBoxMinerGroup.BackColor = Form_Main._backColor;
            groupBoxMinerGroup.DoubleBuffer();
        }

        private Bitmap _stopBtn = Properties.Resources.Pause;
        private Bitmap _stopDisabledBtn = Properties.Resources.Pause_Disabled;
        private Bitmap _runBtn = Properties.Resources.Play;
        private Bitmap _runDisabledBtn = Properties.Resources.Play_Disabled;
        private Font fontRegular = new Font(DefaultFont, FontStyle.Regular);
        private Font fontBold = new Font(DefaultFont, FontStyle.Bold);

        public void UpdateProfitStats(string algoName, string coin, string groupName, string deviceStringInfo,
            string speedString, DateTime StartMinerTime, string btcRateString, string currencyRateString, 
            string ProcessTag, bool paused, bool overheating)
        {
            string effort = "";
            string tooshort = "";
            if (ConfigManager.GeneralConfig.ShowEffort)
            {
                var c = Stats.Stats.CoinList.Find(e => e.symbol.Equals(coin));
                if (c is object && c != null)
                {
                    if (c.effort < 10000)
                    {
                        if (c.real_ttf < 6 * 60) tooshort = " Too short";
                        effort = "  (Effort " + c.effort.ToString() + "%, average " + 
                            c.average_effort.ToString() + "%)" + tooshort;
                    }
                    else
                    {
                        effort = "  (API bug?)";
                    }
                }
            }
            try
            {
                var timenow = DateTime.Now;
                TimeSpan Uptime = timenow.Subtract(StartMinerTime);
                if (ConfigManager.GeneralConfig.ShowUptime)
                {
                    groupBoxMinerGroup.Text = string.Format(International.GetText("Form_Main_MiningDevices"), deviceStringInfo) +
                        "  " + International.GetText("Form_Main_Miner") + 
                        MinerVersion.GetMinerFakeVersion(groupName.Split('-')[0], algoName) +
                        "  " + International.GetText("Form_Main_Uptime") + " " + Uptime.ToString(@"d\ \d\a\y\s\ hh\:mm\:ss") +
                        effort;
                }
                else
                {
                    groupBoxMinerGroup.Text = string.Format(International.GetText("Form_Main_MiningDevices"), deviceStringInfo) +
                        "  " + International.GetText("Form_Main_Miner") + 
                        MinerVersion.GetMinerFakeVersion(groupName.Split('-')[0], algoName) +
                        "  " + International.GetText("Form_Main_Uptime") + 
                        effort;
                }

                if (button_pause.Image != _runBtn && button_pause.Image != _runDisabledBtn)
                {
                    toolTip2.SetToolTip(button_pause, International.GetText("Form_Main_SuspendMiner"));
                    labelBTCRateIndicator.Font = fontRegular;
                    labelBTCRateIndicator.ForeColor = Form_Main._foreColor;
                    if (ConfigManager.GeneralConfig.FiatCurrency)
                    {
                        labelBTCRateIndicator.Text = International.GetText("Rate") + "  " +
                        btcRateString + "         " + currencyRateString;
                    }
                    else
                    {
                        labelBTCRateIndicator.Text = International.GetText("Rate") + "  " +
                        btcRateString;
                    }
                } else
                {
                    toolTip2.SetToolTip(button_pause, International.GetText("Form_Main_ResumeMiner"));
                }

                if (button_pause.Image != _stopBtn && button_pause.Image != _runBtn &&
                    button_pause.Image != _stopDisabledBtn && button_pause.Image != _runDisabledBtn)
                {
                    button_pause.Image = _stopDisabledBtn;
                }
                
                if (button_pause.Image == _runBtn && button_restart.Enabled)
                {
                    labelBTCRateIndicator.Font = fontBold;
                    labelBTCRateIndicator.ForeColor = Form_Main._foreColor;
                    labelBTCRateIndicator.Text = International.GetText("Form_Main_SuspendedByUser") + "        ";
                }
                
                if (overheating && button_restart.Enabled)
                {
                    labelBTCRateIndicator.Font = fontBold;
                    labelBTCRateIndicator.ForeColor = Color.Red;
                    labelBTCRateIndicator.Text = International.GetText("Form_Main_SuspendedOverheating") + "        ";
                }
                button_restart.Enabled = true;

                button_pause.ForeColor = Form_Main._backColor;
                button_pause.UseVisualStyleBackColor = false;
                button_pause.FlatAppearance.BorderSize = 0;

                if (paused || overheating)
                {
                    button_pause.Enabled = false;
                    button_pause.Update();
                }
                else
                {
                    button_pause.Enabled = true;
                    button_pause.Update();
                }

                
                richTextBoxSpeedValue.Text = speedString;
                richTextBoxSpeedValue.SelectionStart = 0;
                richTextBoxSpeedValue.SelectionLength = richTextBoxSpeedValue.Text.Length;
                richTextBoxSpeedValue.BackColor = Form_Main._backColor;
                richTextBoxEffort.BackColor = Form_Main._backColor;

                /*
                if (ConfigManager.GeneralConfig.ShowEffort)
                {
                    richTextBoxEffort.Visible = true;
                    //richTextBoxEffort.Left = (int)(richTextBoxSpeedValue.Text.Length * richTextBoxSpeedValue.Font.Height / 1.8);
                    richTextBoxEffort.Left = (int)(speedString.Length * richTextBoxSpeedValue.Font.Height / 1.8);
                    richTextBoxEffort.Text = effort;
                    richTextBoxEffort.SelectionStart = 0;
                    richTextBoxEffort.SelectionLength = richTextBoxEffort.Text.Length;
                } else
                {
                    richTextBoxEffort.Visible = false;
                }
                */
                button_restart.Tag = ProcessTag;
                button_pause.Tag = ProcessTag;
                button_restart.Refresh();
            }
            catch (Exception ex)
            {

            }
        }


        private void button_restart_MouseLeave(object sender, System.EventArgs e)
        {
            button_restart.Image = Properties.Resources.Refresh_disabled;
        }

        private void button_restart_MouseMove(object sender, MouseEventArgs e)
        {
            button_restart.FlatAppearance.MouseOverBackColor = Form_Main._backColor;
            button_restart.Image = Properties.Resources.Refresh_hot;
        }

        private void button_pause_MouseLeave(object sender, EventArgs e)
        {
            if (button_pause.Image.Equals(_stopBtn))
            {
                button_pause.Image = _stopDisabledBtn;
                button_pause.Update();
                return;
            }
            if (button_pause.Image.Equals(_runBtn))
            {
                button_pause.Image = _runDisabledBtn;
                button_pause.Update();
                return;
            }
        }

        private void button_pause_MouseMove(object sender, MouseEventArgs e)
        {
            button_pause.FlatAppearance.MouseOverBackColor = Form_Main._backColor;
            if (button_pause.Image.Equals(_stopDisabledBtn))
            {
                button_pause.Image = _stopBtn;
                button_pause.Update();
                return;
            }
            if (button_pause.Image.Equals(_runDisabledBtn))
            {
                button_pause.Image = _runBtn;
                button_pause.Update();
                return;
            }
        }

        private void labelBTCRateIndicator_Click(object sender, System.EventArgs e)
        {

        }

        private void buttonBTC_restart(object sender, System.EventArgs e)
        {
            Helpers.ConsolePrint("ZergPoolMiner", "Restarting miner by user: " + button_restart.Tag.ToString());
            MiningSession.RestartMiner(button_restart.Tag.ToString());
            button_pause.Enabled = false;
            button_restart.Enabled = false;
            button_restart.Update();
        }
        
        private void button_pause_Click(object sender, EventArgs e)
        {
            if (button_pause.Image.Equals(_stopBtn))
            {
                Helpers.ConsolePrint("ZergPoolMiner", "Suspend miner by user: " + button_pause.Tag.ToString());
                MiningSession.SuspendResumeMiner(button_pause.Tag.ToString(), true);
                toolTip2.SetToolTip(button_pause, International.GetText("Form_Main_SuspendMiner"));
                labelBTCRateIndicator.Font = fontBold;
                labelBTCRateIndicator.ForeColor = Form_Main._foreColor;
                labelBTCRateIndicator.Text = International.GetText("Form_Main_SuspendedByUser") + "        ";

                button_pause.Image = _runBtn;
                button_pause.Update();
                return;
            }
            if (button_pause.Image.Equals(_runBtn))
            {
                Helpers.ConsolePrint("ZergPoolMiner", "Resume miner by user: " + button_pause.Tag.ToString());
                MiningSession.SuspendResumeMiner(button_pause.Tag.ToString(), false);
                toolTip2.SetToolTip(button_pause, International.GetText("Form_Main_ResumeMiner"));
                button_pause.Image = _stopBtn;
                button_pause.Update();
                return;
            }
        }

        private void richTextBoxSpeedValue_Enter(object sender, EventArgs e)
        {
            ActiveControl = groupBoxMinerGroup;
        }

        private void groupBoxMinerGroup_Paint(object sender, PaintEventArgs e)
        {
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 14) return;
            GroupBox box = sender as GroupBox;
            Form_Main.DrawGroupBox(box, e.Graphics, Form_Main._foreColor, Form_Main._grey);
        }
    }
    
}
