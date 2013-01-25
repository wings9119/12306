using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using _12306Helper.Properties;
using _12306Helper.Helper;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections;
using HtmlAgilityPack;
using System.Threading;
using Crack12306Captcha;
namespace _12306Helper
{
    public partial class main : Form
    {
        DataTable dt = new DataTable();
        string name = string.Empty;
        Dictionary<string, string> mydic = new Dictionary<string, string>();
        DataTable dtTickts = new DataTable();
        List<Checi> myCheci = new List<Checi>();
        public PassengerList pList = new PassengerList();
        public List<Passenger> myList = new List<Passenger>();
        string leftTicketStr = string.Empty;
        string token = "";
        string piaojia = "";
        string myFromno, myTono, myFrom, myTo, myStartTime, myEndTime, myDate, myTrainno, myTraincode;
        string tjdata = "";
        Dictionary<string, string> dicXiBie = new Dictionary<string, string>();
        Dictionary<string, string> dicPiaozhong = new Dictionary<string, string>();
        Thread queryThread;
        public main()
        {
            InitializeComponent();
            Init();
        }
        public main(string _name)
        {
            InitializeComponent();
            Init();
            name = _name;
            linkOpenInIE.Text = name;
        }
        private void Init()
        {
            //初始化控件的值
            this.cmboxCFSJ.SelectedIndex = 0;
            dateCFRQ.MinDate = DateTime.Now;
            dateCFRQ.MaxDate = DateTime.Now.AddDays(19);
            //加载城市
            string cities = Resources.city;
            string[] _cities = cities.Split("@".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            dt.Columns.Add("city");
            dt.Columns.Add("code");
            dt.Columns.Add("filter");
            for (int i = 0; i < _cities.Length; i++)
            {
                string[] lines = _cities[i].Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                DataRow dr = dt.NewRow();
                dr["city"] = lines[1];
                dr["code"] = lines[2];
                dr["filter"] = lines[1] + ";" + lines[3] + ";" + lines[4];
                dt.Rows.Add(dr);
            }
            dt.AcceptChanges();
            mydic.Add("全部", "QB");
            mydic.Add("动车", "D");
            mydic.Add("Z字头", "Z");
            mydic.Add("T字头", "T");
            mydic.Add("K字头", "K");
            mydic.Add("其它", "QT");
            int controlX = 7;
            int controlY = 7;
            foreach (string key in mydic.Keys)
            {
                CheckBox cbox = new CheckBox();
                cbox.Checked = true;
                cbox.Text = key;
                cbox.Tag = mydic[key];
                cbox.Location = new Point(controlX, controlY);
                controlX += 48;
                cbox.Size = new Size(48, 16);
                cbox.Name = "cboxCheCi" + mydic[key];
                cbox.CheckedChanged += new EventHandler(cbox_CheckedChanged);
                this.panel2.Controls.Add(cbox);
            }
            //G525,[始]北京西17:14,汉口22:41,05:27,11,--,有,有,--,--,--,--,--,--,--,预订
            dtTickts.Columns.Add("checi");
            dtTickts.Columns.Add("fazhan");
            dtTickts.Columns.Add("daozhan");
            dtTickts.Columns.Add("lishi");
            dtTickts.Columns.Add("shangwuzuo");
            dtTickts.Columns.Add("tedengzuo");
            dtTickts.Columns.Add("yidengzuo");
            dtTickts.Columns.Add("erdengzuo");
            dtTickts.Columns.Add("gaojiruanwo");
            dtTickts.Columns.Add("ruanwo");
            dtTickts.Columns.Add("yingwo");
            dtTickts.Columns.Add("ruanzuo");
            dtTickts.Columns.Add("yingzuo");
            dtTickts.Columns.Add("wuzuo");
            dtTickts.Columns.Add("qita");
            dtTickts.Columns.Add("goupiao");
            dtTickts.Columns.Add("hideStr");

            this.dataGridView1.Height = this.dataGridView1.Height + this.panel3.Height;

            dicXiBie.Add("商务座", "9");
            dicXiBie.Add("特等座", "p");
            dicXiBie.Add("一等座", "M");
            dicXiBie.Add("二等座", "O");
            dicXiBie.Add("高级软卧", "6");
            dicXiBie.Add("软卧", "4");
            dicXiBie.Add("硬卧", "3");
            dicXiBie.Add("软座", "2");
            dicXiBie.Add("硬座", "1");

            dicPiaozhong.Add("成人票", "1");
            dicPiaozhong.Add("学生票", "2");
            dicPiaozhong.Add("儿童票", "3");



        }

        void cbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbox = sender as CheckBox;
            if (cbox.Tag.ToString() == "QB")
            {
                for (int i = 0; i < this.panel2.Controls.Count; i++)
                {
                    CheckBox _cbox = (CheckBox)this.panel2.Controls[i];
                    _cbox.Checked = cbox.Checked;
                }
            }
        }
        private void txtCFD_TextChanged(object sender, EventArgs e)
        {
            TextChangeEvent(sender);
        }

