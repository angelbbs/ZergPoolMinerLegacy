using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZergPoolMiner.Configs;
using ZergPoolMinerLegacy.Common.Enums;

namespace ZergPoolMiner.Forms
{
    public partial class FormRtf : Form
    {
        public FormRtf()
        {
            InitializeComponent();
            try
            {
                if (ConfigManager.GeneralConfig.Language == LanguageType.Ru)
                {
                    richTextBox1.LoadFile("Help\\InfoRU.rtf");
                } else
                {
                    richTextBox1.LoadFile("Help\\InfoEN.rtf");
                }
                richTextBox1.ReadOnly = true;
                richTextBox1.Show();
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("FormRtf", ex.ToString());
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
