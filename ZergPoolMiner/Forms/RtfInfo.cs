using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZergPoolMiner.Forms
{
    class RtfInfo
    {
        public static void ShowRtf()
        {
            // Create and show the additional window with RTF content
            Form rtfWindow = new Form();
            rtfWindow.Size = new Size(800, 600);
            rtfWindow.Text = "RTF Content";
            rtfWindow.Show();

            // Load RTF file into RichTextBox
            LoadRtfFile(rtfWindow);
        }

        private static void LoadRtfFile(Form rtfWindow)
        {
            string rtfContent;
            using (StreamReader reader = new StreamReader("Help\\InfoEN.rtf", Encoding.UTF8))
            {
                rtfContent = reader.ReadToEnd();
            }

            // Create RichTextBox
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Location = new Point(10, 10);
            richTextBox.Text = rtfContent;
            rtfWindow.Controls.Add(richTextBox);

            rtfWindow.Show(rtfWindow); // Display the window centered over mainForm
        }
    }
}
