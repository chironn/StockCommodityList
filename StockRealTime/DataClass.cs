using System;
using System.Collections.Generic;
using System.Drawing;

namespace GDIChart
{
    partial class StockChart
    {
        /// <summary>
        /// 行情列表列对象
        /// </summary>
        private class ListColumn
        {
            private string _Name;
            private float _Width;
            /// <summary>
            /// 列名称
            /// </summary>
            public string Name
            {
                get { return _Name; }
            }
            /// <summary>
            /// 列宽度
            /// </summary>
            public float Width
            {
                get { return _Width; }
            }

            //列名称及数据对齐方式
            public textAlignStyle TitleAlignStyle = textAlignStyle.Center;
            public textAlignStyle DataAlignStyle = textAlignStyle.Right;
            //数据项比较类型
            public compareType CompareType = compareType.None;

            private float _StartPos = 0;
            /// <summary>
            /// 列起点位置
            /// </summary>
            public float StartPos
            {
                get { return _StartPos; }
            }
            /// <summary>
            /// 列终点位置
            /// </summary>
            public float EndPos
            {
                get { return StartPos + Width; }
            }

            //是否可见
            public bool Visible = true;

            //数据项字体颜色
            public Color dataFontColor = sysColor.Default;

            //数据更新是否改变背景色
            public bool updateDataChangeColor = true;

            //数据是否根据比较值显示涨跌符号
            public bool showCompareSymbol = false;

            //绘制矩形
            private RectangleF rec;

            //标题字符串
            private Bitmap TitleBitmap;

            /// <summary>
            /// 添加一个列
            /// </summary>
            /// <param name="ColumnName">列名称</param>
            /// <param name="CompareType">比较类型</param>
            /// <param name="ColumnWidth">初始列宽</param>
            /// <param name="StartPos">起始位置</param>
            public ListColumn(string ColumnName, compareType CompareType, float ColumnWidth, float StartPos)
            {
                _Name = ColumnName;
                this.CompareType = CompareType;
                setWidth(StartPos, ColumnWidth);

                //绘制完整字符串
                TitleBitmap = new Bitmap((int)staPara.getStringSizeF(ColumnName).Width, (int)staPara.getStringSizeF(ColumnName).Height);
                Graphics gra = Graphics.FromImage(TitleBitmap);
                gra.DrawString(ColumnName, staPara.Font, new SolidBrush(sysColor.Default), new PointF(0, 0));

            }

            /// <summary>
            /// 设置起始位置及列宽
            /// </summary>
            /// <param name="StartPos">起始位置</param>
            /// <param name="ColumnWidth">列宽</param>
            /// <returns>终点位置</returns>
            public float setWidth(float StartPos, float ColumnWidth)
            {
                _Width = ColumnWidth;
                _StartPos = StartPos;

                //设置矩形
                rec = new RectangleF(StartPos + 1, 0, ColumnWidth - 2, staPara.rowHeight);  //避免与行线或列线重复

                return EndPos;
            }

            /// <summary>
            /// 获取指定Y坐标的单位行高、当前列宽矩形
            /// </summary>
            /// <param name="YPos">Y坐标值</param>
            /// <returns>返回一个矩形对象,该矩形是指定Y坐标的单位行高，当前列宽矩形</returns>
            public RectangleF getRectangleF(float YPos)
            {
                rec.Y = YPos;
                return rec;
            }

            /// <summary>
            /// 获取列名称字符串Bitmap
            /// </summary>
            /// <returns></returns>
            public Bitmap getTitleBitmap()
            {
                return TitleBitmap;
            }
        }

        /// <summary>
        /// 商品对象
        /// </summary>
        private class Commodity
        {
            private int _Index = 0;
            private string _Code = "-";
            private string _Name = "-";
            private float _yesterdayClosePrice = 0;

            /// <summary>
            /// 商品序号（记录添加顺序）
            /// </summary>
            public int Index
            {
                get { return _Index; }
            }
            /// <summary>
            /// 商品代码
            /// </summary>
            public string Code
            {
                get { return _Code; }
            }
            /// <summary>
            /// 商品名称
            /// </summary>
            public string Name
            {
                get { return _Name; }
            }
            /// <summary>
            /// 商品昨日收盘价
            /// </summary>
            public float yesterdayClosePrice
            {
                get { return _yesterdayClosePrice; }
            }

