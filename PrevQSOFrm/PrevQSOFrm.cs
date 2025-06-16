using DXLogCalculators;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DXLog.net
{
    public partial class PrevQSOForm : KForm
    {
        private readonly ContestData _cdata = null;
        private Font _windowFont = new Font("Courier New", 10, FontStyle.Bold);
        private FrmMain mainForm = null;
        private delegate void newQsoSaved(DXQSO qso);
        private delegate void CallsignInfoRefreshed(string property);
        private bool IsUpdate = false;
        private string LastProperty = string.Empty;

        public static string CusWinName
        {
            get { return "PrevQSOForm"; }
        }

        public static int CusFormID
        {
            get { return 843201; }
        }

        public PrevQSOForm()
        {
            InitializeComponent();
        }

        public PrevQSOForm(ContestData cdata)
        {
            InitializeComponent();
            ColorSetTypes = new string[] { "Background", "Color", "Header back color", "Header color", "Footer back color", "Footer color", "Final score color", "Selection back color", "Selection color" };
            DefaultColors = new Color[] { Color.Turquoise, Color.Black, Color.Gray, Color.Black, Color.Silver, Color.Black, Color.Blue, Color.SteelBlue, Color.White };
            FormLayoutChangeEvent += new FormLayoutChange(Handle_FormLayoutChangeEvent);
            _cdata = cdata;
        }

        private void Handle_FormLayoutChangeEvent()
        {
            InitializeLayout();
        }

        public override void InitializeLayout()
        {
            InitializeLayout(_windowFont);
            if (FormLayout.FontName.Contains("Courier"))
            {
                _windowFont = new Font(FormLayout.FontName, FormLayout.FontSize, FontStyle.Bold);
            }
            else
            {
                _windowFont = Helper.GetSpecialFont(FontStyle.Bold, FormLayout.FontSize);
            }

            if (mainForm == null)
            {
                if (ParentForm == null)
                {
                    mainForm = (FrmMain)(Owner);
                }
                else
                {
                    mainForm = (FrmMain)(ParentForm);
                }
                if (mainForm != null)
                {
                    mainForm.CallsignInfoRefreshed += new FrmMain.CallsignInfoRefreshDelegate(HandleCallsignInfoRefreshed);
                    mainForm.NewQSOSaved += new FrmMain.NewQSOSavedEvent(HandleNewQSOSaved);
                }
            }
            else
            {
                this.Font = _windowFont;
                this.Width = FormLayout.Width;
                this.Height = FormLayout.Height;
                this.Location = new Point(FormLayout.LocX, FormLayout.LocY);
                listView1.BackColor = mainForm.BackColor; // mainForm.panelQSO.BackColor;
                listView1.Columns[0].TextAlign = HorizontalAlignment.Right;
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }
        private void HandleCallsignInfoRefreshed(string property)
        {
            if (this.InvokeRequired)
            {
                CallsignInfoRefreshed d = new CallsignInfoRefreshed(HandleCallsignInfoRefreshed);
                this.Invoke(d, new object[] { property });
                return;
            }

            if (LastProperty == property) { return; }
            if (IsUpdate) { return; }
            LastProperty = property;
            listView1.Items.Clear();

            // минимальное кол-во символов после которого начинаем искать
            // если лог большой, >2000 - ищем при 3-х символах
            int v = 1; // Math.Min(1, _cdata.QSOList.Count / 1000);
            if (LastProperty.Length > v)
            {
                IsUpdate = true;
                listView1.BeginUpdate();
                string lastRcvdExch = string.Empty;
                for (int i = 0; i < _cdata.QSOList.Count; i++)
                {
                    if (_cdata.QSOList[i].Callsign.Contains(LastProperty))
                    {
                        ListViewItem lvi = new ListViewItem();
                        // ? смена диапазона
                        if (_cdata.QSOList[i].BCRuleBroken)
                        {
                            lvi.Font = new Font(FormLayout.FontName, FormLayout.FontSize, FontStyle.Strikeout);
                        }
                        lvi.Tag = _cdata.QSOList[i].IDQSO;
                        lvi.SubItems.Add(_cdata.QSOList[i].IDQSO.ToString());
                        lvi.SubItems.Add(_cdata.QSOList[i].Frequency_kHz.ToString("F2"));
                        lvi.SubItems.Add(_cdata.QSOList[i].Band);
                        lvi.SubItems.Add(_cdata.QSOList[i].Mode);
                        lvi.SubItems.Add(_cdata.QSOList[i].QSOTime.ToString("yyyy-MM-dd")); // форматировать дату-время
                        lvi.SubItems.Add(_cdata.QSOList[i].QSOTime.ToString("HH:mm")); // форматировать дату-время
                        string s1 = _cdata.QSOList[i].Callsign;
                        s1 = s1.Replace("0", "Ø"); //.Replace("  ", "").Trim();
                        if (_cdata.QSOList[i].XQSO)
                        {
                                lvi.SubItems.Add("(" + s1 + ")");
                        }
                        else
                        {
                            lvi.SubItems.Add(s1);
                        }

                        if (lastRcvdExch == string.Empty)
                        {
                            lastRcvdExch = _cdata.QSOList[i].Rcvd;
                        }

                        // если контест номерной
                        if (_cdata.activeContest.DalHeader.Exchange == string.Empty)
                        {
                            lvi.SubItems.Add(_cdata.QSOList[i].Sent + " " + _cdata.QSOList[i].Nr.ToString("##000"));
                            // проверять номер на Больше предыдущего
                            if (_cdata.QSOList[i].Rcvd != lastRcvdExch)
                            {
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            lvi.SubItems.Add(_cdata.QSOList[i].Sent + " " + _cdata.activeContest.DalHeader.Exchange);
                            if (_cdata.QSOList[i].Rcvd != lastRcvdExch)
                            {
                                lvi.ForeColor = Color.Orange;
                            }
                            else
                            {
                            }
                        }
                        lastRcvdExch = _cdata.QSOList[i].Rcvd;
                        string info = _cdata.activeContest.GetInfoFromDBFile(_cdata.QSOList[i].Callsign, "RCVD");
                        if (info == string.Empty)
                        { 
                            lvi.SubItems.Add(lastRcvdExch); 
                        }
                        else
                        {
                            lvi.SubItems.Add(lastRcvdExch + "(" + info + ")");
                        }
                        lvi.SubItems.Add(_cdata.QSOList[i].Points.ToString());
                        lvi.SubItems.Add(_cdata.QSOList[i].Stn);
                        lvi.SubItems.Add(_cdata.QSOList[i].Mult);
                        lvi.SubItems.Add(_cdata.QSOList[i].Operator);
                       // lvi.SubItems.Add(_cdata.QSOList[i].QSODXCC.CountryName);

                        if (listView1.Items.IndexOf(lvi) == -1)
                        {
                            listView1.Items.Add(lvi);
                        }
                    }
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView1.EndUpdate();
                toolStripStatusLabel1.Text = LastProperty + ": " + listView1.Items.Count.ToString();
            }
            else
            {
                toolStripStatusLabel1.Text = "Enter at least " + (v + 1).ToString() + " chars";
            }
            IsUpdate = false;
        }

        private void HandleNewQSOSaved(DXQSO newQso)
        {
            if (this.InvokeRequired)
            {
                newQsoSaved d = new newQsoSaved(HandleNewQSOSaved);
                this.Invoke(d, new object[] { newQso });
                return;
            }
            LastProperty = string.Empty;
            IsUpdate = false;
            HandleCallsignInfoRefreshed(newQso.Callsign);
        }

        private void InitializeComponent()
        {
            this.panel2 = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.IdHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.QSOIdHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FreqHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BandHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CallsignHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SentHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RcvdHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PointsHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.StnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MultHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OperatorHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DateHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.statusStrip1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 156);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(750, 24);
            this.panel2.TabIndex = 5;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(750, 24);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "© UI3A";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 19);
            this.toolStripStatusLabel1.Text = "© UI3A";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(16, 19);
            this.toolStripStatusLabel2.Text = "...";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.listView1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(750, 156);
            this.panel3.TabIndex = 6;
            // 
            // listView1
            // 
            this.listView1.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listView1.BackColor = System.Drawing.SystemColors.Control;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.IdHeader,
            this.QSOIdHeader,
            this.FreqHeader,
            this.BandHeader,
            this.ModeHeader,
            this.DateHeader,
            this.TimeHeader,
            this.CallsignHeader,
            this.SentHeader,
            this.RcvdHeader,
            this.PointsHeader,
            this.StnHeader,
            this.MultHeader,
            this.OperatorHeader});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(750, 156);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
            // 
            // IdHeader
            // 
            this.IdHeader.Text = "";
            this.IdHeader.Width = 0;
            // 
            // QSOIdHeader
            // 
            this.QSOIdHeader.Text = "QSOId";
            this.QSOIdHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // FreqHeader
            // 
            this.FreqHeader.Text = "Freq";
            this.FreqHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // BandHeader
            // 
            this.BandHeader.Text = "Band";
            this.BandHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ModeHeader
            // 
            this.ModeHeader.Text = "Mode";
            this.ModeHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TimeHeader
            // 
            this.TimeHeader.Text = "Time";
            this.TimeHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CallsignHeader
            // 
            this.CallsignHeader.Text = "Callsign";
            this.CallsignHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SentHeader
            // 
            this.SentHeader.Text = "Sent";
            this.SentHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // RcvdHeader
            // 
            this.RcvdHeader.Text = "Rcvd";
            this.RcvdHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PointsHeader
            // 
            this.PointsHeader.Text = "Pts";
            this.PointsHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // StnHeader
            // 
            this.StnHeader.Text = "Stn";
            this.StnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MultHeader
            // 
            this.MultHeader.Text = "Mult";
            this.MultHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // OperatorHeader
            // 
            this.OperatorHeader.Text = "Operator";
            this.OperatorHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DateHeader
            // 
            this.DateHeader.Text = "Date";
            this.DateHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PrevQSOForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(750, 180);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.FormID = 843201;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrevQSOForm";
            this.Text = "PrevQSO\'s v3.2";
            this.ResizeEnd += new System.EventHandler(this.PrevQSOFrm_ResizeEnd);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void PrevQSOFrm_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string id = listView1.SelectedItems[0].Tag.ToString();
                mainForm.handleKeyCommand("SCROLL_TO_QSO:" + id, null, null);
                mainForm.ActiveQSOLine = _cdata.QSOList[_cdata.QSOList.Count - 1].IDQSO;
            }
            else
            {
                string id = _cdata.QSOList[_cdata.QSOList.Count - 1].IDQSO.ToString();
                mainForm.handleKeyCommand("SCROLL_TO_QSO:" + id, null, null);
                mainForm.handleKeyCommand("SCROLL_TO_END", null, null);
            }
        }
    }
}
