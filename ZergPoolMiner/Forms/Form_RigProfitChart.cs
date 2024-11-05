using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners;
using ZergPoolMiner.Stats;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Timer = System.Windows.Forms.Timer;
using ZergPoolMiner.Algorithms;

namespace ZergPoolMiner.Forms
{
    public partial class Form_RigProfitChart : Form
    {
        public System.Windows.Forms.Timer _RigProfitChartTimer;
        public int ProfitsCount = 0;
        public int LocalProfitsCount = 0;
        public double totalRateAll = 0d;
        public double totalRateLocal = 0d;
        public double currentProfitAllAPI = 0d;
        public double totalRate3 = 0d;
        public double currentProfit3 = 0d;
        public double totalRate12 = 0d;
        public double currentProfit12 = 0d;
        public double totalRate24 = 0d;
        public double currentProfit24 = 0d;
        public static bool FormChartMoved = false;
        private string CurrencyName = "";
        private double totalActualProfit;
        private double totalLocalProfit;//local prof
        public Form_RigProfitChart()
        {
            InitializeComponent();
            chartRigProfit.Hide();
            InitializeColorProfile();
            progressBar1.Visible = false;
            Text = International.GetText("Form_Main_chart");
            checkBox_StartChartWithProgram.Checked = ConfigManager.GeneralConfig.StartChartWithProgram;
            checkBox_Chart_Fiat.Checked = ConfigManager.GeneralConfig.ChartFiat;
            buttonClear.Text = "";
            //labelChartDisabled.Text = International.GetText("Form_Profit_Chart_Disabled");
            labelChartDisabled.Visible = true;
            
            if (Form_Main.ChartDataAvail > 0)
            {
                buttonClear.BackgroundImage = Properties.Resources.recycle2;
            }
            else
            {
                buttonClear.BackgroundImage = Properties.Resources.recycle1;
            }
        }
        public void InitializeColorProfile()
        {
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                this.BackColor = Form_Main._backColor;
                this.ForeColor = Form_Main._foreColor;
                checkBox_StartChartWithProgram.FlatStyle = FlatStyle.Flat;
                checkBox_StartChartWithProgram.BackColor = Form_Main._backColor;
                checkBox_StartChartWithProgram.ForeColor = Form_Main._textColor;

                checkBox_Chart_Fiat.FlatStyle = FlatStyle.Flat;
                checkBox_Chart_Fiat.BackColor = Form_Main._backColor;
                checkBox_Chart_Fiat.ForeColor = Form_Main._textColor;

                chartRigProfit.BackColor = Form_Main._backColor;
                chartRigProfit.ForeColor = Form_Main._textColor;

                chartRigProfit.Legends["Legend1"].BackColor = Form_Main._backColor;
                chartRigProfit.Legends["Legend1"].ForeColor = Form_Main._textColor;

                chartRigProfit.ChartAreas[0].BackColor = Form_Main._backColor;
                //chartRigProfit.ChartAreas[0].fo = Form_Main._textColor;
                progressBar1.BackColor = Form_Main._backColor;
                progressBar1.ForeColor = Form_Main._foreColor;

            }
            else
            {

            }
        }
        void ChartData(object sender, EventArgs e)
        {
            try
            {
                string adaptiveVarning = "";
                /*
                foreach (var a in Stats.Stats.algosProperty)
                {
                    if (Form_Main.adaptiveRunning)
                    {
                        adaptiveVarning = adaptiveVarning + "Adaptive tuning in progress " + a.Key +
                            ". Remaining time " +
                            (ConfigManager.GeneralConfig.ticksAdaptiveTuning +
                            ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart - 
                            a.Value.ticks).ToString() + " min\r\n";
                    }
                }
                */
                labelChartDisabled.Text = adaptiveVarning;

                totalRateLocal = totalRateLocal + MinersManager.GetTotalRate();
                LocalProfitsCount++;
                if (FormChartMoved || Form_Main.FormMainMoved || Form_Settings.FormSettingsMoved || Form_Benchmark.FormBenchmarkMoved) return;

                if (Form_Main.RigProfits.Count <= ProfitsCount)
                {
                    return;
                }
                if (Form_Main.RigProfits.Count == 0)
                {
                    return;
                }

                Form_Main.RigProfitList lastRigProfit = new Form_Main.RigProfitList();
                lastRigProfit.currentPower = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].currentPower;
                lastRigProfit.currentProfit = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].currentProfit;
                lastRigProfit.currentProfitAPI = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].currentProfitAPI;
                lastRigProfit.DateTime = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].DateTime;
                lastRigProfit.totalPowerRate = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].totalPowerRate;
                //lastRigProfit.totalRate = totalRateLocal / LocalProfitsCount;//усредняем за прошедшую минуту
                lastRigProfit.totalRate = MinersManager.GetTotalRate();
                lastRigProfit.unpaidAmount = Form_Main.RigProfits[Form_Main.RigProfits.Count - 1].unpaidAmount;

                Form_Main.RigProfits[Form_Main.RigProfits.Count - 1] = lastRigProfit;
                LocalProfitsCount = 0;
                totalRateLocal = 0;

                if (ConfigManager.GeneralConfig.ChartFiat)
                {
                    chartRigProfit.Series["Series3"].Points.AddXY(ProfitsCount, 
                        Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].totalPowerRate *
                        Form_Main._factorTimeUnit) / 1), 2));
                    chartRigProfit.Series["Series2"].Points.AddXY(ProfitsCount, 
                        Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].totalRate * 
                        Form_Main._factorTimeUnit) / 1), 2));
                    chartRigProfit.Series["Series1"].Points.AddXY(ProfitsCount,
                        Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].currentProfitAPI *
                        Form_Main._factorTimeUnit) / 1), 2));

                }
                else
                {
                    chartRigProfit.Series["Series3"].Points.AddXY(ProfitsCount, 
                        Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].totalPowerRate * 
                        Form_Main._factorTimeUnit), 8));
                    chartRigProfit.Series["Series2"].Points.AddXY(ProfitsCount, 
                        Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].totalRate * 
                        Form_Main._factorTimeUnit), 8));
                    chartRigProfit.Series["Series1"].Points.AddXY(ProfitsCount, 
                        Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].currentProfitAPI * 
                        Form_Main._factorTimeUnit), 8));
                }

                chartRigProfit.ChartAreas[0].AxisX.Maximum = ProfitsCount;
                if (ProfitsCount > 60 * 24)
                {
                    chartRigProfit.ChartAreas[0].AxisX.ScaleView.Size = 60 * 24;//размер скрола
                    chartRigProfit.ChartAreas[0].AxisX.ScaleView.Scroll(ProfitsCount);//скролл
                }

                int h = ProfitsCount / 60;
                if (ProfitsCount / 60 == Math.Truncate((double)(ProfitsCount / 60)))
                {
                    if (Form_Main.lastRigProfit.DateTime.Hour == 0)
                    {
                        chartRigProfit.Series[0].Points[ProfitsCount].AxisLabel = Form_Main.RigProfits[0].DateTime.AddHours((double)h).ToString("dd-MM-yyyy HH:mm:ss");
                    }
                    else
                    {
                        chartRigProfit.Series[0].Points[ProfitsCount].AxisLabel = Form_Main.RigProfits[0].DateTime.AddHours((double)h).ToString("HH:mm:ss");
                    }
                }

                totalRateAll = 0;
                currentProfitAllAPI = 0;

                for (int i = 0; i < Form_Main.RigProfits.Count; i++)
                {
                    totalRateAll = totalRateAll + Form_Main.RigProfits[i].totalRate;
                    currentProfitAllAPI = currentProfitAllAPI + Form_Main.RigProfits[i].currentProfitAPI;
                }

                //ce = currentProfitAllAPI / (Form_Main.RigProfits.Count);
                totalActualProfit = Form_Main.TotalActualBTC; ;
                if (double.IsInfinity(totalActualProfit)) totalActualProfit = 0;
                if (double.IsNaN(totalActualProfit)) totalActualProfit = 0;

                //cel = totalRateAll / (Form_Main.RigProfits.Count);
                totalLocalProfit = Form_Main.TotalBTC;
                if (double.IsInfinity(totalLocalProfit)) totalLocalProfit = 0;
                if (double.IsNaN(totalLocalProfit)) totalLocalProfit = 0;
                string ces = "";
                string cesl = "";

                var cpAPI = totalActualProfit / totalLocalProfit;
                if (double.IsInfinity(cpAPI)) cpAPI = 0;
                if (double.IsNaN(cpAPI)) cpAPI = 0;


                if (ConfigManager.GeneralConfig.ChartFiat)
                {
                    CurrencyName = ConfigManager.GeneralConfig.DisplayCurrency;
                    ces = (ExchangeRateApi.ConvertBTCToNationalCurrency(totalActualProfit *
                        Form_Main._factorTimeUnit / 1)).ToString("F2");
                    cesl = (ExchangeRateApi.ConvertBTCToNationalCurrency(totalLocalProfit *
                        Form_Main._factorTimeUnit / 1)).ToString("F2");
                    chartRigProfit.Series["Series1"].LegendText = International.GetText("Form_Main_current_actual_profitabilities") +
                        ": " + Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].currentProfitAPI *
                        Form_Main._factorTimeUnit) / 1), 2).ToString() + " " + CurrencyName;
                    chartRigProfit.Series["Series2"].LegendText = International.GetText("Form_Main_current_local_profitabilities") +
                        ": " + Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].totalRate *
                        Form_Main._factorTimeUnit) / 1), 2).ToString() + " " + CurrencyName;
                    chartRigProfit.Series["Series3"].LegendText = International.GetText("Form_Main_current_power") +
                        ": " + Math.Round((ExchangeRateApi.ConvertBTCToNationalCurrency(Form_Main.RigProfits[ProfitsCount].totalPowerRate *
                        Form_Main._factorTimeUnit)), 2).ToString() + " " + CurrencyName;
                }
                else
                {
                    CurrencyName = ConfigManager.GeneralConfig.PayoutCurrency;
                    ces = ExchangeRateApi.ConvertBTCToPayoutCurrency(totalActualProfit).ToString("F9");
                    cesl = ExchangeRateApi.ConvertBTCToPayoutCurrency(totalLocalProfit).ToString("F9");
                    chartRigProfit.Series["Series1"].LegendText = International.GetText("Form_Main_current_actual_profitabilities") +
                        ": " + Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].currentProfitAPI * 
                        Form_Main._factorTimeUnit), 8) + " " + CurrencyName;
                    chartRigProfit.Series["Series2"].LegendText = International.GetText("Form_Main_current_local_profitabilities") +
                        ": " + Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].totalRate *
                        Form_Main._factorTimeUnit), 8) + " " + CurrencyName;
                    chartRigProfit.Series["Series3"].LegendText = International.GetText("Form_Main_current_power") +
                        ": " + Math.Round(ExchangeRateApi.ConvertBTCToPayoutCurrency(Form_Main.RigProfits[ProfitsCount].totalPowerRate *
                        Form_Main._factorTimeUnit), 8) + " " + CurrencyName;
                }

                
                if (currentProfitAllAPI == 0 || totalRateAll == 0)
                {
                    label_totalEfficiency.Text = International.GetText("Form_Profit_Total_efficiency") + " --";
                    label_Total_actual_profitabilities.Text = International.GetText("Form_Profit_Total_actual_profitabilities") + " --";
                    label_Total_local_profit.Text = International.GetText("Form_Profit_Total_local_profitabilities") + " --";
                }
                if (currentProfitAllAPI != 0 && totalRateAll != 0)
                {
                    label_totalEfficiency.Text = International.GetText("Form_Profit_Total_efficiency") + " " +
                        Math.Round((cpAPI * 100), 1).ToString() + "%";
                }
                if (totalActualProfit != 0)
                {
                    label_Total_actual_profitabilities.Text = International.GetText("Form_Profit_Total_actual_profitabilities") + " " + ces + " " + CurrencyName;
                }
                if (totalLocalProfit != 0)
                {
                    label_Total_local_profit.Text = International.GetText("Form_Profit_Total_local_profitabilities") + " " + cesl + " " + CurrencyName;
                }
                //chartRigProfit.Update();
                //GC.Collect();
                if (Form_Main.ChartDataAvail > 0)
                {
                    buttonClear.BackgroundImage = Properties.Resources.recycle2;
                }
                else
                {
                    buttonClear.BackgroundImage = Properties.Resources.recycle1;
                }
                ProfitsCount++;
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("ChartData", ex.ToString());
            }
        }

        private void chartRigProfit_Click(object sender, EventArgs e)
        {
            /*
            chartRigProfit.ChartAreas[0].InnerPlotPosition.Width = chartRigProfit.ChartAreas[0].InnerPlotPosition.Width * ((float)2.0);
            chartRigProfit.Invalidate();
            */
        }

        private Timer _updateTimer;
        private void Form_RigProfitChart_Shown(object sender, EventArgs e)
        {
            if (this != null)
            {
                Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                if (ConfigManager.GeneralConfig.ProfitFormLeft + ConfigManager.GeneralConfig.ProfitFormWidth <= screenSize.Size.Width)
                {
                    if (ConfigManager.GeneralConfig.ProfitFormTop + ConfigManager.GeneralConfig.ProfitFormLeft >= 1)
                    {
                        this.Top = ConfigManager.GeneralConfig.ProfitFormTop;
                        this.Left = ConfigManager.GeneralConfig.ProfitFormLeft;
                    }

                    this.Width = ConfigManager.GeneralConfig.ProfitFormWidth;
                    this.Height = ConfigManager.GeneralConfig.ProfitFormHeight;
                }
                else
                {
                    // this.Width = 660; // min width
                }
            }

            if (ConfigManager.GeneralConfig.ChartFiat)
            {
                CurrencyName = $"{ConfigManager.GeneralConfig.DisplayCurrency}/" + International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            }
            else
            {
                CurrencyName = ConfigManager.GeneralConfig.PayoutCurrency + " /" + International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            }

            //chartRigProfit.Legends.Add(new Legend("Legend2"));

            chartRigProfit.Legends["Legend1"].Docking = Docking.Top;
            chartRigProfit.Legends["Legend1"].Alignment = StringAlignment.Center;
            //chartRigProfit.Legends["Legend2"].DockedToChartArea = "ChartArea1";
            chartRigProfit.Legends["Legend1"].LegendStyle = LegendStyle.Row;
            chartRigProfit.Legends["Legend1"].TextWrapThreshold = 100;

            chartRigProfit.Series["Series2"].LegendText = International.GetText("Form_Main_current_local_profitabilities");
            chartRigProfit.Series["Series1"].BorderWidth = 2;
            chartRigProfit.Series["Series2"].BorderWidth = 2;
            chartRigProfit.Series["Series1"].LegendText = International.GetText("Form_Main_current_actual_profitabilities");
            chartRigProfit.Series["Series3"].LegendText = International.GetText("Form_Main_current_power");
            chartRigProfit.Series["Series3"].BorderWidth = 2;
            //chartRigProfit.Series["Series3"].LegendText = "API";
            //chartRigProfit.Series["Series3"].BorderWidth = 1;
            //chartRigProfit.Series["Series3"].ChartType = SeriesChartType.Line;
            chartRigProfit.Series["Series2"].ChartType = SeriesChartType.Spline;
            chartRigProfit.Series["Series1"].ChartType = SeriesChartType.Spline;
            chartRigProfit.Series["Series2"].Color = Color.Orange;
            chartRigProfit.Series["Series1"].Color = Color.Green;
            chartRigProfit.Series["Series3"].ChartType = SeriesChartType.Spline;
            chartRigProfit.Series["Series3"].Color = Color.Blue;

            //chartRigProfit.Series["Series1"].SetCustomProperty("LineTension", "0.1");
            //chartRigProfit.Series["Series2"].SetCustomProperty("LineTension", "0.1");
            //chartRigProfit.Series["Series3"].SetCustomProperty("LineTension", "0.1");
            //chartRigProfit.Series["Series1"].SetCustomProperty("LineTension", "0.9");//0.8 by default
            //chartRigProfit.Series["Series3"].Color = Color.Aqua;

            chartRigProfit.ChartAreas[0].AxisX.Minimum = 0;
            chartRigProfit.ChartAreas[0].AxisY.Minimum = 0;
            //chartRigProfit.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartRigProfit.ChartAreas[0].AxisX.Interval = 60;//
            chartRigProfit.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartRigProfit.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartRigProfit.ChartAreas[0].AxisY.Title = CurrencyName;
            //chartRigProfit.ChartAreas[0].AxisX.Title = International.GetText("Form_Main_Uptime");

            chartRigProfit.AntiAliasing = AntiAliasingStyles.All;

            chartRigProfit.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;//скрол над/под цифрами
            chartRigProfit.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;

            checkBox_StartChartWithProgram.Text = International.GetText("Form_Profit_StartChartWithProgram");
            checkBox_Chart_Fiat.Text = International.GetText("Form_Profit_Chart_Fiat");

            _updateTimer = new Timer();
            _updateTimer.Tick += ChartData;
            _updateTimer.Interval = 1000 * 1;
            _updateTimer.Start();

            progressBar1.Visible = true;
            progressBar1.Maximum = Form_Main.RigProfits.Count - 1;
            for (int i = 0; i < Form_Main.RigProfits.Count; i++)
            {
                progressBar1.Value = i;
                progressBar1.Update();
                this.Update();
                //Thread.Sleep(1);
                ChartData(null, null);
                chartRigProfit.Series[0].Points[0].AxisLabel = Form_Main.RigProfits[0].DateTime.ToString("dd-MM-yyyy HH:mm:ss");
            }
            progressBar1.Visible = false;

            if (Form_Main.RigProfits.Count != 0)
            {
                ChartData(null, null);
                chartRigProfit.Series[0].Points[0].AxisLabel = Form_Main.RigProfits[0].DateTime.ToString("dd-MM-yyyy HH:mm:ss");
            }

            chartRigProfit.Show();
        }

        private void Form_RigProfitChart_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Dispose();
            }
            if (this != null)
            {
                if (ConfigManager.GeneralConfig.Save_windows_size_and_position)
                {
                    ConfigManager.GeneralConfig.ProfitFormWidth = this.Width;
                    ConfigManager.GeneralConfig.ProfitFormHeight = this.Height;
                    if (this.Top + this.Left >= 1)
                    {
                        ConfigManager.GeneralConfig.ProfitFormTop = this.Top;
                        ConfigManager.GeneralConfig.ProfitFormLeft = this.Left;
                    }
                }
            }
            ConfigManager.GeneralConfigFileCommit();
            Form_Main.Form_RigProfitChartRunning = false;
        }

        private bool isLeftButtonPressed = false;
        private Point mouseDown = Point.Empty;
        private void chartRigProfit_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void chartRigProfit_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void chartRigProfit_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void checkBox_StartChartWithProgram_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.StartChartWithProgram = checkBox_StartChartWithProgram.Checked;
        }

        private void Form_RigProfitChart_SizeChanged(object sender, EventArgs e)
        {
            if (this != null)
            {
                if (ConfigManager.GeneralConfig.Save_windows_size_and_position)
                {
                    ConfigManager.GeneralConfig.ProfitFormWidth = this.Width;
                    ConfigManager.GeneralConfig.ProfitFormHeight = this.Height;
                    if (this.Top + this.Left >= 1)
                    {
                        ConfigManager.GeneralConfig.ProfitFormTop = this.Top;
                        ConfigManager.GeneralConfig.ProfitFormLeft = this.Left;
                    }
                }
            }
            ConfigManager.GeneralConfigFileCommit();
        }

        private void Form_RigProfitChart_Deactivate(object sender, EventArgs e)
        {
            if (this != null)
            {
                if (ConfigManager.GeneralConfig.Save_windows_size_and_position)
                {
                    ConfigManager.GeneralConfig.ProfitFormWidth = this.Width;
                    ConfigManager.GeneralConfig.ProfitFormHeight = this.Height;
                    if (this.Top + this.Left >= 1)
                    {
                        ConfigManager.GeneralConfig.ProfitFormTop = this.Top;
                        ConfigManager.GeneralConfig.ProfitFormLeft = this.Left;
                    }
                }
            }
            ConfigManager.GeneralConfigFileCommit();
        }

        private void Form_RigProfitChart_ResizeBegin(object sender, EventArgs e)
        {
            FormChartMoved = true;
        }

        private void Form_RigProfitChart_ResizeEnd(object sender, EventArgs e)
        {
            FormChartMoved = false;
        }

        private void checkBox_Chart_Fiat_CheckedChanged(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.ChartFiat != checkBox_Chart_Fiat.Checked)
            {
                ConfigManager.GeneralConfig.ChartFiat = checkBox_Chart_Fiat.Checked;
                this.Close();
                Thread.Sleep(100);
                var chart = new Form_RigProfitChart();
                try
                {
                    chart.Show();
                }
                catch (Exception er)
                {
                    Helpers.ConsolePrint("chart", er.ToString());
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string curdir = Environment.CurrentDirectory;
            string filename = curdir + "\\Chart\\Chart_" + DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss") + ".jpg";
            try
            {
                if (!Directory.Exists("Chart")) Directory.CreateDirectory("Chart");
                this.chartRigProfit.SaveImage(filename, ChartImageFormat.Jpeg);
                MessageBox.Show(International.GetText("Form_Profit_Chart_Filename") + " " + filename,
            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("ChartSave", er.ToString());
            }
        }

        private void label_totalEfficiency_Click(object sender, EventArgs e)
        {

        }

        private void label_Total_actual_profitabilities_Click(object sender, EventArgs e)
        {

        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(International.GetText("Form_Profit_Chart_Delete"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.No) return;

            Form_Main.RigProfits.Clear();
            Form_Main.ChartDataAvail = 0;
            chartRigProfit.Series["Series1"].Points.Clear();
            chartRigProfit.Series["Series2"].Points.Clear();
            chartRigProfit.Series["Series3"].Points.Clear();
            buttonClear.BackgroundImage = Properties.Resources.recycle1;

            //Form_Main.lastRigProfit.currentProfitAPI = 0;
            //Form_Main.lastRigProfit.totalRate = 0;
            Form_Main.RigProfits.Add(Form_Main.lastRigProfit);
            ChartData(null, null);
            Form_Main.TotalActualProfitability = 0;

            this.Close();
            Thread.Sleep(100);
            var chart = new Form_RigProfitChart();
            try
            {
                chart.Show();
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("chart", er.ToString());
            }
        }

        private void Form_RigProfitChart_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Main.Form_RigProfitChartRunning = false;
        }
    }
}