            /// <summary>
            /// 行情列表数据
            /// </summary>
            public List<ListSubItems> Items = new List<ListSubItems>();

            /// <summary>
            /// 线管理类
            /// </summary>
            public LineDataManager lineManager = new LineDataManager();

            /// <summary>
            /// 创建一个商品对象
            /// </summary>
            /// <param name="CommodityIndex">商品序号</param>
            /// <param name="ColumnCount">行情列表列数</param>
            /// <param name="index">商品序号(记录添加顺序)</param>
            public Commodity(int CommodityIndex, int ColumnCount, int index)
            {
                //根据列对象创建列存储对象
                for (int i = 0; i < ColumnCount; i++)
                {
                    Items.Add(new ListSubItems(index));
                }

                //设置商品序号
                _Index = index;
            }

            /// <summary>
            /// 设置商品属性（代码、名称及开盘价）
            /// </summary>
            /// <param name="Code">商品代码</param>
            /// <param name="Name">商品名称</param>
            /// <param name="yesterdayClosePrice">昨日收盘价</param>
            public void setAttribute(string Code, string Name, float yesterdayClosePrice)
            {
                _Code = Code;
                _Name = Name;
                _yesterdayClosePrice = yesterdayClosePrice;
                Items[1].Value = Code;
                Items[2].Value = Name;
            }
        }

        /// <summary>
        /// 行情列表数据项
        /// </summary>
        private class ListSubItems
        {
            //商品序号
            public int CommodityIndex;

            //数据值
            public string Value;

            //字体颜色
            public Color FontColor;

            //背景颜色
            public Color BackColor;

            /// <summary>
            /// 创建一个数据项
            /// </summary>
            public ListSubItems(int index)
            {
                CommodityIndex = index;
                Value = "-";
                FontColor = sysColor.Default;
                BackColor = Color.FromArgb(0, 0, 0);
            }

            /// <summary>
            /// 数据更新设置背景色
            /// </summary>
            private void setUpdateColor()
            {
                BackColor = Color.FromArgb(0, 0, 250);
            }

            /// <summary>
            /// 淡化背景色
            /// </summary>
            public void downColor()
            {
                if (BackColor.B != 0) BackColor = Color.FromArgb(0, 0, BackColor.B - 50);
            }

            /// <summary>
            /// 清空颜色
            /// </summary>
            public void ClearColor()
            {
                BackColor = Color.FromArgb(0, 0, 0);
            }

