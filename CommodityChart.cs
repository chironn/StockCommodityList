using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GDIChart;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StockCommodityList
{
    public partial class CommodityChart : UserControl
    {
        StockChart sc;
        Thread t;
        delegate void updateth();
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productStyle = "分时走势";
        public CommodityChart(String productName, String productCode)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            //productName = "燃油10";
            //productCode = "NECLA0";
            this.productName = productName;
            this.productCode = productCode;
            //创建走势对象
            sc = new StockChart(pictureBox1);
            //创建时间段
            TimePeriod tp = new TimePeriod();
            tp.addTimePeriod(6, 0, 4, 0);
            //tp.addTimePeriod(9, 30, 11, 30);
            //tp.addTimePeriod(13, 0, 15, 0);
            //tp.addTimePeriod(9, 30, 10, 30);

            //新增画面
            sc.addImage("走势", tp);
            //添加区域
            sc.addArea("分时走势", true);
            sc.addLine("分时走势", LineType.BrokenLine);
            sc.addLine("均线", LineType.BrokenLine);

            sc.addArea("成交量");
            sc.addLine("成交量", LineType.VerticalLine, Color.Yellow);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        private void CommodityChart_Load(object sender, EventArgs e)
        {
            sc.addColumn("时间", 100, compareType.compareWithValue);
            sc.addColumn("昨收", 100, compareType.compareWithValue);
            sc.addColumn("高开收低价的均价", 100, compareType.compareWithValue);
            sc.addColumn("均价", 100, compareType.compareWithValue);//new 
            sc.addColumn("持仓量", 100, compareType.compareWithValue);//new 
            sc.addColumn("当日总成交量", 100, compareType.compareWith0);
            sc.addColumn("当日总成交额", 100, compareType.compareWithValue);
            //sc.addColumn("最低", 100, compareType.compareWithValue);
            //sc.addColumn("成交数", 120, compareType.None);
            //sc.addColumn("持仓量", 120, compareType.None);
            //sc.setCompareSymbol("涨跌");
            AddStockChart(productName,productCode);
            sc.ShowCommodityChart(0);

        }
        public void ShowCommodityChart()
        {
            sc.ShowCommodityChart();
        }
        public void saveValue(string code, string name)
        {
            productCode = code;
            productName = name;
            AddStockChart(productCode, productName);//显示分时走势
            //sc.ShowCommodityChart();
        }
        public void AddStockChart(string name, string code)
        {
            //添加股票
            string serviceAddress = "http://demo.hrain.top/trades/api.php?s=data/getFuturesThetimetrend&code=" + code;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                List<string[]> list = new List<string[]>();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                //添加商品
                JObject jo = (JObject)JsonConvert.DeserializeObject(retString);
                JArray jar = JArray.Parse(jo["data"].ToString());
                
                JObject job = JObject.Parse(jar[0].ToString());
                sc.addCommodity(code, name, float.Parse(job["close"].ToString()));//temp[0]:编码；temp[1]：名称；temp[3]：昨日收盘价
                sc.UpdateData("时间", name, (job["time"].ToString()));
                sc.UpdateData("昨收", name, float.Parse(job["close"].ToString()));//
                sc.UpdateData("高开收低价的均价", name, float.Parse(job["trend"].ToString()));
                sc.UpdateData("均价", name, float.Parse(job["average"].ToString()));
                sc.UpdateData("持仓量", name, float.Parse(job["open_int"].ToString()));
                sc.UpdateData("当日总成交量", name, float.Parse(job["volume"].ToString()));
                sc.UpdateData("当日总成交额", name, float.Parse(job["amount"].ToString()));
                for (var i = 0; i < jar.Count; i++)//jar.Count
                {
                    JObject j = JObject.Parse(jar[i].ToString());
                    //temp[0] 时间 temp[1] 白线 temp[2] 成交量
                    string dtTime = j["time"].ToString().Substring(11, 5);
                    string time = dtTime.Remove(2, 1);
                    sc.addLineData(name, "分时走势", time, float.Parse(j["trend"].ToString()));
                    //Console.WriteLine(commodityName + " " + temp[0] + " " + (float)(sum / (float)(i - 1)));
                    sc.addLineData(name, "均线", time, float.Parse(j["average"].ToString()));
                    sc.addLineData(name, "成交量", time, float.Parse(j["volume"].ToString()));

                }

                //sc.ShowCommodityChart(0);
                //显示列表
                sc.ShowChart();
                t = new Thread(new ThreadStart(updateThread));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }
        public void ShowList()
        {
            sc.ShowList();
        }
        public void ShowChart()
        {
            sc.ShowChart();
        }
        private void updateThread()
        {
            while (true)
            {
                if (this.IsDisposed) return;
                //更新数据
                string serviceAddress = "http://demo.hrain.top/trades/api.php?s=data/getFuturesThetimetrend&code=" + productCode;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                List<string[]> list = new List<string[]>();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                //添加商品
                JObject jo = (JObject)JsonConvert.DeserializeObject(retString);
                JArray jar = JArray.Parse(jo["data"].ToString());
                //for (var i = 0; i < jar.Count; i++)//jar.Count
                //{
                    JObject j = JObject.Parse(jar[0].ToString());
                    sc.UpdateData("时间", ProductName, (j["time"].ToString()));
                    sc.UpdateData("昨收", ProductName, float.Parse(j["close"].ToString()));//
                    sc.UpdateData("高开收低价的均价", ProductName, float.Parse(j["trend"].ToString()));
                    sc.UpdateData("均价", ProductName, float.Parse(j["average"].ToString()));
                    sc.UpdateData("持仓量", ProductName, float.Parse(j["open_int"].ToString()));
                    sc.UpdateData("当日总成交量", ProductName, float.Parse(j["volume"].ToString()));
                    sc.UpdateData("当日总成交额", ProductName, float.Parse(j["amount"].ToString()));
                //}
                Thread.Sleep(2000);
            }
        }

        private void getLine(string code, string commodityName)
        {
            //Console.WriteLine(commodityName + " 分时走势已添加");
            //HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse("http://data.gtimg.cn/flashdata/hushen/minute/sh" + code + ".js", null, null, null);
            //string gethtml;
            //using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("gb2312")))
            //{
            //    gethtml = reader.ReadToEnd();
            //    //关闭输入流
            //    reader.Close();
            //    //关闭连接
            //    response.Close();
            //}
            //gethtml = gethtml.Replace("\\n\\\n", "\n");
            //string[] str = gethtml.Split('\n');
            //float sum = 0;
            //int lasttrace = 0;
            //for (int i = 2; i < str.Length - 2; i++)
            //{
            //    string[] temp = str[i].Split(' ');
            //    sc.addLineData(commodityName, "分时走势", temp[0], float.Parse(temp[1]));
            //    sum += float.Parse(temp[1]);
            //    //Console.WriteLine(commodityName + " " + temp[0] + " " + (float)(sum / (float)(i - 1)));
            //    sc.addLineData(commodityName, "均线", temp[0], (float)(sum / (float)(i - 1)));
            //    sc.addLineData(commodityName, "成交量", temp[0], int.Parse(temp[2]) - lasttrace);
            //    lasttrace = int.Parse(temp[2]);
            //}
        }
    }
}