        private void txtCFD_Leave(object sender, EventArgs e)
        {
            if (this.ActiveControl.GetType().Name != "ListBox")
            {
                this.panel1.Hide();
            }
        }

        private void listBox1_Leave(object sender, EventArgs e)
        {

        }

        private void panel1_Leave(object sender, EventArgs e)
        {

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.panel1.Hide();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GetCity();
        }

        private void TextChangeEvent(object sender)
        {
            TextBox tbox = sender as TextBox;
            if (tbox.Text != string.Empty)
            {
                //查找
                this.listBox1.Items.Clear();
                string filter = tbox.Text.Trim();
                DataRow[] rows = dt.Select("filter like '%" + filter + "%'");
                if (rows.Length > 0)
                {
                    for (int i = 0; i < rows.Length; i++)
                    {
                        string city = rows[i]["city"].ToString();
                        string code = rows[i]["code"].ToString();
                        string space = string.Empty;
                        int length = 10;
                        int sLength = length - (city.Length + code.Length);
                        for (int j = 0; j < sLength; j++)
                        {
                            space = space + "  ";
                        }
                        string str = city + space + code;
                        this.listBox1.Items.Add(str);
                    }
                }
                else
                    tbox.Tag = null;
                Point p1 = tbox.Location;
                this.panel1.Location = new Point(p1.X, p1.Y + tbox.Height);
                this.panel1.Show();
                this.panel1.BringToFront();
            }
            else
                this.panel1.Hide();
        }

