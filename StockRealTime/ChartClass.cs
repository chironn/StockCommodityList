using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace GDIChart
{
    partial class StockChart
    {
        /// <summary>
        /// 分时走势图表类
        /// </summary>
        private class ChartClass
        {
            //UI更新委托
            private delegate void UpdateUI();

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="pic">用于显示的PictureBox对象</param>
            /// <param name="dataManager">数据管理对象</param>
            public ChartClass(PictureBox pic, DataManager dataManager)
            {
                this.pic = pic;
                this.dataManager = dataManager;

                //设置默认画面
                image = null;
                //设置默认商品
                commodity = null;
                //初始化颜色数组和画笔
                lineColor = new Color[6];
                lineColor[0] = Color.FromArgb(255, 255, 255);   //白色
                lineColor[1] = Color.FromArgb(255, 255, 0);     //黄色
                lineColor[2] = Color.FromArgb(255, 0, 255);     //紫色
                lineColor[3] = Color.FromArgb(0, 255, 0);       //绿色
                lineColor[4] = Color.FromArgb(192, 192, 192);   //灰色
                lineColor[5] = Color.FromArgb(0, 0, 255);       //蓝色
            }

            #region 成员列表
            private PictureBox pic; //用于显示的PictureBox对象
            private DataManager dataManager; //数据管理对象
            private bool isShowed = false;  //是否可见

            private int startIndex; //起始显示序号
            private int endIndex;   //终止显示序号

            private ChartImage image;       //当前绘画画面
            private Commodity commodity;    //当前绘画商品

            private float InfoAreaWidth = 10;  //信息区域宽度

            private UpdateUI drawDelegate;  //更新委托方法            
            private Thread t_BackColorChange = null;//更新数据线程

            private Color[] lineColor;  //线颜色数组

            //图形绘画
            Graphics gra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap bit;

            //鼠标操作相关绘画
            Graphics MouseGra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap MouseBit;

            //操作标志线绘画
            Graphics ColumnTagGra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap ColumnTagBit;
            #endregion

            #region 事件列表
            /// <summary>
            /// 绑定事件
            /// </summary>
            private void bindEvent()
            {
                pic.SizeChanged += new EventHandler(ESizeChanged);
                pic.MouseMove += new MouseEventHandler(EMouseMove);
                pic.MouseDown += new MouseEventHandler(EMouseDown);
                pic.MouseUp += new MouseEventHandler(EMouseUp);
                pic.PreviewKeyDown += new PreviewKeyDownEventHandler(EKeyPress);
                pic.MouseWheel += new MouseEventHandler(EMouseWheel);
            }

            /// <summary>
            /// 解绑事件
            /// </summary>
            private void unbindEvent()
            {
                pic.SizeChanged -= new EventHandler(ESizeChanged);
                pic.MouseMove -= new MouseEventHandler(EMouseMove);
                pic.MouseDown -= new MouseEventHandler(EMouseDown);
                pic.MouseUp -= new MouseEventHandler(EMouseUp);
                pic.PreviewKeyDown -= new PreviewKeyDownEventHandler(EKeyPress);
                pic.MouseWheel -= new MouseEventHandler(EMouseWheel);
            }

            //事件鼠标抬起
            private void EMouseUp(object sender, MouseEventArgs e)
            {
            }

            //事件鼠标按下
            private void EMouseDown(object sender, MouseEventArgs e)
            {

            }

            //事件鼠标移动
            private void EMouseMove(object sender, MouseEventArgs e)
            {

            }

            //事件控件大小改变
            private void ESizeChanged(object sender, EventArgs e)
            {
                //rowStartIndex = 0;
                Draw(drawType.redraw);
            }

            //事件键盘按键
            private void EKeyPress(object sender, PreviewKeyDownEventArgs e)
            {

            }

            //事件鼠标滚轮
            private void EMouseWheel(object sender, MouseEventArgs e)
            {
            }
            #endregion

            #region 绘画方法
            //绘画总方法
            private void Draw(drawType DrawType)
            {
                //委托控件执行，避免多线程调用锁死
                drawDelegate = delegate
                {
                    DrawDelegate(DrawType);
                };
                pic.Invoke(drawDelegate);
            }

            //绘画委托
            private void DrawDelegate(drawType DrawType)
            {
                //如果不显示 则退出方法
                if (isShowed == false) return;
                //如果宽或高 == 0 则退出方法
                if (pic.Width == 0 || pic.Height == 0) return;
                //如果当前绘制商品不存在，则退出方法
                if (commodity == null) return;
                //如果当前绘制画面不存在，则退出方法
                if (image == null) return;

                switch (DrawType)
                {
                    case drawType.redraw:
                        {
                            init();
                            drawAll();
                            break;
                        }
                }

                //显示绘画数据
                pic.BackgroundImage = bit;
                pic.Image = MouseBit;

                //垃圾回收
                GC.Collect();
            }

            //清空绘画对象
            private void Clear()
            {
                //数据层
                gra.Clear(Color.Transparent);
                //鼠标操作层
                MouseGra.Clear(Color.Transparent);
                //鼠标标记层
                ColumnTagGra.Clear(Color.Transparent);
            }

            //初始化画布
            private void init()
            {
                //数据层
                bit = new Bitmap(pic.Width, pic.Height);
                gra = Graphics.FromImage(bit);
                gra.Clear(Color.Transparent);

                //鼠标操作层
                MouseBit = new Bitmap(pic.Width, pic.Height);
                MouseGra = Graphics.FromImage(MouseBit);
                MouseGra.Clear(Color.Transparent);

                //操作标志层
                ColumnTagBit = new Bitmap(pic.Width, pic.Height);
                ColumnTagGra = Graphics.FromImage(ColumnTagBit);
                ColumnTagGra.Clear(Color.Transparent);
            }

            //全部重画
            private void drawAll()
            {
                //绘制信息区域分割线
                gra.DrawLine(Pens.Red, new PointF(InfoAreaPos, 0), new PointF(InfoAreaPos, pic.Height));

                //绘画两侧垂直边界线
                gra.DrawLine(Pens.Red, new PointF(leftBorderPos, 0), new PointF(leftBorderPos, pic.Height));
                gra.DrawLine(new Pen(Color.Red, 2), new PointF(rightBorderPos, 0), new PointF(rightBorderPos, pic.Height));

                //区域绘制起点
                PointF startPos = new PointF(leftBorderPos, 0);
                //绘制区域
                foreach (AreaClass ac in image.AreaList)
                {
                    drawArea(ac, startPos);
                    startPos.Y += getAreaHeight(ac.heightPercent);
                }
            }

            //绘画区域布局
            private void drawArea(AreaClass Area, PointF StartPos)
            {
                //区域高度
                float areaHeight = getAreaHeight(Area.heightPercent);
                //区域有效高度（不包括区域名称的高度）
                float areaDrawHeight = areaHeight - staPara.rowChartHeight;
                //区域有效起始点Y坐标（不包括区域名称的高度）
                float areaDrawStartYPos = StartPos.Y + staPara.rowChartHeight;
                //----------------------------------------------------------(经常使用的计算变量)

                //绘制区域外框
                gra.DrawRectangle(Pens.Red,
                    StartPos.X,
                    areaDrawStartYPos,
                    areaWidth,
                    areaDrawHeight);//减去区域名称的高度

                //绘制区域分割线（区域下方，与下一个区域分割）
                gra.DrawLine(Pens.Red, new PointF(0, areaDrawStartYPos + areaDrawHeight), new PointF(InfoAreaPos, areaDrawStartYPos + areaDrawHeight));

                #region 绘制区域标题（商品名称、区域名称及所有线名称）
                //绘制点
                PointF printPos = new PointF(StartPos.X, StartPos.Y);
                //显示区域名称，如果是第一个区域则显示商品名称
                if (image.AreaList.IndexOf(Area) == 0)
                {
                    drawTitle(commodity.Name, sysColor.Gray, ref printPos);
                }
                else
                {
                    drawTitle(Area.areaName, sysColor.Gray, ref printPos);
                }
                //绘制线名称
                int lineColorIndex = 0; //线颜色序号
                foreach (LineInfoClass lic in Area.listLineInfo)
                {
                    foreach (LineData ld in commodity.lineManager.listLine)
                    {
                        if (lic.lineName == ld.lineName && lic.isShow)
                        {
                            //如果是K线则名称显示为红色
                            if (ld.lineType == LineType.KLine)
                            {
                                drawTitle(lic.showName, sysColor.Red, ref printPos);
                            }
                            //如果线本身有颜色，则显示线本身的颜色
                            else if (lic.lineColor != null)
                            {
                                drawTitle(lic.showName, (Color)lic.lineColor, ref printPos);
                            }
                            //否则按线颜色数组顺序指定颜色
                            else
                            {
                                drawTitle(lic.showName, lineColor[lineColorIndex++ % lineColor.Length], ref printPos);
                            }
                        }
                    }
                }
                #endregion
                #region 计算区域最大值和最小值
                string maxValue = "-";
                string minValue = "-";
                //在商品线数据列表中匹配区域线信息列表，并获取每条线的最大值，得到所有线的最大值
                foreach (LineInfoClass lic in Area.listLineInfo)
                {
                    foreach (LineData ld in commodity.lineManager.listLine)
                    {
                        if (lic.lineName == ld.lineName && lic.isShow)
                        {
                            string maxTemp = ld.getMax(startIndex, endIndex);
                            string minTemp = ld.getMin(startIndex, endIndex);
                            if (maxTemp != "-")
                            {
                                if (maxValue == "-")
                                {
                                    maxValue = maxTemp;
                                }
                                else
                                {
                                    maxValue = float.Parse(maxTemp) > float.Parse(maxValue) ? maxTemp : maxValue;
                                }
                            }
                            if (minTemp != "-")
                            {
                                if (minValue == "-")
                                {
                                    minValue = minTemp;
                                }
                                else
                                {
                                    minValue = float.Parse(minTemp) < float.Parse(minValue) ? minTemp : minValue;
                                }
                            }
                        }
                    }
                }
                //如果起始行在中间，需要以昨收价为中间值，对称最大值和最小值
                if (Area.inMiddle)
                {
                    //获取昨日收盘价
                    float startFloat = commodity.yesterdayClosePrice;

                    //如果最大值和最小值不存在，则默认为开盘价的5%
                    if (maxValue == "-" || minValue == "-")
                    {
                        maxValue = (startFloat * 1.05f).ToString();
                        minValue = (startFloat * 0.95f).ToString();
                    }

                    //临时转换为float用于计算
                    float max = float.Parse(maxValue);
                    float min = float.Parse(minValue);

                    //如果最高值减中间值，大于中间值减最小值，则使中间值减最小值等于最高值减中间值。
                    //如果最高值减中间值，小于中间值减最小值，则使最高值减中间值等于中间值减最小值。
                    if (max - startFloat > startFloat - min)
                    {
                        minValue = (startFloat - (max - startFloat)).ToString();
                    }
                    else if (max - startFloat < startFloat - min)
                    {
                        maxValue = (startFloat + (startFloat - min)).ToString();
                    }
                }
                else
                {
                    //如果最大值和最小值不存在，则默认为0
                    if (maxValue == "-" || minValue == "-")
                    {
                        maxValue = "0";
                        minValue = "0";
                    }
                }

                //临时记录最大值最小值在区域对象中，用于新增数据判断是否超出区域范围
                Area.maxValue = float.Parse(maxValue);
                Area.minValue = float.Parse(minValue);
                #endregion
                #region 绘制行线
                //计算行数
                int rowCount = (int)(areaDrawHeight / staPara.rowMinSpacing);
                //如果起始行在中间，则使行数为偶数
                if (Area.inMiddle && rowCount % 2 != 0) rowCount++;
                //计算行间隔
                float rowSpacing = areaDrawHeight / rowCount;

                //绘制点
                PointF leftPoint = new PointF(leftBorderPos, 0);
                PointF rightPoint = new PointF(rightBorderPos, 0);
                //绘制行线
                for (int i = 0; i < rowCount; i++)
                {
                    //计算坐标Y值
                    rightPoint.Y = leftPoint.Y = areaDrawStartYPos + i * rowSpacing;

                    //判断是否为中间线
                    if (Area.inMiddle && i == rowCount / 2)
                    {
                        //绘制粗线条
                        gra.DrawLine(new Pen(Color.Red, 2), leftPoint, rightPoint);
                    }
                    else
                    {
                        //绘制普通线条
                        gra.DrawLine(Pens.DarkRed, leftPoint, rightPoint);
                    }
                }
                #endregion
                #region 绘制左数值标签和右侧百分比标签（如果存在）
                //画笔颜色
                SolidBrush solBrush;
                //使字体垂直居中，求出字体高度的一半
                float fontHalfHeight = staPara.rowChartHeight / 2.0f;
                //绘制矩形
                RectangleF recLeft = new RectangleF(0, 0, leftBorderPos, staPara.rowChartHeight);
                RectangleF recRight = new RectangleF(rightBorderPos, 0, leftBorderPos, staPara.rowChartHeight);
                //绘制字体格式(垂直居中)
                StringFormat sfLeft = new StringFormat();
                sfLeft.LineAlignment = StringAlignment.Center;
                sfLeft.Alignment = StringAlignment.Far;
                sfLeft.FormatFlags = StringFormatFlags.LineLimit;
                StringFormat sfRight = new StringFormat();
                sfRight.LineAlignment = StringAlignment.Center;
                sfRight.FormatFlags = StringFormatFlags.LineLimit;
                //临时转换为float用于计算
                float maxFloat = float.Parse(maxValue);
                float minFloat = float.Parse(minValue);
                //每条线之间的数值差
                float valueOfLine = (maxFloat - minFloat) / (float)rowCount;
                //绘制数值字符串及浮点数
                string tagString;
                float tagFloat;
                //百分比数值
                float percentFloat;
                //绘制标签
                if (Area.inMiddle)
                {
                    for (int i = 0; i < rowCount; i++)
                    {
                        //计算坐标Y值（使字体垂直居中，减去字体高度的一半）
                        recRight.Y = recLeft.Y = areaDrawStartYPos + i * rowSpacing - fontHalfHeight;

                        if (i < rowCount / 2)
                        {
                            //如果在中间行上方，则显示红色
                            solBrush = sysColor.sRed;
                        }
                        else if (i == rowCount / 2)
                        {
                            //如果是中间行，则显示白色
                            solBrush = new SolidBrush(Color.White);
                        }
                        else
                        {
                            //如果在中间行下方，则显示绿色
                            solBrush = sysColor.sGreen;
                        }

                        //获取数值字符串(替换小数点)
                        tagFloat = (maxFloat - valueOfLine * i);
                        tagString = tagFloat.ToString("0.00").Replace(".", "");

                        //左侧绘制标签（小数部分绘制下划线）
                        gra.DrawString(tagString.Substring(0, tagString.Length - 2) , staPara.ChartFont, solBrush, recLeft, sfLeft);
                        //gra.DrawString(tagString.Substring(0, tagString.Length - 2) + "__", staPara.ChartFont, solBrush, recLeft, sfLeft);
                        //gra.DrawString(tagString.Substring(tagString.Length - 2, 2), staPara.ChartFont, solBrush, recLeft, sfLeft);

                        //右侧绘制百分比
                        percentFloat = ((tagFloat / commodity.yesterdayClosePrice - 1) * 100);
                        gra.DrawString(percentFloat.ToString("0.0") + "%", staPara.ChartFont, solBrush, recRight, sfRight);

                        //绘制矩形(测试，确定边框)
                        //gra.DrawRectangle(Pens.Green, rec.X, rec.Y, rec.Width, rec.Height);
                    }
                }
                else
                {
                    //设置为红色
                    solBrush = sysColor.sRed;
                    //判断是否是整数（如果最高值和最低值都不包含小数点，则为整数，否则为小数）
                    bool isInt = false;
                    if (maxValue.IndexOf(".") < 0 && minValue.IndexOf(".") < 0) isInt = true;

                    for (int i = 0; i < rowCount; i++)
                    {
                        //计算坐标Y值（使字体垂直居中，减去字体高度的一半）
                        recRight.Y = recLeft.Y = areaDrawStartYPos + i * rowSpacing - fontHalfHeight;

                        //左侧绘制标签
                        if (isInt)
                        {
                            //获取数值字符串
                            tagFloat = (maxFloat - (int)valueOfLine * i);
                            tagString = ((int)tagFloat).ToString();
                            gra.DrawString(tagString, staPara.ChartFont, solBrush, recLeft, sfLeft);
                        }
                        else
                        {
                            //获取数值字符串
                            tagFloat = (maxFloat - valueOfLine * i);
                            tagString = tagFloat.ToString("0.00").Replace(".", "");
                            gra.DrawString(tagString.Substring(0, tagString.Length - 2) , staPara.ChartFont, solBrush, recLeft, sfLeft);
                            //gra.DrawString(tagString.Substring(0, tagString.Length - 2) + "__", staPara.ChartFont, solBrush, recLeft, sfLeft);
                            //gra.DrawString(tagString.Substring(tagString.Length - 2, 2), staPara.ChartFont, solBrush, recLeft, sfLeft);
                        }

                        //绘制矩形(测试，确定边框)
                        //gra.DrawRectangle(Pens.Green, rec.X, rec.Y, rec.Width, rec.Height);
                    }
                }
                #endregion
                #region 绘制列线及时间标签
                if (image.timePeriod != null)
                {
                    //绘制点
                    PointF topPoint = new PointF(0, areaDrawStartYPos);
                    PointF bottomPoint = new PointF(0, areaDrawStartYPos + areaDrawHeight);
                    PointF tagPoint = new PointF(0, areaDrawStartYPos + areaDrawHeight);
                    //绘制个数
                    int columnCount = image.timePeriod.getMinuteCount() / image.timePeriod.timeSpacing;
                    //列线间隔
                    float columnSpacing = areaWidth / ((float)(image.timePeriod.getMinuteCount() - 1) / (float)image.timePeriod.timeSpacing);
                    //笔对象
                    Pen pen = new Pen(Color.Red);
                    //是否是最后一个区域
                    bool lastArea = (image.AreaList.IndexOf(Area) == image.AreaList.Count - 1);

                    //绘制列线
                    for (int i = 1; i < columnCount; i++)
                    {
                        for (int j = 1; j < columnCount*2; j++)
                        {
                            //计算坐标X值
                            bottomPoint.X = topPoint.X = leftBorderPos + j * columnSpacing/2;
                            tagPoint.X = (leftBorderPos + j * columnSpacing - staPara.getChartStringSizeF("00").Width)/2;   //标签绘制点应居中， 减去两位数字的宽度
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;   //绘制虚线
                            gra.DrawLine(pen, topPoint, bottomPoint);
                            j++;
                        }
                        //计算坐标X值
                        bottomPoint.X = topPoint.X = leftBorderPos + i * columnSpacing;
                        tagPoint.X = leftBorderPos + i * columnSpacing - staPara.getChartStringSizeF("00").Width;   //标签绘制点应居中， 减去两位数字的宽度
                        //判断时间点是否为间隔点
                        if (image.timePeriod.strPoint.IndexOf(image.timePeriod.str[i * image.timePeriod.timeSpacing]) != -1)
                        {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;   //绘制实线
                        }
                        else
                        {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;    //绘制虚线
                        }
                        
                        //如果是最后一个区域，则绘制时间标签
                        if (lastArea)
                        {
                            gra.DrawString(image.timePeriod.str[i * image.timePeriod.timeSpacing], staPara.ChartFont, sysColor.sGray, tagPoint);
                        }
                        gra.DrawLine(pen, topPoint, bottomPoint);
                    }

                    //如果是最后一个区域，则绘制时间标签（第一个点和最后一个点）
                    if (lastArea)
                    {
                        //第一个时间点，包含日期
                        gra.DrawString(DateTime.Now.ToShortDateString().Remove(0,5) + " " + image.timePeriod.str[0], staPara.ChartFont, sysColor.sGray, new PointF(0, areaDrawStartYPos + areaDrawHeight));
                        //Console.WriteLine(DateTime.Now.ToShortDateString().Remove(0, 5));
                        //根据边框情况，选择最后一个点的位置
                        if (image.borderWidth == image.borderWidthWithOutDigital)
                        {
                            tagPoint.X = leftBorderPos + columnCount * columnSpacing  - staPara.getChartStringSizeF("00:00").Width;   //标签绘制点应右对齐， 减去所有字符宽度
                        }
                        else
                        {
                            tagPoint.X = leftBorderPos + columnCount * columnSpacing - staPara.getChartStringSizeF("00").Width;   //标签绘制点应居中， 减去两位数字的宽度
                        }
                        gra.DrawString(image.timePeriod.str[columnCount * image.timePeriod.timeSpacing], staPara.ChartFont, sysColor.sGray, tagPoint);
                    }
                }
                #endregion
                #region 绘制区域内的线
                //定义绘制区域的矩形
                RectangleF recAreaDrawingLine = new RectangleF(leftBorderPos, areaDrawStartYPos, areaWidth, areaDrawHeight);
                //重置颜色序号
                lineColorIndex = 0;
                foreach (LineInfoClass lic in Area.listLineInfo)
                {
                    foreach (LineData ld in commodity.lineManager.listLine)
                    {
                        if (lic.lineName == ld.lineName && lic.isShow)
                        {
                            //如果是K线则名称显示为红色
                            if (ld.lineType == LineType.KLine)
                            {
                                drawLine(ld, recAreaDrawingLine, sysColor.Red, Area.maxValue, Area.minValue);
                            }
                            //如果线本身有颜色，则显示线本身的颜色
                            else if (lic.lineColor != null)
                            {
                                drawLine(ld, recAreaDrawingLine, (Color)lic.lineColor, Area.maxValue, Area.minValue);
                            }
                            //否则按线颜色数组顺序指定颜色
                            else
                            {
                                drawLine(ld, recAreaDrawingLine, lineColor[lineColorIndex++ % lineColor.Length], Area.maxValue, Area.minValue);
                            }
                        }
                    }
                }
                #endregion
            }

            /// <summary>
            /// 画线总方法
            /// </summary>
            /// <param name="ld">线数据对象</param>
            /// <param name="rec">绘制区域矩形</param>
            /// <param name="lineColor">线颜色</param>
            /// <param name="areaMaxValue">区域最大值</param>
            /// <param name="areaMinValue">区域最小值</param>
            private void drawLine(LineData ld, RectangleF rec, Color lineColor, float areaMaxValue, float areaMinValue)
            {
                if (ld.lineType == LineType.BrokenLine)
                {
                    drawBrokenLine((NormalLineData)ld, rec, lineColor, areaMaxValue, areaMinValue);
                }
            }

            /// <summary>
            /// 画折线
            /// </summary>
            /// <param name="nld">普通线数据对象</param>
            /// <param name="rec">绘制区域矩形</param>
            /// <param name="lineColor">线颜色</param>
            /// <param name="areaMaxValue">区域最大值</param>
            /// <param name="areaMinValue">区域最小值</param>
            private void drawBrokenLine(NormalLineData nld, RectangleF rec, Color lineColor, float areaMaxValue, float areaMinValue)
            {
                //如果数据不存在则退出绘制过程
                if (nld.listData.Count == 0) return;

                //生成画笔
                Pen pen = new Pen(lineColor);
                //计算点间隔(间隔数比数据数少1)
                float pointSpacing = rec.Width / (float)(endIndex - startIndex - 1);
                //绘制点
                PointF drawPointS = new PointF();
                PointF drawPointE = new PointF();
                //上一数据值记录
                float lastValue = 0;
                //绘制数据计数
                int dataCount = startIndex;
                //找到第一个非空的数
                while (nld.listData[dataCount++].value == "-" && dataCount != nld.listData.Count) ;
                dataCount--;
                //得到第一个数据值
                lastValue = float.Parse(nld.listData[dataCount].value);
                drawPointS.X = rec.X + dataCount * pointSpacing;
                drawPointS.Y = rec.Y + getDrawHeightOffset(rec.Height, areaMaxValue, areaMinValue, lastValue);

                for (int i = dataCount; i < endIndex; i++)
                {
                    if (i == nld.listData.Count) break;
                    //计算绘制点坐标
                    drawPointE.X = rec.X + i * pointSpacing;
                    drawPointE.Y = rec.Y + getDrawHeightOffset(rec.Height, areaMaxValue, areaMinValue, float.Parse(nld.listData[i].value));
                    //绘制线
                    gra.DrawLine(pen, drawPointS, drawPointE);
                    ////数据点矩形
                    //if (i % 10 == 0)
                    //{
                    //    gra.FillRectangle(Brushes.BlueViolet, drawPointS.X - 3, drawPointS.Y - 3, 6, 6);
                    //    gra.FillRectangle(Brushes.BlueViolet, drawPointE.X - 3, drawPointE.Y - 3, 6, 6);
                    //}
                    //记录当前点坐标
                    drawPointS.X = drawPointE.X;
                    drawPointS.Y = drawPointE.Y;
                }
            }

            /// <summary>
            /// 绘画区域标题
            /// </summary>
            /// <param name="str">绘制字符串</param>
            /// <param name="fontColor">字体颜色</param>
            /// <param name="printPos">绘制位置（绘制完成后将自动改变为下一个绘制点）</param>
            private void drawTitle(string str, Color fontColor, ref PointF printPos)
            {
                //绘画字体
                gra.DrawString(str, staPara.ChartFont, new SolidBrush(fontColor), printPos);
                //计算字体宽度并修改绘制点坐标（+3像素点的间隔）
                printPos.X += staPara.getChartStringSizeF(str).Width;
            }
            #endregion

            #region 计算方法
            /// <summary>
            /// 启动更新线程（如果正在运行则忽略，否则创建一个新的线程并启动）
            /// </summary>
            private void startUpdateThread()
            {
                if (isShowed == false) return;
                if (t_BackColorChange == null || t_BackColorChange.ThreadState == ThreadState.Stopped)
                {
                    t_BackColorChange = new Thread(new ThreadStart(thread_UpdateDataChangeBackColor));
                    t_BackColorChange.Start();
                }
            }

            /// <summary>
            /// 获取指定百分比的区域高度(不包含区域下方的标签)
            /// </summary>
            /// <param name="areaHieghtPercent">高度百分比</param>
            /// <returns></returns>
            private float getAreaHeight(float areaHieghtPercent)
            {
                //返回(控件高度-下方标签高度)*区域百分比
                return areaHieghtPercent * ((float)pic.Height - staPara.rowChartHeight);
            }

            /// <summary>
            /// 右侧边框X坐标值
            /// </summary>
            private float rightBorderPos
            {
                get { return pic.Width - InfoAreaWidth - image.borderWidth; }//InfoAreaWidth
            }

            /// <summary>
            /// 左侧边框X坐标值
            /// </summary>
            private float leftBorderPos
            {
                get { return image.borderWidthWithDigital; }
            }

            /// <summary>
            /// 绘制区域宽
            /// </summary>
            private float areaWidth
            {
                get { return rightBorderPos - leftBorderPos; }
            }

            /// <summary>
            /// 信息区域边框坐标值(左侧)
            /// </summary>
            private float InfoAreaPos
            {
                get { return pic.Width - InfoAreaWidth; }
            }

            /// <summary>
            /// 重设数据序号（如果画面包含时间段，则默认为全部时间段的数据，如果不包含，则遍历画面中所有区域中的所有线，得到最多的数据数予以显示）
            /// </summary>
            private void resetDataIndex()
            {
                startIndex = 0;
                //画面实例不存在
                if (image == null)
                {
                    endIndex = 0;
                    return;
                }
                //包含时间段
                if (image.timePeriod != null)
                {
                    endIndex = image.timePeriod.getMinuteCount();
                }
                else
                {
                    //遍历画面中所有的区域
                    foreach (AreaClass ac in image.AreaList)
                    {
                        //遍历区域中包含的所有线信息
                        foreach (LineInfoClass lic in ac.listLineInfo)
                        {
                            //遍历商品中包含的所有线数据
                            foreach (LineData ld in commodity.lineManager.listLine)
                            {
                                //对比该线数据是否与该区域内的线信息匹配
                                if (lic.lineName == ld.lineName && lic.isShow)
                                {
                                    //终止序号等于最大的数据数
                                    endIndex = ld.listData.Count > endIndex ? ld.listData.Count : endIndex;
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// 根据数据计算Y坐标偏移量
            /// </summary>
            /// <param name="areaHeight">区域高度</param>
            /// <param name="max">最大值</param>
            /// <param name="min">最小值</param>
            /// <param name="value">数值</param>
            /// <returns></returns>
            private float getDrawHeightOffset(float areaHeight, float max, float min, float value)
            {
                if ((max - min) == 0) return 0;
                return (max - value) * areaHeight / (max - min);
            }
            #endregion

            #region 更新线程
            private void thread_UpdateDataChangeBackColor()
            {

            }
            #endregion

            #region 接口API
            /// <summary>
            /// 更新行情列表数据
            /// </summary>
            /// <returns>返回是否更新成功</returns>
            public void UpdateData()
            {

            }

            /// <summary>
            /// 显示行情列表
            /// </summary>
            public void Show()
            {
                //尝试设置默认画面
                if (image == null)
                {
                    if (dataManager.listImage.Count != 0) image = dataManager.listImage[0];
                }
                isShowed = true;
                bindEvent();
                Draw(drawType.redraw);
            }

            /// <summary>
            /// 隐藏行情列表
            /// </summary>
            public void Hide()
            {
                isShowed = false;
                unbindEvent();
            }

            /// <summary>
            /// 设置绘画商品(双击列表调用)
            /// </summary>
            /// <param name="com">商品对象</param>
            public void setCommodity(Commodity com)
            {
                this.commodity = com;

                //重设数据起止序号
                resetDataIndex();

                //重绘
                Draw(drawType.redraw);
            }
            #endregion

            #region 枚举类型
            //绘画类型
            private enum drawType
            {
                /// <summary>
                /// 画布初始化，全部重绘
                /// </summary>
                redraw,
            }

            //鼠标点击状态
            private enum MouseClickType
            {
                /// <summary>
                /// 没有点击
                /// </summary>
                None
            }
            #endregion
        }
    }
}
