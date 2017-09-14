using System;
using System.Collections.Generic;
using System.Drawing;

namespace GDIChart
{
    partial class StockChart
    {
        /// <summary>
        /// 线信息类
        /// </summary>
        public class LineInfoClass
        {
            private string _lineName;
            /// <summary>
            /// 线名称
            /// </summary>
            public string lineName
            {
                get { return _lineName; }
            }

            private LineType _lineType;
            /// <summary>
            /// 线类型
            /// </summary>
            public LineType lineType
            {
                get { return _lineType; }
            }

            private string _showName;
            /// <summary>
            /// 显示名称
            /// </summary>
            public string showName
            {
                get { return _showName; }
            }

            private string _groupName;
            /// <summary>
            /// 分组名称
            /// </summary>
            public string groupName
            {
                get { return _groupName; }
            }

            private bool _isShow = true;
            /// <summary>
            /// 是否显示该线
            /// </summary>
            public bool isShow
            {
                get { return _isShow; }
            }

            private Color? _lineColor;
            /// <summary>
            /// 线颜色
            /// </summary>
            public Color? lineColor
            {
                get { return _lineColor; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="lineType">线类型</param>
            /// <param name="showName">显示名称</param>
            /// <param name="groupName">分组名称</param>
            /// <param name="lineColor">线颜色（如果为Null，则按颜色列表顺序显示）</param>
            public LineInfoClass(string lineName, LineType lineType, string showName, string groupName, Color? lineColor = null)
            {
                _lineName = lineName;
                _lineType = lineType;
                _showName = showName;
                _groupName = groupName;
                _lineColor = lineColor;
            }

            /// <summary>
            /// 显示该线
            /// </summary>
            public void showLine()
            {
                _isShow = true;
            }

            /// <summary>
            /// 隐藏该线
            /// </summary>
            public void hideLine()
            {
                _isShow = false;
            }
        }

        /// <summary>
        /// 线数据管理类
        /// </summary>
        private class LineDataManager
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public LineDataManager()
            {

            }

            /// <summary>
            /// 线类集合
            /// </summary>
            public List<LineData> listLine = new List<LineData>();

            /// <summary>
            /// 添加线
            /// </summary>
            /// <param name="lineType">线类型</param>
            /// <param name="showName">显示名称</param>
            /// <param name="lineName">线名称</param>
            public void addLine(LineType lineType, string showName, string lineName)
            {
                //判断线类型
                if (lineType == LineType.KLine)
                {
                    listLine.Add(new KLineData(showName, lineName));
                }
                else
                {
                    listLine.Add(new NormalLineData(lineType, showName, lineName));
                }
            }

            /// <summary>
            /// 添加普通线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">数据值</param>
            public void addData(string lineName, string dateTime, float Value)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is NormalLineData && ld.lineName == lineName)
                    {
                        ((NormalLineData)ld).addData(dateTime, Value);
                    }
                }
            }