        /// <summary>
        /// 获取城市
        /// </summary>
        private void GetCity()
        {
            string text = this.listBox1.Text;
            string[] list = text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string city = list[0];
            string code = list[1];
            if (this.panel1.Location.X == txtCFD.Location.X)
            {
                txtCFD.Text = city;
                txtCFD.Tag = code;
            }
            else
            {
                txtMDD.Text = city;
                txtMDD.Tag = code;
            }
            LoadCheci();
            this.panel1.Hide();
        }
        //加载验证码
        private void GetLoginCode()
        {
            bool flag = false;
            while (!flag)
            {
                string yzm = GetYZM();
                flag = yzm.Length == 4;
            }
        }
        private string GetYZM()
        {
            string url = "https://dynamic.12306.cn/otsweb/passCodeAction.do?rand=randp";
            Stream stream = null;
            stream = HttpHelper.GetResponseImage(url);
            Image image = Image.FromStream(stream);
            this.BeginInvoke((ThreadStart)delegate() { this.picYZM.Image = image; });
            Cracker cracker = new Cracker();
            var result = cracker.Read((Bitmap)image);
            SetYzm(result);
            return result;
        }
        private delegate void WriteYZMDelegate(object yzm);
        private void SetYzm(object text)
        {
            this.txtYZM.Invoke(new WriteYZMDelegate(WriteYZM), text);
        }
        private void WriteYZM(object text)
        {
            txtYZM.Text = text.ToString();
        }
        private void LoadCheci()
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke((ThreadStart)delegate()
                {
                    if (txtCFD.Tag != null && txtMDD.Tag != null)
                    {
                        string url = "https://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=queryststrainall";
                        string from = txtCFD.Tag.ToString();
                        string to = txtMDD.Tag.ToString();
                        string date = dateCFRQ.Value.ToString("yyyy-MM-dd");
                        string time = cmboxCFSJ.SelectedItem.ToString();
                        time = System.Web.HttpUtility.UrlEncode(time);
                        string p = "date={0}&fromstation={1}&tostation={2}&starttime={3}";
                        p = string.Format(p, date, from, to, time);
                        ArrayList mylist = new ArrayList();
                        this.BeginInvoke((ThreadStart)delegate()
                        {
                            string checiJson = HttpHelper.GetResponse(url, "POST", p);
                            if (checiJson != "")
                            {
                                myCheci = JsonHelper.DeserializeToObj<List<Checi>>(checiJson);
                                mylist.Add(new DictionaryEntry("", ""));
                                for (int i = 0; i < myCheci.Count; i++)
                                {
                                    string line = myCheci[i].value + "(" + myCheci[i].start_station_name + myCheci[i].start_time + "→" + myCheci[i].end_station_name + myCheci[i].end_time + ")";
                                    string key = myCheci[i].id;
                                    mylist.Add(new DictionaryEntry(key, line));
                                   
                                }
                                this.BeginInvoke(new MethodInvoker(delegate()
                                {
                                    this.cmbCFCHECI.DataSource = mylist;
                                    this.cmbCFCHECI.DisplayMember = "name";
                                    this.cmbCFCHECI.ValueMember = "value";
                                })); 
                            }
                        });
                    }
                });
            }
        }
        private void txtCFD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                GetCity();
            }
        }

        private void txtCFD_KeyUp(object sender, KeyEventArgs e)
        {
            //↑ code 38 ↓ code 40
            if (e.KeyValue == 38)
            {
                if (this.panel1.Visible == true && this.listBox1.Items.Count > 0)
                {
                    int index = this.listBox1.SelectedIndex;
                    if (index > 0)
                    {
                        this.listBox1.SelectedIndex = index - 1;
                    }
                }
            }
            //↑ code 38 ↓ code 40
            if (e.KeyValue == 40)
            {
                if (this.panel1.Visible == true && this.listBox1.Items.Count > 0)
                {
                    int index = this.listBox1.SelectedIndex;
                    if (index < this.listBox1.Items.Count - 1)
                    {
                        this.listBox1.SelectedIndex = index + 1;
                    }
                }
            }
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                GetCity();
            }
        }

        private void txtMDD_TextChanged(object sender, EventArgs e)
        {
            TextChangeEvent(sender);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pboxChange_Click(object sender, EventArgs e)
        {
            string temp = txtMDD.Text;
            txtMDD.Text = txtCFD.Text;
            txtCFD.Text = temp;
            string tag = txtMDD.Tag != null ? txtMDD.Tag.ToString() : "";
            txtMDD.Tag = txtCFD.Tag;
            txtCFD.Tag = tag;
        }

        private void OpenInIE()
        {
            foreach (Cookie cookie in HttpHelper.CookieContainers.GetCookies(new Uri("https://dynamic.12306.cn/otsweb/loginAction.do?method=login")))
            {
                API.InternetSetCookie("https://" + cookie.Domain.ToString(), cookie.Name.ToString(), cookie.Value.ToString() + ";expires=Sun,22-Feb-2099 00:00:00 GMT");
            }
            Process.Start("IExplore.exe", "https://dynamic.12306.cn/otsweb/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenInIE();
        }
        int querycount = 0;
        delegate void BindCallBack(DataGridView grid, DataTable dt);
        private void DataGridViewBind(DataGridView grid, DataTable dt)
        {
            if (grid.InvokeRequired)
            {
                BindCallBack method = new BindCallBack(this.DataGridViewBind);
                base.Invoke(method, new object[]{grid, dt});
            }
            else
            {
                grid.DataSource = dt;
            }
        }
        delegate void SetQueryInfoCallBack(Label lb, string txt);
        private void SetQueryInfo(Label lb, string txt)
        {
            if (lb.InvokeRequired)
            {
                SetQueryInfoCallBack method = new SetQueryInfoCallBack(SetQueryInfo);
                base.Invoke(method, new object[] {lb,txt});
            }
            else
                lb.Text = txt;
        }
        private void BindCheCi()
        {
            queryThread = new Thread((ThreadStart)delegate()
                {
                    do
                    {
                        querycount++;
                        SetQueryInfo(lbQueryInfo, "第" + querycount + "次查询!");
                        string[] list = DoQuery();
                        this.BeginInvoke(new MethodInvoker(delegate()
                            {
                                dataGridView1.Rows.Clear();
                            }));
                        if (list != null && list.Length > 0)
                        {
                            for (int i = 0; i < list.Length; i++)
                            {
                                string line = list[i];
                                if (line.Length > 10)
                                {
                                    string[] info = line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    DataGridViewRow row = new DataGridViewRow();
                                    row.CreateCells(this.dataGridView1);
                                    for (int j = 1; j < info.Length - 1; j++)
                                    {
                                        row.Cells[j - 1].Value = info[j];
                                    }
                                    string str = info[info.Length - 1];
                                    string startStr = "getSelected(\'";
                                    string endStr = "\')>";
                                    int startIndex = str.IndexOf(startStr);
                                    int endIndex = str.IndexOf(endStr);
                                    row.Cells[info.Length - 2].Value = "预定";
                                    string hideStr = "";
                                    if (startIndex > -1)
                                    {
                                        hideStr = str.Substring(startIndex + startStr.Length, endIndex - (startIndex + startStr.Length));
                                    }
                                    row.Cells[info.Length - 1].Value = hideStr;
                                    this.BeginInvoke(new MethodInvoker(delegate()
                                    {
                                        this.dataGridView1.Rows.Add(row);
                                    }));
                                }
                            }
                            Thread.Sleep(200);
                            FormatDG();
                            Thread.Sleep(5000);
                        }

                    }
                    while (chkAuto.Checked);
                });
            queryThread.IsBackground=true;
            queryThread.Start();
        }
        private void btnQuery_Click(object sender, EventArgs e)
        {
            if (chkAuto.Enabled)
            {
                btnQuery.Enabled = false;
                btnStop.Enabled = true;
            }
            else
                btnQuery.Enabled = true;
            BindCheCi();
        }
        delegate void SetValueHandler(Control ctl,string key,string value);
        SetValueHandler svh;
        private void Set(Control ctl, string key, string value)
        {
            svh = new SetValueHandler(SetControlValue);
            svh.Invoke(ctl,key,value);
            svh.Invoke(ctl, key, value);
        }
        private void SetControlValue(Control ctl,string key, string value)
        {

            if (ctl.InvokeRequired)
            {
                ctl.Invoke(svh, new object[] {ctl, key, value });
            }
            else
            {
                ctl.Text = value; 
            }  
        }
        private string[] DoQuery()
        {
            string from = txtCFD.Tag.ToString();
            string to = txtMDD.Tag.ToString();
            string checi = string.Empty;
            for (int i = 0; i < this.panel2.Controls.Count; i++)
            {
                CheckBox cbox = (CheckBox)this.panel2.Controls[i];
                if (cbox.Checked)
                {
                    checi = checi + cbox.Tag.ToString() + "#";
                }
            }
            string passtype = string.Empty;
            if (radioGL.Checked) passtype = radioGL.Tag.ToString();
            if (radiooQB.Checked) passtype = radiooQB.Tag.ToString();
            if (radioSF.Checked) passtype = radioSF.Tag.ToString();
            string train_no = "";
            var checiIndex = (int)this.Invoke(new Func<int>(() =>
            {
                return cmbCFCHECI.SelectedIndex;
            }));
            if (checiIndex > 0)
            {
                train_no = (string)this.Invoke(new Func<string>(() =>
            {
                return cmbCFCHECI.SelectedValue.ToString();
            }));
            }
            TimeSpan ts = dateCFRQ.Value - DateTime.Now;
            string[] list = null;
            if (ts.Days > 18)
            {
                MessageBox.Show("您选择的日期不在预售期内!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string date = dateCFRQ.Value.ToString("yyyy-MM-dd");
                list = Query(from, to, date, checi, train_no, passtype);
            }
            return list;
        }
        private string[] Query(string from, string to, string date, string checi, string train_no, string passtype)
        {
            string prarams = string.Empty;
            prarams = prarams + "orderRequest.train_date=" + date + "&";
            prarams = prarams + "orderRequest.from_station_telecode=" + from + "&orderRequest.to_station_telecode=" + to + "&";
            prarams = prarams + "orderRequest.train_no=" + train_no + "&";
            checi = System.Web.HttpUtility.UrlEncode(checi);
            prarams = prarams + "trainPassType=" + passtype + "&trainClass=" + checi + "&";
            string student = "00";
            if (chkIsStudent.Checked)
            {
                student = "0X00";
            }
            string time = (string)this.Invoke(new Func<string>(() =>
            {
                return cmboxCFSJ.SelectedItem.ToString();
            }));
            time = System.Web.HttpUtility.UrlEncode(time);
            prarams = prarams + "includeStudent=" + student + "&seatTypeAndNum=&orderRequest.start_time_str=" + time;
            string url = "https://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=queryLeftTicket&";
            url = url + prarams;
            string imgStart = "<img src='/otsweb/images/tips/first.gif'>";
            string imgEnd = "<img src='/otsweb/images/tips/last.gif'>";
            string txtStart = "[始]";
            string txtEnd = "[终]";
            string content = Helper.HttpHelper.GetResponse(url, "get", "");
            content = content.Replace(imgStart, txtStart).Replace(imgEnd, txtEnd);
            content = content.Replace("<a name='btn130_2' class='btn130_2' style='text-decoration:none;' onclick=javascript:", "").Replace("</a>", "");
            content = Common.ClearHtml(content);
            string[] list = content.Replace("&nbsp;", "").Split("\\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            return list;
        }

        private void dateCFRQ_ValueChanged(object sender, EventArgs e)
        {
            LoadCheci();
        }

        private void cmboxCFSJ_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCheci();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender; //如果是"Link"列，被点击
            if (dgv.Columns[e.ColumnIndex].Name == "colCheci")
            {

            }
            if (dgv.Columns[e.ColumnIndex].Name == "colgoupiao")
            {
                DataGridViewDisableButtonCell cell1 = (DataGridViewDisableButtonCell)dgv[e.ColumnIndex, e.RowIndex];
                if (cell1.Value.ToString() == "预定")
                {
                    try
                    {
                        queryThread.Abort();
                        btnQuery.Enabled = true;
                        btnStop.Enabled = false;
                    }
                    catch
                    {
                    }
                    Application.DoEvents();
                    DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgv[e.ColumnIndex + 1, e.RowIndex];
                    string selectStr = cell.Value.ToString();
                    string[] selectStr_arr = selectStr.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var station_train_code = selectStr_arr[0];
                    myTraincode = station_train_code;
                    var train_date = dateCFRQ.Value.ToString("yyyy-MM-dd");
                    myDate = train_date;
                    var lishi = selectStr_arr[1];
                    var starttime = selectStr_arr[2];
                    var trainno = selectStr_arr[3];
                    myTrainno = trainno;
                    var from_station_telecode = selectStr_arr[4];
                    var to_station_telecode = selectStr_arr[5];
                    var arrive_time = selectStr_arr[6];
                    var from_station_name = selectStr_arr[7];
                    myFrom = from_station_name;
                    var to_station_name = selectStr_arr[8];
                    myTo = to_station_name;
                    var from_station_no = selectStr_arr[9];
                    myFromno = from_station_telecode;
                    var to_station_no = selectStr_arr[10];
                    myTono = to_station_telecode;
                    var ypInfoDetail = selectStr_arr[11];
                    var mmStr = selectStr_arr[12];
                    var include_student = chkIsStudent.Checked ? "0X00" : "00";
                    var locationCode = selectStr_arr[13];
                    var round_start_time_str = cmboxCFSJ.SelectedItem.ToString();
                    string checi = string.Empty;
                    for (int i = 0; i < this.panel2.Controls.Count; i++)
                    {
                        CheckBox cbox = (CheckBox)this.panel2.Controls[i];
                        if (cbox.Checked)
                        {
                            checi = checi + cbox.Tag.ToString() + "#";
                        }
                    }
                    string url = "https://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=submutOrderRequest";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("station_train_code", station_train_code);
                    dic.Add("train_date", train_date);
                    dic.Add("seattype_num", "");
                    dic.Add("from_station_telecode", from_station_telecode);
                    dic.Add("to_station_telecode", to_station_telecode);
                    dic.Add("include_student", include_student);
                    dic.Add("from_station_telecode_name", System.Web.HttpUtility.UrlEncode(from_station_name));
                    dic.Add("to_station_telecode_name", System.Web.HttpUtility.UrlEncode(to_station_name));
                    dic.Add("round_train_date", System.Web.HttpUtility.UrlEncode(round_start_time_str));
                    dic.Add("single_round_type", "1");
                    dic.Add("train_pass_type", "QB");
                    dic.Add("train_class_arr", System.Web.HttpUtility.UrlEncode(checi));
                    dic.Add("start_time_str", System.Web.HttpUtility.UrlEncode(round_start_time_str));
                    dic.Add("lishi", System.Web.HttpUtility.UrlEncode(lishi));
                    dic.Add("train_start_time", System.Web.HttpUtility.UrlEncode(starttime));
                    dic.Add("trainno4", trainno);
                    dic.Add("arrive_time", System.Web.HttpUtility.UrlEncode(arrive_time));
                    dic.Add("from_station_name", System.Web.HttpUtility.UrlEncode(from_station_name));
                    dic.Add("to_station_name", System.Web.HttpUtility.UrlEncode(to_station_name));
                    dic.Add("from_station_no", from_station_no);
                    dic.Add("to_station_no", to_station_no);
                    dic.Add("ypInfoDetail", ypInfoDetail);
                    dic.Add("mmStr", mmStr);
                    dic.Add("locationCode", locationCode);
                    string data = "";
                    foreach (string key in dic.Keys)
                    {
                        data = data + key + "=" + dic[key] + "&";
                    }
                    data = data.Substring(0, data.Length - 1);
                    this.BeginInvoke((ThreadStart)delegate()
                    {
                        lbCheciInfo.Text = "数据加载中。。。";
                        string str = HttpHelper.GetResponse(url, "POST", data);
                        if (!str.Contains("目前您还有未处理的订单"))
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(str);
                            HtmlNodeCollection node = doc.DocumentNode.SelectNodes("//tr[1]//td");
                            string checiinfo = "";
                            for (int i = 0; i < node.Count; i++)
                            {
                                string info = node[i].InnerText;
                                info = info.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("&nbsp;", "");
                                if (i == 2)
                                {
                                    var temp = info;
                                    myStartTime = temp.Replace(myFrom, "").Replace("开", "").Replace("（", "").Replace("）", "").Trim();
                                }
                                if (i == 3)
                                {
                                    var temp = info;
                                    myEndTime = temp.Replace(myTo, "").Replace("到", "").Replace("（", "").Replace("）", "").Trim();
                                }
                                checiinfo = checiinfo + info + "                 ";
                            }
                            lbCheciInfo.Text = checiinfo;
                            string _piaojia = doc.DocumentNode.SelectNodes("//tr[2]")[0].InnerText.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("&nbsp;", " ");
                            string[] _piaojiaList = _piaojia.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < _piaojiaList.Length; i++)
                            {
                                string info = _piaojiaList[i];
                                piaojia = piaojia + info + "                 ";
                            }
                            lbPiaojia.Text = piaojia;

                            this.dataGridView1.Height = this.dataGridView1.Height = this.panel3.Height;
                            this.panel3.Show();

                            // 更新 TOKEN 
                            url = "https://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=init";
                            data = string.Empty;
                            string html = HttpHelper.GetResponse(url, "POST", data);
                            string temp1 = html;
                            temp1 = temp1.Substring(temp1.IndexOf("name=\"org.apache.struts.taglib.html.TOKEN\" value=\"") + 50);
                            token = temp1.Substring(0, temp1.IndexOf('"'));  // 获取令牌
                            temp1 = html;
                            int index = temp1.IndexOf("id=\"left_ticket\"");
                            temp1 = temp1.Substring(temp1.IndexOf("id=\"left_ticket\"") + 26);
                            leftTicketStr = temp1.Substring(0, temp1.IndexOf('"'));
                        }
                        else
                            MessageBox.Show("目前您还有未处理的订单，请先处理订单！");
                    });

                }
            }
        }
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {

        }
       
        private void LoadPasssenger()
        {
            BackgroundWorker Bw = new BackgroundWorker();
            Bw.WorkerSupportsCancellation = true;
            Bw.WorkerReportsProgress = true;
            Bw.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                string url = "https://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=getpassengerJson";
                string content = HttpHelper.GetResponse(url, "GET", "");
                pList = JsonHelper.DeserializeToObj<PassengerList>(content);
                int controlX = 6;
                int controlY = 6;
                int i = 0;
                foreach (Passenger key in pList.passengerJson)
                {
                    CheckBox cbox = new CheckBox();
                    cbox.Checked = false;
                    cbox.Text = key.passenger_name;
                    cbox.Tag = key;
                    cbox.Location = new Point(controlX, controlY);
                    controlX += 70;
                    cbox.Size = new Size(70, 16);
                    cbox.CheckedChanged += new EventHandler(cboxChange);
                    cbox.Name = "cbox" + i;
                    i++;
                    this.panleUSER.Controls.Add(cbox);
                }
            });
            Bw.RunWorkerAsync();
            
        }
        void cboxChange(object sender, EventArgs e)
        {
            string selected = "";
            myList = new List<Passenger>();
            int count = 0;
            for (int i = 0; i < this.panleUSER.Controls.Count; i++)
            {
                CheckBox cb = this.panleUSER.Controls[i] as CheckBox;
                if (cb.Checked)
                {
                    count++;
                    if (count > 5)
                    {
                        MessageBox.Show("对不起，你最多只能选择5位联系人！");
                        selected = selected.Replace(cb.Text + " ", "");
                        myList.Remove(cb.Tag as Passenger);
                        cb.Checked = false;
                        break;
                    }
                    else
                    {
                        selected = selected + cb.Text + " ";
                        myList.Add(cb.Tag as Passenger);

                    }
                }
                else
                {
                    selected = selected.Replace(cb.Text + " ", "");
                    myList.Remove(cb.Tag as Passenger);
                }
            }
            Goupiao(myList);
        }
        private void FormatDG()
        {
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                bool canOrder = false;
                DataGridViewDisableButtonCell cell = (DataGridViewDisableButtonCell)dataGridView1[this.dataGridView1.ColumnCount-2, i];
                DataGridViewLinkCell cellCheci = (DataGridViewLinkCell)dataGridView1[0, i];
                for (int j = 4; j < this.dataGridView1.ColumnCount - 2; j++)
                {
                    string value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    if (Common.IsNumeric(value) || value.IndexOf("有") > -1)
                    {
                        //dataGridView1.Rows[i].Cells[j].Style.BackColor = System.Drawing.Color.Red;
                        canOrder = true;
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = System.Drawing.Color.Gray;
                        continue;
                    }
                }
                if (canOrder)
                {
                    cell.Tag = cellCheci.Value;
                    cell.Enabled = true;
                    cell.Style.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    cell.Enabled = false;
                    cell.Value = "无票";
                }
            }
        }


        private void main_Load(object sender, EventArgs e)
        {
            GetLoginCode();
            LoadPasssenger();
        }

        private void picYZM_Click(object sender, EventArgs e)
        {
            GetLoginCode();
        }


        public void Goupiao(List<Passenger> list)
        {
            //1.绑定席别
            DataTable dtxibie = new DataTable();
            dtxibie.Columns.Add("xibie");
            dtxibie.Columns.Add("code");
            string[] piaojiaList = piaojia.Split("                 ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < piaojiaList.Length; i++)
            {
                string str = piaojiaList[i];
                int start = str.IndexOf("(");
                string xibie = str.Substring(0, start);
                int end = str.IndexOf(")");
                string piaoj = str.Substring(end + 1);
                if (piaoj.IndexOf("张票") > -1 || piaoj.IndexOf("有") > -1)
                {
                    xibie = xibie == "无座" ? "硬座" : xibie;
                    DataRow row = dtxibie.NewRow();
                    row["xibie"] = xibie;
                    row["code"] = dicXiBie[xibie];
                    dtxibie.Rows.Add(row);
                }
            }
            if (dtxibie.Rows.Count > 0)
            {
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).DataSource = dtxibie;
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).ValueMember = "code";
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).DisplayMember = "xibie";
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).DataPropertyName = "xibie";
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).DefaultCellStyle.NullValue = dtxibie.Rows[0]["xibie"];
                ((DataGridViewComboBoxColumn)dataGridView2.Columns["colxiebie"]).DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

            }
            DataTable dtPiaozhong = new DataTable();
            dtPiaozhong.Columns.Add("name");
            dtPiaozhong.Columns.Add("value");
            foreach (string key in dicPiaozhong.Keys)
            {
                DataRow row = dtPiaozhong.NewRow();
                row["name"] = key;
                row["value"] = dicPiaozhong[key];
                dtPiaozhong.Rows.Add(row);
            }
            ((DataGridViewComboBoxColumn)dataGridView2.Columns["colpiaozhong"]).DataSource = dtPiaozhong;
            ((DataGridViewComboBoxColumn)dataGridView2.Columns["colpiaozhong"]).ValueMember = "value";
            ((DataGridViewComboBoxColumn)dataGridView2.Columns["colpiaozhong"]).DisplayMember = "name";
            ((DataGridViewComboBoxColumn)dataGridView2.Columns["colpiaozhong"]).DefaultCellStyle.NullValue = dtPiaozhong.Rows[0]["name"];
            ((DataGridViewComboBoxColumn)dataGridView2.Columns["colpiaozhong"]).DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

            //绑定
            DataTable dtGoupiao = new DataTable();
            dtGoupiao.Columns.Add("xh");
            dtGoupiao.Columns.Add("xm");
            dtGoupiao.Columns.Add("zjhm");
            dtGoupiao.Columns.Add("sjhm");
            dtGoupiao.Columns.Add("zjlx");
            dtGoupiao.Columns.Add("del");

            for (int i = 0; i < list.Count; i++)
            {
                DataRow row = dtGoupiao.NewRow();
                string xm = list[i].passenger_name;
                string xh = "第" + (i + 1) + "位";
                string zjhm = list[i].passenger_id_no;
                string zjlx = "二代身份证";
                string sjhm = list[i].mobile_no;
                row["xh"] = xh;
                row["xm"] = xm;
                row["zjhm"] = zjhm;
                row["zjlx"] = zjlx;
                row["sjhm"] = sjhm;
                row["del"] = "删除";
                dtGoupiao.Rows.Add(row);
            }
            this.dataGridView2.DataSource = dtGoupiao;
            this.dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
        }
        private void btnOrder_Click(object sender, EventArgs e)
        {
            //循环获取乘客信息
            GetLoginCode();
            string pssengerstr = string.Empty;
            if (this.dataGridView2.Rows.Count > 0)
            {
                for (int i = 0; i < this.dataGridView2.Rows.Count; i++)
                {
                    string xiebie = this.dataGridView2.Rows[i].Cells[0].FormattedValue.ToString();
                    xiebie = dicXiBie[xiebie];
                    string seat = this.dataGridView2.Rows[i].Cells[1].FormattedValue.ToString();
                    seat = dicPiaozhong[seat];
                    string xm = this.dataGridView2.Rows[i].Cells["colxm"].Value.ToString();
                    string zjhm = this.dataGridView2.Rows[i].Cells["colzjhm"].Value.ToString();
                    string sjhm = this.dataGridView2.Rows[i].Cells["colsjhm"].Value.ToString();

                    string data = "passengerTickets=" + System.Web.HttpUtility.UrlEncode(xiebie + ",undefined," + seat + "," + xm + ",1," + zjhm + "," + sjhm + ",Y");
                    data += "&oldPassengers=" + System.Web.HttpUtility.UrlEncode(xm + ",1," + zjhm);
                    data += "&passenger_" + (i + 1) + "_seat=" + System.Web.HttpUtility.UrlEncode(xiebie);   // 1硬座/3硬卧/4软卧
                    data += "&passenger_" + (i + 1) + "_ticket=" + System.Web.HttpUtility.UrlEncode(seat);
                    data += "&passenger_" + (i + 1) + "_name=" + System.Web.HttpUtility.UrlEncode(xm);
                    data += "&passenger_" + (i + 1) + "_cardtype=1";
                    data += "&passenger_" + (i + 1) + "_cardno=" + zjhm;
                    data += "&passenger_" + (i + 1) + "_mobileno=" + sjhm;
                    data += "&checkbox9=Y";
                    pssengerstr = pssengerstr + data + "&";
                }
                int leftcount = 5 - this.dataGridView2.Rows.Count;
                for (int i = 0; i < leftcount; i++)
                {
                    pssengerstr = pssengerstr + "oldPassengers=" + "&checkbox9=Y&";
                }
                pssengerstr = pssengerstr.Substring(0, pssengerstr.Length - 1);

                string html = Order(myDate, myTrainno, myTraincode, myFromno, myFrom, myTono, myTo, myStartTime, myEndTime, pssengerstr);

                //string result = html.Split('"')[15];

                if (html.Contains("对不起，由于您取消次数过多"))
                {
                    Output("对不起，由于您取消次数过多，今日将不能继续受理您的订票请求，程序结束");
                    GetLoginCode();
                }
                else if (html.Contains("输入的验证码不正确") || html.Contains("验证码 必须输入"))
                {
                    Output("订单验证码比对失败，重新加载");
                    GetLoginCode();
                }
                else
                {
                    new Thread(new ThreadStart(Buy)).Start();
                }
            }
            else
                MessageBox.Show("请至少选择一名乘客信息！");
        }
        public void Buy() // 购票
        {
            //查询余票
            string url = "https://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=getQueueCount";
            string xibie = this.dataGridView2.Rows[0].Cells[0].FormattedValue.ToString();
            xibie = dicXiBie[xibie];
            string data = "train_date=" + myDate;
            data += "&train_no=" + myTrainno;
            data += "&station=" + myTraincode;
            data += "&seat=" + xibie;
            data += "&from=" + myFromno;
            data += "&to=" + myTono;
            data += "&ticket=" + leftTicketStr;
            string html = HttpHelper.GetResponse(url, "Post", data);


            Output("购票");

             url = "https://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=confirmSingleForQueueOrder";
             string refer = "https://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=submutOrderRequest";
             html = HttpHelper.GetResponse(url, "POST", tjdata.Substring(0, tjdata.Length - 9),refer);

            string result = html.Split('"')[3];

            if (result == "Y")
            {
                Output("确认订单成功:");
            }
            else
            {
                Output("确认订单失败:" + html.Split('"')[3] + "" + (html.Split('"').Length >= 11 ? html.Split('"')[11] : "") + "\r\n");
                GetLoginCode();
            }
            while (true)
            {
                url = "https://dynamic.12306.cn/otsweb/order/myOrderAction.do?method=queryMyOrderNotComplete&leftmenu=Y";
                html = HttpHelper.GetResponse(url, "Post", "");
                int index = html.IndexOf("<div class=\"clear\"></div>");  //-1
                if (index == -1)
                {
                    Output("未完成订单没有数据\r\n");
                    break;
                }
                else
                {
                    html = html.Substring(index);
                    html = html.Substring(0, html.IndexOf("</table>"));
                    html = Common.ClearHtml(html);
                    int xxxx = html.IndexOf("\r\n");
                    html = html.Replace("\r\n", "");
                    html = html.Replace(" ", "");
                    html = html.Replace("\t", "");
                    html = html.Replace("\n", "");

                    Output("系统返回：" + html.Substring(16) + "\r\n");
                    if (html.Contains("继续支付"))
                    {
                        //OutputMusic();
                        Output(DateTime.Now.ToString("hh-mm-ss") + " 购票成功,程序终止\r\n");
                        break;
                    }
                    else if (html.Contains("取消订单"))
                    {
                        Output("排队中,2秒后查询结果\r\n");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        Output("购票未成功\r\n");
                        Output(html.Substring(16) + "\r\n");
                        break;
                    }
                }
            }
        }
        private delegate void OutputDelegate(object entry);
        private void Output(string text)
        {
            this.txtMessage.Invoke(new OutputDelegate(AddMessage), text);
        }
        private void AddMessage(object obj)
        {
            this.txtMessage.Text =this.txtMessage.Text+"\r\n"+ DateTime.Now.ToString() + ":" + obj.ToString() ;
        }
        public string Order(string date, string trainno, string traincode, string fromno, string from, string tono, string to, string starttime, string endtime, string passenger) // 提交订单
        {
            string yzm = txtYZM.Text.Trim();
            string url = "https://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=checkOrderInfo&rand=" + yzm;
            string data_buy = "";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("org.apache.struts.taglib.html.TOKEN", token);
            dic.Add("leftTicketStr", leftTicketStr);
            dic.Add("textfield", System.Web.HttpUtility.UrlEncode("中文或拼音首字母"));
            dic.Add("checkbox0", "0");
            dic.Add("orderRequest.train_date", date);
            dic.Add("orderRequest.train_no", trainno);
            dic.Add("orderRequest.station_train_code", traincode);
            dic.Add("orderRequest.from_station_telecode", fromno);
            dic.Add("orderRequest.to_station_telecode", tono);
            dic.Add("orderRequest.seat_type_code", "");
            dic.Add("orderRequest.ticket_type_order_num", "");
            dic.Add("orderRequest.bed_level_order_num", "000000000000000000000000000000");
            dic.Add("orderRequest.start_time", System.Web.HttpUtility.UrlEncode(starttime));
            dic.Add("orderRequest.end_time", System.Web.HttpUtility.UrlEncode(endtime));
            dic.Add("orderRequest.from_station_name", System.Web.HttpUtility.UrlEncode(from));
            dic.Add("orderRequest.to_station_name", System.Web.HttpUtility.UrlEncode(to));
            dic.Add("orderRequest.cancel_flag", "1");
            dic.Add("orderRequest.id_mode", "Y");
            foreach (string key in dic.Keys)
            {
                data_buy = data_buy + key + "=" + dic[key] + "&";
            }
            data_buy += passenger;
            data_buy += "&randCode=" + yzm;              // 验证码
            data_buy += "&orderRequest.reserve_flag=A";
            data_buy += "&tFlag=dc";
            tjdata = data_buy;
            string html = HttpHelper.GetResponse(url, "POST", data_buy);
            return html;
        }

        private void picYZM_Click_1(object sender, EventArgs e)
        {
            GetLoginCode();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                queryThread.Abort();
                btnQuery.Enabled = true;
                btnStop.Enabled = false;
            }
            catch
            {
            }
        }

    }
}