            /// <summary>
            /// 设置数据，如果数据更新则返回true
            /// </summary>
            /// <param name="Value">更新值</param>
            /// <param name="FontColor">字体颜色</param>
            /// <param name="ChangeBackColor">是否更新背景颜色</param>
            /// <returns></returns>
            public bool setValue(string Value, Color FontColor, bool ChangeBackColor)
            {
                if (Value == "")
                {
                    Value = "-";
                }
                //数据相等则忽略
                if (this.Value != Value)
                {
                    this.Value = Value;
                    if (ChangeBackColor) setUpdateColor();       //设置背景更新颜色
                    this.FontColor = FontColor;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 画面对象
        /// </summary>
        private class ChartImage
        {
            /// <summary>
            /// 构造函数(包含时间段对象，则画面不允许放大，将按照时间间隔绘制列线)
            /// </summary>
            /// <param name="imgName">画面名称</param>
            /// <param name="timePeriod">时间段对象</param>
            public ChartImage(string imgName, TimePeriod timePeriod)
            {
                this._timePeriod = timePeriod;
                this._imgName = imgName;
            }

            /// <summary>
            /// 构造函数(不包含时间段对象，则画面允许缩放，不绘制列线)
            /// </summary>
            /// <param name="imgName"></param>
            public ChartImage(string imgName)
            {
                this._imgName = imgName;
                this._timePeriod = null;
            }

            #region 成员列表
            private string _imgName;
            /// <summary>
            /// 画面名称（唯一标识）
            /// </summary>
            public string imgName
            {
                get { return _imgName; }
            }

            private TimePeriod _timePeriod = null;
            /// <summary>
            /// 时间段管理类
            /// </summary>
            public TimePeriod timePeriod
            {
                get { return _timePeriod; }
            }

            /// <summary>
            /// 区域列表
            /// </summary>
            public List<AreaClass> AreaList = new List<AreaClass>();

            /// <summary>
            /// 边框宽度
            /// </summary>
            public int borderWidth;
            public readonly int borderWidthWithDigital = 60;   //带数字的宽度
            public readonly int borderWidthWithOutDigital = 5; //不带数字的宽度
            #endregion

            #region 公共接口API
            /// <summary>
            /// 添加区域
            /// </summary>
            /// <param name="areaName">区域名称</param>
            /// <param name="inMiddle">是否在区域中间显示起始值，默认为false。如果为true，则会在右侧显示百分比，并将商品的昨日收盘价作为中间值</param>
            /// <param name="tagMaxMin">是否标记最高值和最低值</param>
            public void addArea(string areaName, bool inMiddle = false, bool tagMaxMin = false)
            {
                //判断如果在区域中间显示起始值，则设置带百分比的宽度
                if (inMiddle) borderWidth = borderWidthWithDigital;
                else if (borderWidth != borderWidthWithDigital) borderWidth = borderWidthWithOutDigital;

                if (AreaList.Count == 0)
                {
                    AreaList.Add(new AreaClass(areaName, inMiddle, tagMaxMin, 0.65f));
                }
                else
                {
                    //计算调整的高度
                    float heightPercent = 0.35f / AreaList.Count;
                    //重新设置高度
                    for (int i = 1; i < AreaList.Count; i++)
                    {
                        AreaList[i].setHeightPercent(heightPercent);
                    }
                    //添加对象
                    AreaList.Add(new AreaClass(areaName, inMiddle, tagMaxMin, heightPercent));
                }
            }
            #endregion
        }

        /// <summary>
        /// 数据管理类
        /// </summary>
        private class DataManager
        {
            /// <summary>
            /// 商品对象
            /// </summary>
            public List<Commodity> commodity = new List<Commodity>();

            /// <summary>
            /// 行情列表列对象
            /// </summary>
            public List<ListColumn> listColumn = new List<ListColumn>();

            /// <summary>
            /// 画面对象
            /// </summary>
            public List<ChartImage> listImage = new List<ChartImage>();

            #region 对象添加方法
            /// <summary>
            /// 添加一个列
            /// </summary>
            /// <param name="ColumnName">列名称</param>
            /// <param name="ColumnWidth">列宽</param>
            /// <param name="color">数据项颜色</param>
            /// <param name="CompareType">比较方式</param>
            public void addColumn(string ColumnName, float ColumnWidth, compareType CompareType = compareType.None, sysColorType color = sysColorType.Default)
            {
                ListColumn lc;
                //判断最小列宽
                if (ColumnWidth < staPara.minColumnWidth) ColumnWidth = staPara.minColumnWidth;
                if (listColumn.Count == 0)
                {
                    lc = new ListColumn(ColumnName, CompareType, ColumnWidth, 0);
                }
                else
                {
                    lc = new ListColumn(ColumnName, CompareType, ColumnWidth, listColumn[listColumn.Count - 1].EndPos);
                }
                lc.dataFontColor = sysColor.getColor(color);
                listColumn.Add(lc);
            }

            /// <summary>
            /// 添加商品
            /// </summary>
            /// <param name="CommodityName">商品名称</param>
            /// <param name="Code">商品代码</param>
            /// <param name="yesterdayClosePrice">昨日收盘价</param>
            public void addCommodity(string Code, string CommodityName, float yesterdayClosePrice)
            {
                //创建商品对象
                Commodity com = new Commodity(commodity.Count + 1, listColumn.Count, commodity.Count);
                //设置商品属性
                com.setAttribute(Code, CommodityName, (float)Math.Round(yesterdayClosePrice, 2));
                //根据线信息添加线
                foreach (ChartImage ci in listImage)
                {
                    foreach (AreaClass ac in ci.AreaList)
                    {
                        foreach (LineInfoClass lic in ac.listLineInfo)
                        {
                            com.lineManager.addLine(lic.lineType, lic.showName, lic.lineName);
                        }
                    }
                }

                commodity.Add(com);
            }

            /// <summary>
            /// 添加一个包含时间段的画面
            /// </summary>
            /// <param name="imgName">画面名称</param>
            /// <param name="timePeriod">时间段对象</param>
            public void addImage(string imgName, TimePeriod timePeriod)
            {
                listImage.Add(new ChartImage(imgName, timePeriod));
            }

            /// <summary>
            /// 添加一个不包含时间段的画面
            /// </summary>
            /// <param name="imgName">画面名称</param>
            public void addImage(string imgName)
            {
                listImage.Add(new ChartImage(imgName));
            }

            /// <summary>
            /// 在最后添加的画面添加一个区域
            /// </summary>
            /// <param name="areaName">区域名称</param>
            /// <param name="inMiddle">是否在区域中间显示起始值，默认为false。如果为true，则会在右侧显示百分比，并将商品的昨日收盘价作为中间值</param>
            /// <param name="tagMaxMin">是否标记最高值和最低值</param>
            public void addArea(string areaName, bool inMiddle = false, bool tagMaxMin = false)
            {
                ChartImage ci = listImage[listImage.Count - 1];
                ci.addArea(areaName, inMiddle, tagMaxMin);
            }

            /// <summary>
            /// 在最后添加的区域里添加线信息
            /// </summary>
            /// <param name="lineName">线名称</param>
            /// <param name="lineType">线类型</param>
            /// <param name="showName">显示名称</param>
            /// <param name="groupName">分组名称</param>
            /// <param name="lineColor">线颜色（如果为Null，则按颜色列表顺序显示）</param>
            public void addLine(string lineName, LineType lineType, string showName, string groupName, Color? lineColor = null)
            {
                //获取最后一个画面
                ChartImage ci = listImage[listImage.Count - 1];
                //获取上述画面的最后一个区域
                AreaClass ac = ci.AreaList[ci.AreaList.Count - 1];
                ac.addLineInfo(lineName, lineType, showName, groupName, lineColor);
            }
            #endregion

            #region 对象获取方法            
            /// <summary>
            /// 获取列对象
            /// </summary>
            /// <param name="columnName">列名称</param>
            /// <returns></returns>
            public ListColumn getListColumn(string columnName)
            {
                foreach (ListColumn lc in listColumn)
                {
                    if (lc.Name == columnName)
                    {
                        return lc;
                    }
                }
                return null;
            }

            /// <summary>
            /// 获取商品对象
            /// </summary>
            /// <param name="commodityName">商品名称</param>
            /// <returns></returns>
            public Commodity getCommodity(string commodityName)
            {
                foreach (Commodity com in commodity)
                {
                    if (com.Name == commodityName)
                    {
                        return com;
                    }
                }
                return null;
            }

            /// <summary>
            /// 获取画面对象
            /// </summary>
            /// <param name="imgName"></param>
            /// <returns></returns>
            public ChartImage getImage(string imgName)
            {
                foreach (ChartImage ci in listImage)
                {
                    if (ci.imgName == imgName)
                    {
                        return ci;
                    }
                }
                return null;
            }

            /// <summary>
            /// 获取列对象序号
            /// </summary>
            /// <param name="columnName"></param>
            /// <returns></returns>
            public int getListColumnIndex(string columnName)
            {
                return listColumn.IndexOf(getListColumn(columnName));
            }

            /// <summary>
            /// 获取商品对象序号
            /// </summary>
            /// <param name="commodityName"></param>
            /// <returns></returns>
            public int getCommodityIndex(string commodityName)
            {
                return commodity.IndexOf(getCommodity(commodityName));
            }
            #endregion

            #region 修改对象属性方法
            /// <summary>
            /// 更改数据对齐方式
            /// </summary>
            /// <param name="columnName">列名称</param>
            /// <param name="dataAlginStyle">对齐方式</param>
            public void setAlignStyle(string columnName, textAlignStyle dataAlginStyle)
            {
                ListColumn l = getListColumn(columnName);
                if (l != null) l.DataAlignStyle = dataAlginStyle;
            }

            /// <summary>
            /// 设置数据更新不改变背景色
            /// </summary>
            /// <param name="columnName"></param>
            public void setNoUpdateBackColor(string columnName)
            {
                ListColumn l = getListColumn(columnName);
                if (l != null) l.updateDataChangeColor = false;
            }

            /// <summary>
            /// 设置根据比较值显示涨跌符号（即不显示正负号，改为上下箭头）
            /// </summary>
            /// <param name="columnName"></param>
            public void setCompareSymbol(string columnName)
            {
                ListColumn l = getListColumn(columnName);
                if (l != null) l.showCompareSymbol = true;
            }
            #endregion

            #region 数据更新方法
            /// <summary>
            /// 更新行情列表数据
            /// </summary>
            /// <param name="columnName">列名称</param>
            /// <param name="commodityName">商品名称</param>
            /// <param name="value">更新值</param>
            /// <returns>返回是否更新成功</returns>
            public bool UpdateData(string columnName, string commodityName, string value)
            {
                //获取对象
                ListColumn lc = getListColumn(columnName);
                Commodity com = getCommodity(commodityName);
                if (lc == null || com == null) return false;

                //设置数据
                return com.Items[listColumn.IndexOf(lc)].setValue(value, getFontColor(lc, com, value), lc.updateDataChangeColor);
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
                getCommodity(commodityName).lineManager.addData(lineName, dateTime, value);
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
                getCommodity(commodityName).lineManager.addData(lineName, dateTime, value);
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
                getCommodity(commodityName).lineManager.addKData(lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
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
                getCommodity(commodityName).lineManager.addKData(lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
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
                getCommodity(commodityName).lineManager.updateData(lineName, dateTime, value);
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
                getCommodity(commodityName).lineManager.updateData(lineName, dateTime, value);
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
                getCommodity(commodityName).lineManager.updateKData(lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
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
                getCommodity(commodityName).lineManager.updateKData(lineName, dateTime, value, openPrice, closePrice, maxPrice, minPrice);
            }

            #endregion

            #region 数据获取方法
            /// <summary>
            /// 获取数据项字体颜色
            /// </summary>
            /// <param name="col">列对象</param>
            /// <param name="com">商品对象</param>
            /// <param name="Value">数据项值</param>
            /// <returns></returns>
            public Color getFontColor(ListColumn col, Commodity com, string Value)
            {
                compareResultType temp = getComapreResult(col, com, Value);
                switch (temp)
                {
                    case compareResultType.Equal:
                        return sysColor.Gray;
                    case compareResultType.Less:
                        return sysColor.Green;
                    case compareResultType.More:
                        return sysColor.Red;
                    default:
                        return sysColor.Default;
                }
            }

            /// <summary>
            /// 获取比较结果
            /// </summary>
            /// <param name="col">列对象</param> 
            /// <param name="com">商品对象</param>
            /// <param name="Value">数据项值</param>
            /// <returns></returns>
            public compareResultType getComapreResult(ListColumn col, Commodity com, string Value)
            {
                float compareValue;
                //设置数据项颜色
                switch (col.CompareType)
                {
                    case compareType.compareWithValue:
                        {
                            compareValue = com.yesterdayClosePrice;
                            break;
                        }
                    case compareType.compareWith0:
                        {
                            compareValue = 0;
                            break;
                        }
                    default:
                        {
                            return compareResultType.noComapre;
                        }
                }

                if (Value == "-")
                {
                    return compareResultType.Error;
                }
                else
                {
                    float ParseValue;
                    //尝试转换，如果失败
                    if (float.TryParse(Value, out ParseValue) == false)
                    {
                        return compareResultType.Error;
                    }
                    else
                    {
                        if (ParseValue > compareValue)
                        {
                            return compareResultType.More;
                        }
                        else if (ParseValue == compareValue)
                        {
                            return compareResultType.Equal;
                        }
                        else
                        {
                            return compareResultType.Less;
                        }
                    }
                }
            }
            #endregion
        }
    }
}