            /// <summary>
            /// 添加普通线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">数据值</param>
            public void addData(string lineName, string dateTime, int Value)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is NormalLineData && ld.lineName == lineName)
                    {
                        ((NormalLineData)ld).addData(dateTime, Value);
                    }
                }
            }

            /// <summary>
            /// 添加K线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void addKData(string lineName, string dateTime, float Value, float openPrice, float closePrice, float maxPrice, float minPrice)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is KLineData && ld.lineName == lineName)
                    {
                        ((KLineData)ld).addData(dateTime, Value, openPrice, closePrice, maxPrice, minPrice);
                    }
                }
            }

            /// <summary>
            /// 添加K线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void addKData(string lineName, string dateTime, int Value, int openPrice, int closePrice, int maxPrice, int minPrice)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is KLineData && ld.lineName == lineName)
                    {
                        ((KLineData)ld).addData(dateTime, Value, openPrice, closePrice, maxPrice, minPrice);
                    }
                }
            }

            /// <summary>
            /// 修改普通线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">数据值</param>
            public void updateData(string lineName, string dateTime, float Value)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is NormalLineData && ld.lineName == lineName)
                    {
                        ((NormalLineData)ld).updateData(dateTime, Value);
                    }
                }
            }

            /// <summary>
            /// 修改普通线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">数据值</param>
            public void updateData(string lineName, string dateTime, int Value)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is NormalLineData && ld.lineName == lineName)
                    {
                        ((NormalLineData)ld).updateData(dateTime, Value);
                    }
                }
            }

            /// <summary>
            /// 修改K线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void updateKData(string lineName, string dateTime, float Value, float openPrice, float closePrice, float maxPrice, float minPrice)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is KLineData && ld.lineName == lineName)
                    {
                        ((KLineData)ld).updateData(dateTime, Value, openPrice, closePrice, maxPrice, minPrice);
                    }
                }
            }

            /// <summary>
            /// 修改K线数据
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="dateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void updateKData(string lineName, string dateTime, int Value, int openPrice, int closePrice, int maxPrice, int minPrice)
            {
                foreach (LineData ld in listLine)
                {
                    if (ld is KLineData && ld.lineName == lineName)
                    {
                        ((KLineData)ld).updateData(dateTime, Value, openPrice, closePrice, maxPrice, minPrice);
                    }
                }
            }
        }

        /// <summary>
        /// 线数据抽象类
        /// </summary>
        private abstract class LineData
        {
            protected LineType _lineType;
            /// <summary>
            /// 线类型
            /// </summary>
            public LineType lineType
            {
                get { return _lineType; }
            }

            protected string _lineName;
            /// <summary>
            /// 线名称
            /// </summary>
            public string lineName
            {
                get { return _lineName; }
            }

            protected string _showName;
            /// <summary>
            /// 显示名称
            /// </summary>
            public string showName
            {
                get { return _showName; }
            }

            /// <summary>
            /// 线最大值
            /// </summary>
            public string getMax(int startIndex, int endIndex)
            {
                string max = "-";
                //判断是否超出线数据数
                if (endIndex > listData.Count) endIndex = listData.Count;
                //循环求最大值
                for (int i = startIndex; i < endIndex; i++)
                {
                    listData[i].getCompareMax(ref max);
                }
                return max;
            }

            /// <summary>
            /// 线最小值
            /// </summary>
            public string getMin(int startIndex, int endIndex)
            {
                string min = "-";
                //判断是否超出线数据数
                if (endIndex > listData.Count) endIndex = listData.Count;
                //循环求最小值
                for (int i = startIndex; i < endIndex; i++)
                {
                    listData[i].getCompareMin(ref min);
                }
                return min;
            }

            /// <summary>
            /// 线数据列表
            /// </summary>
            public List<LineDataUnit> listData = new List<LineDataUnit>();

            /// <summary>
            /// 获取指定日期的线数据单元
            /// </summary>
            /// <param name="DateTime"></param>
            /// <returns></returns>
            public abstract LineDataUnit getDataUnit(string DateTime);
        }

        /// <summary>
        /// 普通线数据（只包含一个值，该值即为显示值，也为比较值）
        /// </summary> 
        private class NormalLineData : LineData
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="lineType">线类型</param>
            /// <param name="showName">显示名称</param>
            /// <param name="lineName">线名称</param>
            public NormalLineData(LineType lineType, string showName, string lineName)
            {
                _lineType = lineType;
                _showName = showName;
                _lineName = lineName;
            }

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            public void addData(string DateTime, float Value)
            {
                listData.Add(new NormalLineDataUnit(DateTime, Value.ToString()));
            }

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            public void addData(string DateTime, int Value)
            {
                listData.Add(new NormalLineDataUnit(DateTime, Value.ToString()));
            }

            /// <summary>
            /// 修改指定日期时间的数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            public void updateData(string DateTime, float Value)
            {
                LineDataUnit ldu = getDataUnit(DateTime);
                if (ldu == null) return;
                ldu.updateData(Value.ToString());
            }

            /// <summary>
            /// 修改指定日期时间的数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            public void updateData(string DateTime, int Value)
            {
                LineDataUnit ldu = getDataUnit(DateTime);
                if (ldu == null) return;
                ldu.updateData(Value.ToString());
            }

            /// <summary>
            /// 获取指定时间的线数据单元
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <returns></returns>
            public override LineDataUnit getDataUnit(string DateTime)
            {
                foreach (LineDataUnit ldu in listData)
                {
                    if (ldu.dateTime == DateTime)
                    {
                        return ldu;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// K线数据（包含四个值，开盘价，收盘价，最高价，最低价。需设定显示值，最大值与最高价比较，最小值与最低价比较）
        /// </summary>
        private class KLineData : LineData
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="showName">显示名称</param>
            /// <param name="lineName">线名称</param>
            public KLineData(string showName, string lineName)
            {
                _lineType = LineType.KLine;
                _showName = showName;
                _lineName = lineName;
            }

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void addData(string DateTime, float Value, float openPrice, float closePrice, float maxPrice, float minPrice)
            {
                LineDataUnit ldu = new KLineDataUnit(DateTime,
                    Value.ToString(),
                    openPrice.ToString(),
                    closePrice.ToString(),
                    maxPrice.ToString(),
                    minPrice.ToString());
                listData.Add(ldu);
            }

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void addData(string DateTime, int Value, int openPrice, int closePrice, int maxPrice, int minPrice)
            {
                LineDataUnit ldu = new KLineDataUnit(DateTime,
                    Value.ToString(),
                    openPrice.ToString(),
                    closePrice.ToString(),
                    maxPrice.ToString(),
                    minPrice.ToString());
                listData.Add(ldu);
            }

            /// <summary>
            /// 修改指定日期时间的数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void updateData(string DateTime, float Value, float openPrice, float closePrice, float maxPrice, float minPrice)
            {
                LineDataUnit ldu = getDataUnit(DateTime);
                if (ldu == null) return;
                ((KLineDataUnit)ldu).updateData(Value.ToString(),
                    openPrice.ToString(),
                    closePrice.ToString(),
                    maxPrice.ToString(),
                    minPrice.ToString());
            }

            /// <summary>
            /// 修改指定日期时间的数据
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <param name="Value">数据</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void updateData(string DateTime, int Value, int openPrice, int closePrice, int maxPrice, int minPrice)
            {
                LineDataUnit ldu = getDataUnit(DateTime);
                if (ldu == null) return;
                ((KLineDataUnit)ldu).updateData(Value.ToString(),
                    openPrice.ToString(),
                    closePrice.ToString(),
                    maxPrice.ToString(),
                    minPrice.ToString());
            }

            /// <summary>
            /// 获取指定时间的线数据单元
            /// </summary>
            /// <param name="DateTime">日期时间</param>
            /// <returns></returns>
            public override LineDataUnit getDataUnit(string DateTime)
            {
                foreach (LineDataUnit ldu in listData)
                {
                    if (ldu.dateTime == DateTime)
                    {
                        return ldu;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 线数据基本单位抽象类
        /// </summary>
        private abstract class LineDataUnit
        {
            protected string _dateTime;
            /// <summary>
            /// 日期时间
            /// </summary>
            public string dateTime
            {
                get { return _dateTime; }
            }

            protected string _value;
            /// <summary>
            /// 显示数据值
            /// </summary>
            public string value
            {
                get { return _value; }
            }

            /// <summary>
            /// 更新数据
            /// </summary>
            /// <param name="value">更新值</param>
            public virtual void updateData(string value)
            {
                _value = value;
            }

            /// <summary>
            /// 与当前值比较获得最大值
            /// </summary>
            /// <param name="Max">最大值</param>
            public virtual void getCompareMax(ref string Max)
            {
                if (Max == "-") Max = _value;
                else Max = float.Parse(Max) > float.Parse(_value) ? Max : _value;
            }

            /// <summary>
            /// 与当前值比较获得最小值
            /// </summary>
            /// <param name="Min">最小值</param>
            public virtual void getCompareMin(ref string Min)
            {
                if (Min == "-") Min = _value;
                else Min = float.Parse(Min) < float.Parse(_value) ? Min : _value;
            }
        }

        /// <summary>
        /// 线数据基本单位
        /// </summary>
        private class NormalLineDataUnit : LineDataUnit
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="dateTime">日期时间</param>
            /// <param name="value">数据</param>
            public NormalLineDataUnit(string dateTime, string value)
            {
                _dateTime = dateTime;
                _value = value;
            }
        }

        /// <summary>
        /// K线数据基本单位
        /// </summary>  
        private class KLineDataUnit : LineDataUnit
        {
            private string _openPrice;
            /// <summary>
            /// 开盘价
            /// </summary>
            public string openPrice
            {
                get { return _openPrice; }
            }

            protected string _closePrice;
            /// <summary>
            /// 收盘价
            /// </summary>
            public string closePrice
            {
                get { return _closePrice; }
            }

            protected string _maxPrice;
            /// <summary>
            /// 最高价
            /// </summary>
            public string maxPrice
            {
                get { return _maxPrice; }
            }

            protected string _minPrice;
            /// <summary>
            /// 最低价
            /// </summary>
            public string minPrice
            {
                get { return _minPrice; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="dateTime">日期时间</param>
            /// <param name="value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public KLineDataUnit(string dateTime, string value, string openPrice, string closePrice, string maxPrice, string minPrice)
            {
                _dateTime = dateTime;
                _value = value;
                _openPrice = openPrice;
                _closePrice = closePrice;
                _maxPrice = maxPrice;
                _minPrice = minPrice;
            }

            /// <summary>
            /// 更新数据
            /// </summary>
            /// <param name="value">显示数值</param>
            /// <param name="openPrice">开盘价</param>
            /// <param name="closePrice">收盘价</param>
            /// <param name="maxPrice">最高价</param>
            /// <param name="minPrice">最低价</param>
            public void updateData(string value, string openPrice, string closePrice, string maxPrice, string minPrice)
            {
                _value = value;
                _openPrice = openPrice;
                _closePrice = closePrice;
                _maxPrice = maxPrice;
                _minPrice = minPrice;
            }

            /// <summary>
            /// 与当前值比较获得最大值
            /// </summary>
            /// <param name="Max">最大值</param>
            public override void getCompareMax(ref string Max)
            {
                if (Max == "-") Max = _maxPrice;
                else Max = float.Parse(Max) > float.Parse(_maxPrice) ? Max : _maxPrice;
            }

            /// <summary>
            /// 与当前值比较获得最小值
            /// </summary>
            /// <param name="Min">最小值</param>
            public override void getCompareMin(ref string Min)
            {
                if (Min == "-") Min = _minPrice;
                else Min = float.Parse(Min) > float.Parse(_minPrice) ? Min : _minPrice;
            }
        }
    }
}
