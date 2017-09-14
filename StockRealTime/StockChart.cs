using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GDIChart
{
    partial class StockChart
    {
        public delegate void MetalTableGetHttpEventHandle(int Index);
        public event MetalTableGetHttpEventHandle MetalTableGetHttpEvent;
        public delegate void MetalGetHttpEventHandle(string code,string name);
        public event MetalGetHttpEventHandle MetalGetHttpEvent;
        public delegate void DomesticfuturesGetHttpEventHandle(string code, string name);
        public event DomesticfuturesGetHttpEventHandle DomesticfuturesGetHttpEvent;
        public delegate void commodityGetHttpEventHandle(string code, string name);
        public event commodityGetHttpEventHandle commodityGetHttpEvent;

        public delegate void ClickDoubleListShowDayLineEventHandle();
        public event ClickDoubleListShowDayLineEventHandle ClickDoubleListShowDayLineEvent;
        //数据管理对象
        private DataManager dataManager = new DataManager();
        //行情列表对象
        private ListClass listManager;
        //走势图表对象
        private ChartClass chartManager;
        public StockChart(PictureBox pic)
        {
            //设置PictureBox属性
            pic.Dock = DockStyle.Fill;
            pic.BackColor = Color.Black;
            pic.BackgroundImageLayout = ImageLayout.None;

            //添加行情列表初始列
            dataManager.addColumn("序号", 0, compareType.None, sysColorType.Gray);
            dataManager.addColumn("代码", 70);
            dataManager.addColumn("名称", 100);
            dataManager.setAlignStyle("代码", textAlignStyle.Left);
            dataManager.setAlignStyle("名称", textAlignStyle.Left);
            dataManager.setNoUpdateBackColor("序号");
            dataManager.setNoUpdateBackColor("代码");
            dataManager.setNoUpdateBackColor("名称");

            //初始化行情列表对象
            listManager = new ListClass(pic, dataManager);
            listManager.eventShowChart += new ListClass.delegeteShowChart(ShowCommodityChart);  //绑定事件

            //调试--初始化走势图表
            chartManager = new ChartClass(pic, dataManager);
        }
        public StockChart(PictureBox pic,PictureBox picRealTime)
        {
            //设置PictureBox属性
            pic.Dock = DockStyle.Fill;
            pic.BackColor = Color.Black;
            pic.BackgroundImageLayout = ImageLayout.None;
            picRealTime.Dock= DockStyle.Fill;
            picRealTime.BackColor = Color.Black;
            picRealTime.BackgroundImageLayout = ImageLayout.None;
            //添加行情列表初始列
            dataManager.addColumn("序号", 0, compareType.None, sysColorType.Gray);
            dataManager.addColumn("代码", 70);
            dataManager.addColumn("名称", 100);
            dataManager.setAlignStyle("代码", textAlignStyle.Left);
            dataManager.setAlignStyle("名称", textAlignStyle.Left);
            dataManager.setNoUpdateBackColor("序号");
            dataManager.setNoUpdateBackColor("代码");
            dataManager.setNoUpdateBackColor("名称");

            //初始化行情列表对象
            listManager = new ListClass(pic, dataManager);
            listManager.eventShowChart += new ListClass.delegeteShowChart(ShowCommodityChartRealTime);  //绑定事件

            //调试--初始化走势图表
            chartManager = new ChartClass(picRealTime, dataManager);
        }
        public StockChart()
        {

        }

        #region API接口
        /// <summary>
        /// 添加一个列
        /// </summary>
        /// <param name="ColumnName">列名称</param>
        /// <param name="ColumnWidth">列宽</param>
        /// <param name="CompareType">比较方式</param>
        /// <param name="color">数据项颜色</param>
        public void addColumn(string ColumnName, float ColumnWidth, compareType CompareType = compareType.None, sysColorType color = sysColorType.Default)
        {
            dataManager.addColumn(ColumnName, ColumnWidth, CompareType, color);
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="CommodityName">商品名称</param>
        /// <param name="Code">商品代码</param>
        /// <param name="yesterdayClosePrice">昨日收盘价</param>
        public void addCommodity(string Code, string CommodityName, float yesterdayClosePrice)
        {
            dataManager.addCommodity(Code, CommodityName, yesterdayClosePrice);
        }

        /// <summary>
        /// 添加一个包含时间段的画面
        /// </summary>
        /// <param name="imgName">画面名称</param>
        /// <param name="timePeriod">时间段对象</param>
        public void addImage(string imgName, TimePeriod timePeriod)
        {
            dataManager.addImage(imgName, timePeriod);
        }

        /// <summary>
        /// 添加一个不包含时间段的画面
        /// </summary>
        /// <param name="imgName">画面名称</param>
        public void addImage(string imgName)
        {
            dataManager.addImage(imgName);
        }

        /// <summary>
        /// 在最后添加的画面添加一个区域
        /// </summary>
        /// <param name="areaName">区域名称</param>
        /// <param name="inMiddle">是否在区域中间显示起始值，默认为false。如果为true，则会在右侧显示百分比，并将商品的昨日收盘价作为中间值</param>
        /// <param name="tagMaxMin">是否标记最高值和最低值</param>
        public void addArea(string areaName, bool inMiddle = false, bool tagMaxMin = false)
        {
            dataManager.addArea(areaName, inMiddle, tagMaxMin);
        }

        /// <summary>
        /// 在最后添加的区域里添加线信息（指定显示名称）
        /// </summary>
        /// <param name="lineName">线名称</param>
        /// <param name="lineType">线类型</param>
        /// <param name="showName">显示名称</param>
        /// <param name="groupName">分组名称(默认不写为不分组)</param>
        /// <param name="lineColor">线颜色（如果为Null，则按颜色列表顺序显示）</param>
        public void addLine(string lineName, LineType lineType, string showName, Color? lineColor = null, string groupName = "-")
        {
            dataManager.addLine(lineName, lineType, showName, groupName, lineColor);
        }

        /// <summary>
        /// 在最后添加的区域里添加线信息（不指定显示名称，默认为线名称）
        /// </summary>
        /// <param name="lineName">线名称</param>
        /// <param name="lineType">线类型</param>
        /// <param name="groupName">分组名称(默认不写为不分组)</param>
        /// <param name="lineColor">线颜色（如果为Null，则按颜色列表顺序显示）</param>
        public void addLine(string lineName, LineType lineType, Color? lineColor = null, string groupName = "-")
        {
            addLine(lineName, lineType, lineName, lineColor, groupName);
        }

        /// <summary>
        /// 更改数据对齐方式
        /// </summary>
        /// <param name="columnName">列名称</param>
        /// <param name="dataAlginStyle">对齐方式</param>
        public void setAlignStyle(string columnName, textAlignStyle dataAlginStyle)
        {
            dataManager.setAlignStyle(columnName, dataAlginStyle);
        }

        /// <summary>
        /// 设置数据更新不改变背景色
        /// </summary>
        /// <param name="columnName"></param>
        public void setNoUpdateBackColor(string columnName)
        {
            dataManager.setNoUpdateBackColor(columnName);
        }

        /// <summary>
        /// 设置根据比较值显示涨跌符号（即不显示正负号，改为上下箭头）
        /// </summary>
        /// <param name="columnName"></param>
        public void setCompareSymbol(string columnName)
        {
            dataManager.setCompareSymbol(columnName);
        }

        /// <summary>
        /// 更新行情列表数据(相同则不更新)
        /// </summary>
        /// <param name="columnName">列名称</param>
        /// <param name="commodityName">商品名称</param>
        /// <param name="value">更新值</param>
        public void UpdateData(string columnName, string commodityName, string value)
        {
            if (dataManager.UpdateData(columnName, commodityName, value))
            {
                //如果数据更新成功，则通知listManager修改数据
                listManager.UpdateData(columnName, commodityName, value);
            }
        }

        /// <summary>
        /// 更新行情列表数据(相同则不更新)
        /// </summary>
        /// <param name="columnName">列名称</param>
        /// <param name="commodityName">商品名称</param>
        /// <param name="value">更新值</param>
        public void UpdateData(string columnName, string commodityName, float value)
        {
            UpdateData(columnName, commodityName, Math.Round(value, 2).ToString("0.00"));
        }

        /// <summary>
        /// 添加普通线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">数据值</param>
        public void addLineData(string commodityName, string lineName, string dateTime, float value)
        {
            dataManager.addLineData(commodityName, lineName, dateTime, value);
        }

        /// <summary>
        /// 添加普通线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">数据值</param>
        public void addLineData(string commodityName, string lineName, string dateTime, int value)
        {
            dataManager.addLineData(commodityName, lineName, dateTime, value);
        }

        /// <summary>
        /// 添加K线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">显示数值</param>
        /// <param name="openPrice">开盘价</param>
        /// <param name="closePrice">收盘价</param>
        /// <param name="maxPrice">最高价</param>
        /// <param name="minPrice">最低价</param>
        public void addKLineData(string commodityName, string lineName, string dateTime, float value, float openPrice, float closePrice, float maxPrice, float minPrice)
        {
            dataManager.addKLineData(commodityName, lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
        }

        /// <summary>
        /// 添加K线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">显示数值</param>
        /// <param name="openPrice">开盘价</param>
        /// <param name="closePrice">收盘价</param>
        /// <param name="maxPrice">最高价</param>
        /// <param name="minPrice">最低价</param>
        public void addKLineData(string commodityName, string lineName, string dateTime, int value, int openPrice, int closePrice, int maxPrice, int minPrice)
        {
            dataManager.addKLineData(commodityName, lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
        }

        /// <summary>
        /// 修改普通线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">数据值</param>
        public void updateLineData(string commodityName, string lineName, string dateTime, float value)
        {
            dataManager.updateLineData(commodityName, lineName, dateTime, value);
        }

        /// <summary>
        /// 修改普通线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">数据值</param>
        public void updateLineData(string commodityName, string lineName, string dateTime, int value)
        {
            dataManager.updateLineData(commodityName, lineName, dateTime, value);
        }

        /// <summary>
        /// 修改K线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">显示数值</param>
        /// <param name="openPrice">开盘价</param>
        /// <param name="closePrice">收盘价</param>
        /// <param name="maxPrice">最高价</param>
        /// <param name="minPrice">最低价</param>
        public void updateKLineData(string commodityName, string lineName, string dateTime, float value, float openPrice, float closePrice, float maxPrice, float minPrice)
        {
            dataManager.updateKLineData(commodityName, lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
        }

        /// <summary>
        /// 修改K线数据
        /// </summary>
        /// <param name="commodityName">商品名称</param>
        /// <param name="lineName">线名称</param>
        /// <param name="dateTime">日期时间</param>
        /// <param name="value">显示数值</param>
        /// <param name="openPrice">开盘价</param>
        /// <param name="closePrice">收盘价</param>
        /// <param name="maxPrice">最高价</param>
        /// <param name="minPrice">最低价</param>
        public void updateKLineData(string commodityName, string lineName, string dateTime, int value, int openPrice, int closePrice, int maxPrice, int minPrice)
        {
            dataManager.updateKLineData(commodityName, lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
        }
        #endregion

        #region 控制显示或隐藏接口API
        /// <summary>
        /// 显示行情列表
        /// </summary>
        public void ShowList()
        {
            listManager.Show();
        }

        /// <summary>
        /// 隐藏行情列表
        /// </summary>
        public void HideList()
        {
            listManager.Hide();
        }

        /// <summary>
        /// 显示行情走势
        /// </summary>
        public void ShowChart()
        {
            chartManager.Show();
        }

        /// <summary>
        /// 隐藏行情走势
        /// </summary>
        public void HideChart()
        {
            chartManager.Hide();
        }
        /// <summary>
        /// 显示指定商品的行情走势
        /// </summary>
        /// <param name="CommoditIndex"></param>
        public void ShowCommodityChart()
        {
            //*****加入判断有几个pic
            HideList();
            //ShowChart();
            //设置商品
            chartManager.setCommodity(dataManager.commodity[dataManager.commodity.Count-1]);
            
        }
        /// <summary>
        /// 显示指定商品的行情走势（由双击行情列表事件触发）
        /// </summary>
        /// <param name="CommoditIndex"></param>
        public void ShowCommodityChart(int CommoditIndex)
        {
            //*****加入判断有几个pic
            HideList();
            //ShowChart();
            //设置商品
            chartManager.setCommodity(dataManager.commodity[CommoditIndex]);
            if (DomesticfuturesGetHttpEvent != null)
            {
                DomesticfuturesGetHttpEvent(dataManager.commodity[CommoditIndex].Code.ToString(), dataManager.commodity[CommoditIndex].Name.ToString());
            }
            if (commodityGetHttpEvent != null)
            {
                commodityGetHttpEvent(dataManager.commodity[CommoditIndex].Code.ToString(), dataManager.commodity[CommoditIndex].Name.ToString());
            }
            if (ClickDoubleListShowDayLineEvent != null)
            {
                ClickDoubleListShowDayLineEvent();
            }
        }
        /// <summary>
        /// 显示指定商品的行情走势(商品栏中)
        /// </summary>
        /// <param name="CommoditIndex"></param>
        public void ShowCommodityChartRealTime(int CommoditIndex)
        {
            //双击事件触发商品表
            MetalTableGetHttpEvent(CommoditIndex);
            //*****加入判断有几个pic
            //HideList();
            ShowChart();
            //设置商品
            chartManager.setCommodity(dataManager.commodity[CommoditIndex]);
            //string str=dataManager.commodity[CommoditIndex].Code.ToString();
            if (MetalGetHttpEvent != null)
            {
                //dataManager.commodity
                MetalGetHttpEvent(dataManager.commodity[CommoditIndex].Code.ToString(), dataManager.commodity[CommoditIndex].Name.ToString());
            }
            
        }

        #endregion

        #region 枚举类型
        //颜色参数类
        private static class sysColor
        {
            public static readonly Color Default = Color.FromArgb(82, 255, 255);
            public static readonly Color Red = Color.FromArgb(255, 82, 82);   //红色
            public static readonly Color Gray = Color.FromArgb(192, 192, 192); //灰色
            public static readonly Color Green = Color.FromArgb(82, 255, 82);   //绿色
            public static readonly Color Blue = Color.FromArgb(0, 0, 128);  //蓝色
            public static readonly Color Yellow = Color.FromArgb(255, 255, 82); //黄色

            public static readonly SolidBrush sDefault = new SolidBrush(Default);
            public static readonly SolidBrush sRed = new SolidBrush(Red);
            public static readonly SolidBrush sGray = new SolidBrush(Gray);
            public static readonly SolidBrush sGreen = new SolidBrush(Green);
            public static readonly SolidBrush sBlue = new SolidBrush(Blue);
            public static readonly SolidBrush sYellow = new SolidBrush(Yellow);

            public static Color getColor(sysColorType sc)
            {
                switch (sc)
                {
                    case sysColorType.Blue: return Blue;
                    case sysColorType.Gray: return Gray;
                    case sysColorType.Green: return Green;
                    case sysColorType.Red: return Red;
                    case sysColorType.Yellow: return Yellow;
                    default: return Default;
                }
            }
        }

        //比较结果
        public enum compareResultType
        {
            /// <summary>
            /// 没有比较类型
            /// </summary>
            noComapre,
            /// <summary>
            /// 大于
            /// </summary>
            More,
            /// <summary>
            /// 小于
            /// </summary>
            Less,
            /// <summary>
            /// 相等
            /// </summary>
            Equal,
            /// <summary>
            /// 比较错误
            /// </summary>
            Error
        }
        #endregion
    }
}
