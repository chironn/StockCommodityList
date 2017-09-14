using System;
using System.Collections.Generic;
using System.Drawing;

namespace GDIChart
{
    /// <summary>
    /// 文本对齐方式
    /// </summary>
    public enum textAlignStyle
    {
        /// <summary>
        /// 左对齐
        /// </summary>
        Left,
        /// <summary>
        /// 居中对齐
        /// </summary>
        Center,
        /// <summary>
        /// 右对齐
        /// </summary>
        Right
    }

    /// <summary>
    /// 数值比较类型
    /// </summary>
    public enum compareType
    {
        /// <summary>
        /// 不比较
        /// </summary>
        None,
        /// <summary>
        /// 与比较值比较
        /// </summary>
        compareWithValue,
        /// <summary>
        /// 正负比较
        /// </summary>
        compareWith0
    }

    /// <summary>
    /// 颜色枚举
    /// </summary>
    public enum sysColorType
    {
        /// <summary>
        /// 默认颜色
        /// </summary>
        Default,
        /// <summary>
        /// 红色
        /// </summary>
        Red,
        /// <summary>
        /// 灰色
        /// </summary>
        Gray,
        /// <summary>
        /// 绿色
        /// </summary>
        Green,
        /// <summary>
        /// 蓝色
        /// </summary>
        Blue,
        /// <summary>
        /// 黄色
        /// </summary>
        Yellow
    }

    /// <summary>
    /// 线类型
    /// </summary>
    public enum LineType
    {
        /// <summary>
        /// 折线
        /// </summary>
        BrokenLine,
        /// <summary>
        /// K线
        /// </summary>
        KLine,
        /// <summary>
        /// 垂直线
        /// </summary>
        VerticalLine,
        /// <summary>
        /// 柱状线
        /// </summary>
        BarLine
    }

    /// <summary>
    /// 时间段类
    /// </summary>
    public class TimePeriod
    {
        /// <summary>
        /// 根据时间段生成的时间点字符串集合
        /// </summary>
        public List<string> str = new List<string>();
        /// <summary>
        /// 时间段的中断时间点字符串集合
        /// </summary>
        public List<string> strPoint = new List<string>();
        /// <summary>
        /// 时间段集合
        /// </summary>
        public List<DateTime> dt = new List<DateTime>();

        int _timeSpacing = 60;
        /// <summary>
        /// 获取时间间隔
        /// </summary>
        public int timeSpacing
        {
            get { return _timeSpacing; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Minutes">时间间隔</param>
        public TimePeriod(int Minutes = 120)
        {
            this._timeSpacing = Minutes;
        }

        /// <summary>
        /// 添加时间段
        /// </summary>
        /// <param name="Hour1">起始时间的小时</param>
        /// <param name="Min1">起始时间的分钟</param>
        /// <param name="Hour2">终止时间的小时</param>
        /// <param name="Min2">终止时间的分钟</param>
        public void addTimePeriod(int Hour1, int Min1, int Hour2, int Min2)
        {
            try
            {
                DateTime a = new DateTime(2014, 1, 1, Hour1, Min1, 0);
                DateTime b = new DateTime(2014, 1, 2, Hour2, Min2, 0);
                dt.Add(a);
                dt.Add(b);

                //计算时间差
                TimeSpan ts = b - a;
                for (int i = 0; i <= ts.TotalMinutes; i++)
                {
                    //如果不是第一个时间段，则跳过第一个时间点，避免和上一时间段的最后一个时间点重复
                    if (str.Count != 0 && i == 0) continue;
                    str.Add((a + TimeSpan.FromMinutes(i)).ToString("HH:mm"));
                }
                //添加终止时间点为间隔点（画粗的时间列线）
                strPoint.Add(b.ToString("HH:mm"));
            }
            catch (Exception)
            {
                Console.WriteLine("时间格式有误");
            }
        }
        /// <summary>
        /// 获取总时间段的分钟数
        /// </summary>
        /// <returns></returns>
        public int getMinuteCount()
        {
            return str.Count;
        }
    }

    /// <summary>
    /// 静态参数类
    /// </summary>
    static class staPara
    {
        //Graphics测量对象
        private static Graphics gra = Graphics.FromImage(new Bitmap(1, 1));

        /// <summary>
        /// 全局字体
        /// </summary>
        public static readonly Font Font = new Font("宋体", 13.5f);

        /// <summary>
        /// 全局图表字体
        /// </summary>
        public static readonly Font ChartFont = new Font("宋体", 11.5f);

        /// <summary>
        /// 字符串尺寸测量方法
        /// </summary>
        /// <param name="str">待测量的字符串</param>
        /// <returns>字符串尺寸</returns>
        public static SizeF getStringSizeF(string str)
        {
            return gra.MeasureString(str, staPara.Font);
        }

        /// <summary>
        /// 图表字符串尺寸测量方法
        /// </summary>
        /// <param name="str">待测量的字符串</param>
        /// <returns>字符串尺寸</returns>
        public static SizeF getChartStringSizeF(string str)
        {
            return gra.MeasureString(str, staPara.ChartFont);
        }

        /// <summary>
        /// 鼠标识别误差象素点数
        /// </summary>
        public static readonly int IdentificationErrorPixelPoints = 5;

        /// <summary>
        /// 列前后间隔宽度和
        /// </summary>
        public static readonly int intervalColumnWidth2 = 10;
        public static readonly int intervalColumnWidth = intervalColumnWidth2 / 2;

        /// <summary>
        /// 最小列宽(建议大于列前后间隔宽度和)
        /// </summary>
        public static readonly int minColumnWidth = 60;

        /// <summary>
        /// 鼠标滚轮滚动行数
        /// </summary>
        public static readonly int MouseWheelScroll = 3;

        /// <summary>
        /// 行情列表单位行高度
        /// </summary>
        public static readonly float rowHeight = gra.MeasureString("获取单位行高度", new Font("New Times Roman", 16f)).Height;

        /// <summary>
        /// 图表字体单位行高度
        /// </summary>
        public static readonly float rowChartHeight = gra.MeasureString("获取单位行高度", ChartFont).Height;

        /// <summary>
        /// 行情列表固定列数
        /// </summary>
        public static readonly int fixColumnIndex = 2;

        /// <summary>
        /// 行情走势的最小行间隔
        /// </summary>
        public static readonly float rowMinSpacing = 35;
    }
}
