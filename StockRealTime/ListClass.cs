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
        /// 行情列表类
        /// </summary>
        private class ListClass
        {
            //绘图跳转委托与事件
            public delegate void delegeteShowChart(int CommodityIndex);
            public event delegeteShowChart eventShowChart;

            //UI更新委托
            private delegate void UpdateUI();

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="pic">用于显示的PictureBox对象</param>
            /// <param name="dm">数据管理对象</param>
            public ListClass(PictureBox pic, DataManager dm)
            {
                this.pic = pic;
                this.dm = dm;
            }

            #region 成员列表
            private PictureBox pic; //用于显示的PictureBox对象
            private DataManager dm; //数据管理对象
            private bool isShowed = false;  //是否可见

            //图形绘画
            Graphics gra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap bit;

            //鼠标操作相关绘画
            Graphics MouseGra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap MouseBit;

            //列操作标志线绘画
            Graphics ColumnTagGra = Graphics.FromImage(new Bitmap(1, 1));
            Bitmap ColumnTagBit;

            //鼠标点击状态
            MouseClickType MouseClickStatu = MouseClickType.None;

            //列调整鼠标点击列序号
            int columnIndexMouseClick;

            //排序的列序号
            int columnIndexSort;
            //是否排序列
            sortType SortStatu = sortType.None;

            //更新对象数组
            List<ListSubItems> listChange = new List<ListSubItems>();

            //行选择标志
            bool rowSelectClick = false;
            //行选择鼠标点击行序号
            int rowIndexMouseClick = 0;
            List<int> listRowIndex = new List<int>();
            //行起始序号
            int rowStartIndex = 0;
            //行终止序号
            int rowEndIndex = 0;
            //列起始序号
            int columnStartIndex = 0;

            //改变窗体大小后重绘列操作标志
            bool redrawLineTag = false;

            //鼠标调整列线移动位置（未松开鼠标时）
            float MouseMovePoint_X;

            //滚动条宽高(水平滚动条和垂直滚动条相等)
            int ScrollWidth = 20;
            //水平滚动条宽度
            float HScrollWidth;
            //垂直滚动条高度
            float VScrollHeight;
            //垂直滚动条单位数据高度
            float VScrollUnitHeight;

            //更新数据背景色线程
            Thread t_BackColorChange = null;

            //更新委托方法
            UpdateUI drawDelegate;
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
                pic.MouseDoubleClick += new MouseEventHandler(EMouseDoubleClick);
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
                pic.MouseDoubleClick -= new MouseEventHandler(EMouseDoubleClick);
            }

            //事件鼠标抬起
            private void EMouseUp(object sender, MouseEventArgs e)
            {
                if (MouseClickStatu == MouseClickType.ColumnLine)
                {
                    //获取当前调整的列宽
                    float tempwidth = e.X - dm.listColumn[columnIndexMouseClick].StartPos;
                    //判断如果位置超过垂直滚动条，则定义列宽为滚动条
                    if (e.X >= pic.Width - ScrollWidth) tempwidth = (pic.Width - ScrollWidth - 1) - dm.listColumn[columnIndexMouseClick].StartPos;

                    //调用调整方法
                    setColumnWidth(columnIndexMouseClick, tempwidth);

                    //绘制列调整鼠标点击标志
                    redrawLineTag = true;

                    //清空鼠标状态
                    MouseClickStatu = MouseClickType.None;
                    //重绘画面
                    Draw(drawType.ColumnAdjust);
                }

                //清空鼠标状态
                MouseClickStatu = MouseClickType.None;
            }

            //事件鼠标按下
            private void EMouseDown(object sender, MouseEventArgs e)
            {
                pic.Select();
                if (e.X < 0 || e.X > pic.Width) return;
                //如果点击范围在列线
                if (ColumnTagBit.GetPixel(e.X, 0).R != 0 && e.Y <= staPara.rowHeight && e.X < pic.Width - ScrollWidth)
                {
                    #region 执行体
                    MouseClickStatu = MouseClickType.ColumnLine;

                    MouseMovePoint_X = e.X;

                    //记录点击的列序号
                    columnIndexMouseClick = ColumnTagBit.GetPixel(e.X, 0).B;
                    #endregion
                    Draw(drawType.drawColumnAdjustLine);
                }

                //如果点击范围在其他，则选择行
                else if (e.Y > staPara.rowHeight && e.Y < pic.Height - ScrollWidth && e.X < pic.Width - ScrollWidth)
                {
                    #region 执行体
                    rowSelectClick = true;
                    rowIndexMouseClick = (int)(e.Y / staPara.rowHeight) - 1;   //减去一行为列名称行

                    //如果选择的列的行线位置超出屏幕，则选择最后一行的上一行
                    if ((rowIndexMouseClick + 2) * staPara.rowHeight >= pic.Height)
                    {
                        rowIndexMouseClick = rowEndIndex - rowStartIndex - 1;
                    }
                    #endregion
                    Draw(drawType.drawRowSelectLine);
                }

                //如果点击范围在列名称行内
                else if (e.Y <= staPara.rowHeight && e.X < pic.Width - ScrollWidth)
                {
                    #region 执行体
                    //更改排序状态
                    /*
                     * 如果点击的是同一列，则根据当前排序方式切换
                     * 如果点击的不是同一列，则默认为升序排列
                     */
                    if (ColumnTagBit.GetPixel(e.X, 0).B == 0)
                    {
                        sortbyIndex();
                    }
                    else
                    {
                        if (ColumnTagBit.GetPixel(e.X, 0).B != columnIndexSort)
                        {
                            SortStatu = sortType.Asc;
                        }
                        else
                        {
                            if (SortStatu == sortType.None)
                            {
                                SortStatu = sortType.Asc;
                            }
                            else if (SortStatu == sortType.Asc)
                            {
                                SortStatu = sortType.Desc;
                            }
                            else
                            {
                                SortStatu = sortType.Asc;
                            }
                        }
                        //记录点击的列序号
                        columnIndexSort = ColumnTagBit.GetPixel(e.X, 0).B;
                        //排序
                        commodity_sort();
                    }
                    #endregion
                    Draw(drawType.redraw);
                }

                //如果点击的是滚动条
                else if (e.Y >= pic.Height - ScrollWidth || e.X >= pic.Width - ScrollWidth)
                {
                    //开启Timer控件监听长时间按钮                
                    //timer.Enabled = true;
                    if (e.Y >= pic.Height - ScrollWidth && e.X <= ScrollWidth)
                    {
                        //点击水平左按钮
                        subitemsLeft(columnStartIndex - 1);
                        //设置鼠标标志
                        MouseClickStatu = MouseClickType.LeftButton;
                    }
                    else if (e.Y >= pic.Height - ScrollWidth && e.X >= pic.Width - ScrollWidth)
                    {
                        //点击水平右按钮
                        subitemsRight(columnStartIndex + 1);
                        //设置鼠标标志
                        MouseClickStatu = MouseClickType.RightButton;
                    }
                    else if (e.X >= pic.Width - ScrollWidth && e.Y <= ScrollWidth)
                    {
                        //点击垂直上按钮
                        subitemsUp(rowStartIndex - 1);
                        //设置鼠标标志
                        MouseClickStatu = MouseClickType.UpButton;
                    }
                    else if (e.X >= pic.Width - ScrollWidth && e.Y >= pic.Height - 2 * ScrollWidth)
                    {
                        //点击垂直下按钮
                        subitemsDown(rowStartIndex + 1);
                        //设置鼠标标志
                        MouseClickStatu = MouseClickType.DownButton;
                    }
                    else
                    {
                        //判断点击的是水平滚动条还是垂直滚动条
                        if (e.Y >= pic.Height - ScrollWidth)
                        {
                            if (bit.GetPixel(e.X, (int)(pic.Height - ScrollWidth * 0.5f)).B == 128)
                            {
                                MouseClickStatu = MouseClickType.HScroll;
                            }
                            else
                            {
                                //MessageBox.Show("点击了水平滚动条其他部分");
                            }
                        }
                        else if (e.X >= pic.Width - ScrollWidth)
                        {
                            if (e.Y < 0 || e.Y > pic.Height) return;
                            if (bit.GetPixel((int)(pic.Width - ScrollWidth * 0.5f), e.Y).B == 128)
                            {
                                MouseClickStatu = MouseClickType.VScroll;
                            }
                            else
                            {
                                //MessageBox.Show("点击了垂直滚动条其他部分");
                            }
                        }
                    }
                    Draw(drawType.redraw);
                }
            }

            //事件鼠标移动
            private void EMouseMove(object sender, MouseEventArgs e)
            {
                if (e.X < 0 || e.X > pic.Width) return;
                switch (MouseClickStatu)
                {
                    //鼠标没有点击
                    case MouseClickType.None:
                        {
                            //判断是否超出可见范围
                            if (e.X > pic.Width - ScrollWidth)
                            {
                                pic.Cursor = Cursors.Default;
                            }
                            else if (ColumnTagBit.GetPixel(e.X, 0).R == 1 && e.Y <= staPara.rowHeight)
                            {
                                pic.Cursor = Cursors.VSplit;
                            }
                            else
                            {
                                pic.Cursor = Cursors.Default;
                            }
                            break;
                        }
                    case MouseClickType.ColumnLine:
                        {
                            MouseMovePoint_X = e.X;
                            Draw(drawType.drawColumnAdjustLine);
                            break;
                        }
                    case MouseClickType.HScroll:
                        {
                            bool tag = false;
                            int indexofClick = (int)((e.X - 1 - ScrollWidth) / HScrollWidth);
                            //判断鼠标位置是左滚、右滚还是没有移动
                            if (indexofClick == columnStartIndex) return;
                            else if (indexofClick > columnStartIndex) tag = subitemsRight(indexofClick);
                            else if (indexofClick < columnStartIndex) tag = subitemsLeft(indexofClick);
                            //重绘
                            if (tag) Draw(drawType.redraw);
                            break;
                        }
                    case MouseClickType.VScroll:
                        {
                            bool tag = false;
                            int indexofClick = (int)((e.Y - 1 - ScrollWidth) / VScrollHeight);
                            //判断鼠标当前位置是上滚、下滚还是没有移动
                            if (indexofClick == rowStartIndex) return;
                            else if (indexofClick > rowStartIndex) tag = subitemsDown(indexofClick);
                            else if (indexofClick < rowStartIndex) tag = subitemsUp(indexofClick);
                            //重绘
                            if (tag) Draw(drawType.redraw);
                            break;
                        }
                }
            }

            //事件鼠标双击
            public void EMouseDoubleClick(object sender, MouseEventArgs e)
            {
                if (listRowIndex.Count == 0)
                {
                    //如果点击范围是选择行，则跳转
                    if (e.Y > staPara.rowHeight && e.Y < pic.Height - ScrollWidth && e.X < pic.Width - ScrollWidth)
                    {
                        //调用事件，通知外部类显示指定序号商品走势图
                        listRowIndex.Add(rowIndexMouseClick);
                        eventShowChart(rowIndexMouseClick + rowStartIndex);

                    }
                }
                else if (listRowIndex.Count > 0)
                {
                    //如果点击范围是选择行，则跳转
                    if (e.Y > staPara.rowHeight && e.Y < pic.Height - ScrollWidth && e.X < pic.Width - ScrollWidth && listRowIndex[listRowIndex.Count - 1] != rowIndexMouseClick)
                    {
                        //调用事件，通知外部类显示指定序号商品走势图
                        listRowIndex.Add(rowIndexMouseClick);
                        eventShowChart(rowIndexMouseClick + rowStartIndex);

                    }
                }
            }

            //事件控件大小改变
            private void ESizeChanged(object sender, EventArgs e)
            {
                rowStartIndex = 0;
                Draw(drawType.redraw);
            }

            //事件键盘按键
            private void EKeyPress(object sender, PreviewKeyDownEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Down:
                        {
                            if (rowSelectClick)
                            {
                                //超出屏幕范围
                                if ((rowIndexMouseClick + 3) * staPara.rowHeight < pic.Height - ScrollWidth)
                                {
                                    rowIndexMouseClick++;
                                }
                                else
                                {
                                    //如果超出屏幕范围，则下滚
                                    if (subitemsDown(rowStartIndex + 1)) Draw(drawType.redraw);
                                }
                            }
                            else
                            {
                                rowSelectClick = true;
                            }
                            Draw(drawType.drawRowSelectLine);
                            break;
                        }
                    case Keys.Up:
                        {
                            if (rowSelectClick)
                            {
                                if (rowIndexMouseClick != 0)
                                {
                                    rowIndexMouseClick--;
                                }
                                else
                                {
                                    //如果超出屏幕范围，则上滚
                                    if (subitemsUp(rowStartIndex - 1)) Draw(drawType.redraw);
                                }
                            }
                            else
                            {
                                rowSelectClick = true;
                            }
                            Draw(drawType.drawRowSelectLine);
                            break;
                        }
                    case Keys.Escape:
                        {
                            Draw(drawType.ClearMouseGra);
                            break;
                        }
                }
            }

            //事件鼠标滚轮
            private void EMouseWheel(object sender, MouseEventArgs e)
            {
                bool tag;
                if (e.Delta < 0)
                {
                    //滚轮下滚
                    tag = subitemsDown(rowStartIndex + staPara.MouseWheelScroll);
                }
                else
                {
                    //滚轮上滚
                    tag = subitemsUp(rowStartIndex - staPara.MouseWheelScroll);
                }
                if (tag) Draw(drawType.redraw);
            }

            #region 数据项滚动
            //数据项下滚(检查输入的index是否有效，直接设置有效值)
            private bool subitemsDown(int index)
            {
                //如果超出屏幕显示范围
                //那么此时 起始index 最大值满足等式 
                //起始index + 屏幕显示的数据 - 6(定义的空行) = 数据项总数
                if (index > dm.commodity.Count + 6 - ((int)(pic.Height / staPara.rowHeight) - 1)) index = dm.commodity.Count + 6 - ((int)(pic.Height / staPara.rowHeight) - 1);

                if (rowStartIndex == index)
                {
                    return false;
                }
                //如果当前页面允许显示的行数大于已有的数据行数，则不允许滚动，始终返回0
                else if (((int)(pic.Height / staPara.rowHeight) - 1) > dm.commodity.Count)
                {
                    rowStartIndex = 0;
                    return false;
                }
                //如果上述语句没有执行，则说明数据没有超下界或不允许滚动，则输入的index有效
                else
                {
                    rowStartIndex = index;
                    return true;
                }
            }

            //数据项上滚
            private bool subitemsUp(int index)
            {
                //无效预处理
                if (index < 0) index = 0;

                if (rowStartIndex == index)
                {
                    return false;
                }
                //如果上述语句没有执行，则序号有效
                else
                {
                    rowStartIndex = index;
                    return true;
                }
            }

            //数据项左滚
            private bool subitemsLeft(int index)
            {
                //无效预处理
                if (index < 0) index = 0;

                if (columnStartIndex == index)
                {
                    return false;
                }
                //如果上述语句没有执行，则序号有效
                else
                {
                    columnStartIndex = index;
                    //设置起始列
                    setColumnStart(columnStartIndex + staPara.fixColumnIndex + 1);
                    return true;
                }
            }

            //数据项右滚
            private bool subitemsRight(int index)
            {
                //序号的最大值应该等于 列总数 - 固定列数 - 1
                if (index > dm.listColumn.Count - (staPara.fixColumnIndex + 1) - 1) index = dm.listColumn.Count - (staPara.fixColumnIndex + 1) - 1;

                if (columnStartIndex == index)
                {
                    return false;
                }
                //如果除了固定列没有其他列的时候，直接返回0
                else if (dm.listColumn.Count - (staPara.fixColumnIndex + 1) == 0)
                {
                    columnStartIndex = 0;
                    return false;
                }
                //如果上述代码没有执行，则index有效
                else
                {
                    columnStartIndex = index;
                    //设置起始列
                    setColumnStart(columnStartIndex + staPara.fixColumnIndex + 1);
                    return true;
                }
            }

            #endregion
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

                switch (DrawType)
                {
                    case drawType.redraw:
                        {
                            init();
                            drawAll();
                            break;
                        }
                    case drawType.drawRowSelectLine:
                        {
                            drawRowSelectLine();
                            break;
                        }
                    case drawType.drawColumnAdjustLine:
                        {
                            drawColumnAdjustLine();
                            break;
                        }
                    case drawType.ColumnAdjust:
                        {
                            Clear();
                            drawAll();
                            break;
                        }
                    case drawType.ClearMouseGra:
                        {
                            ClearMouseGra();
                            break;
                        }
                    case drawType.updateData:
                        {
                            drawUpdateData();
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

                //计算滚动条宽度
                getScrollWidth();

                //计算行终止序号
                rowEndIndex = (int)(pic.Height / staPara.rowHeight) - 1 + rowStartIndex;

                //重置列标志绘制标志
                redrawLineTag = true;
            }

            //绘制列
            private void drawColumns()
            {
                //标记是否有不可见列
                bool spaceLine = false;

                foreach (ListColumn col in dm.listColumn)
                {
                    //如果不可见则跳过
                    if (col.Visible == false)
                    {
                        spaceLine = true;
                        continue;
                    }

                    string colName = col.Name;
                    //判断如果当前列在排序，则输出排序方式标志，并绘制背景色
                    if (dm.listColumn.IndexOf(col) == columnIndexSort && SortStatu != sortType.None)
                    {
                        switch (SortStatu)
                        {
                            case sortType.Asc:
                                {
                                    colName += "↓";
                                    break;
                                }
                            case sortType.Desc:
                                {
                                    colName += "↑";
                                    break;
                                }
                        }
                        RectangleF rec = col.getRectangleF(0);
                        rec.Height = pic.Height;
                        gra.FillRectangle(sysColor.sBlue, rec);
                    }

                    drawText(col.TitleAlignStyle, colName, sysColor.Default, col.getRectangleF(0), false);

                    //绘制分割线
                    gra.DrawLine(Pens.Red, new PointF(col.EndPos, 6), new PointF(col.EndPos, staPara.rowHeight - 12));

                    //绘制列线(调试用)
                    //gra.DrawLine(Pens.Red, new PointF(col.EndPos, 0), new PointF(col.EndPos, pic.Height));

                    //绘制允许鼠标调整范围线（调试用）
                    //gra.DrawLine(new Pen(Color.Green, 3), new PointF(col.Start2End.X + staPara.IdentificationErrorPixelPoints, staPara.rowHeight), new PointF(col.EndPos - staPara.IdentificationErrorPixelPoints, staPara.rowHeight));
                    //gra.DrawLine(new Pen(Color.Yellow, 3), new PointF(col.EndPos - staPara.IdentificationErrorPixelPoints, staPara.rowHeight), new PointF(col.EndPos + staPara.IdentificationErrorPixelPoints, staPara.rowHeight));

                    if (redrawLineTag)
                    {
                        //绘制列标志线
                        ColumnTagGra.DrawLine(new Pen(Color.FromArgb(0, 0, dm.listColumn.IndexOf(col))), new PointF(col.StartPos + staPara.IdentificationErrorPixelPoints, 0), new PointF(col.EndPos - staPara.IdentificationErrorPixelPoints, 0));
                        ColumnTagGra.DrawLine(new Pen(Color.FromArgb(1, 0, dm.listColumn.IndexOf(col))), new PointF(col.EndPos - staPara.IdentificationErrorPixelPoints, 0), new PointF(col.EndPos + staPara.IdentificationErrorPixelPoints, 0));
                    }
                }

                //绘制列名称分隔水平线
                gra.DrawLine(Pens.Red, new PointF(0, (int)staPara.rowHeight), new PointF(pic.Width - ScrollWidth, (int)staPara.rowHeight));

                //如果没有显示某些列，则绘制一条线
                if (spaceLine) gra.DrawLine(Pens.Yellow, new PointF(dm.listColumn[staPara.fixColumnIndex].EndPos, 0), new PointF(dm.listColumn[staPara.fixColumnIndex].EndPos, pic.Height - ScrollWidth));
            }

            //绘制行
            private void drawRows(Commodity com)
            {
                foreach (ListColumn col in dm.listColumn)
                {
                    drawSubItems(col, com);
                }
            }

            //绘制选择行线
            private void drawRowSelectLine()
            {
                //如果绘制行线标记为false，则不绘制
                if (rowSelectClick == false) return;

                //判断如果超出范围，则定位点击序号为最后一行数据序号
                if (rowIndexMouseClick + rowStartIndex + 1 >= dm.commodity.Count)
                {
                    rowIndexMouseClick = dm.commodity.Count - 1 - rowStartIndex;
                    if (dm.commodity.Count == 0)
                    {
                        rowSelectClick = false;
                        return;
                    }
                }

                //绘制选择行线
                MouseGra.Clear(Color.Transparent);
                //判断如果遮盖下方滚动条则退出
                if ((rowIndexMouseClick + 2) * staPara.rowHeight > pic.Height - ScrollWidth) return;
                MouseGra.DrawLine(Pens.White, new PointF(0, (rowIndexMouseClick + 2) * staPara.rowHeight), new PointF(pic.Width - ScrollWidth, (rowIndexMouseClick + 2) * staPara.rowHeight)); //Y坐标行序号加1是因为绘制在行下方，也就是第二行的上方
            }

            //绘画鼠标调整列线
            private void drawColumnAdjustLine()
            {
                MouseGra.Clear(Color.Transparent);
                //判断如果移动位置小于列宽最小值，则绘制列线在最小列宽位置
                float tempwidth = MouseMovePoint_X - dm.listColumn[columnIndexMouseClick].StartPos;
                if (tempwidth < staPara.minColumnWidth)
                {
                    //计算最小列宽位置
                    float minPos = dm.listColumn[columnIndexMouseClick].StartPos + staPara.minColumnWidth;

                    //如果小于最小列宽，则绘制列线在最小列宽位置
                    MouseGra.DrawLine(Pens.White, new PointF(minPos, 0), new PointF(minPos, pic.Height - ScrollWidth));
                }
                //判断如果移动位置超过右边滚动条，则绘制列线在滚动条处
                else if (MouseMovePoint_X >= pic.Width - ScrollWidth)
                {
                    MouseGra.DrawLine(Pens.White, new PointF(pic.Width - ScrollWidth - 1, 0), new PointF(pic.Width - ScrollWidth - 1, pic.Height - ScrollWidth));
                }
                else
                {
                    //绘画调整列线在鼠标位置
                    MouseGra.DrawLine(Pens.White, new PointF(MouseMovePoint_X, 0), new PointF(MouseMovePoint_X, pic.Height - ScrollWidth));
                }
            }

            //全部重绘
            private void drawAll()
            {
                //绘画列
                drawColumns();

                //重置列操作绘制标志
                redrawLineTag = false;

                //绘制行，并重新编号
                for (int i = rowStartIndex; i <= rowEndIndex; i++)
                {
                    if (i >= dm.commodity.Count) break;
                    dm.commodity[i].Items[0].Value = (i + 1).ToString();
                    drawRows(dm.commodity[i]);
                }

                //绘制滚动条
                drawScrollBar();

                //绘制行选择线
                drawRowSelectLine();

                //绘制鼠标移动线
                if (MouseClickStatu == MouseClickType.ColumnLine) drawColumnAdjustLine();
            }

            //清空鼠标操作层
            private void ClearMouseGra()
            {
                //置零选择线
                rowIndexMouseClick = 0;
                //重置行选择标志
                rowSelectClick = false;

                //清空鼠标操作图层
                MouseGra.Clear(Color.Transparent);

                //按序号排序，取消排序模式
                sortbyIndex();
                Draw(drawType.redraw);
            }

            //数据更新
            private void drawUpdateData()
            {
                if (listChange.Count == 0) return;
                int i = 0;

                while (i != listChange.Count)
                {
                    ListSubItems ls = listChange[i++];

                    //找到位置行号对应商品的数组位置
                    foreach (Commodity com in dm.commodity)
                    {
                        if (com.Index == ls.CommodityIndex)
                        {
                            //获取位置对应的矩形(列序号是数据项对象在商品Items中位置-当前起始列)
                            if (com.Items.IndexOf(ls) - columnStartIndex < 0)
                            {
                                //清空颜色
                                ls.ClearColor();
                                break;
                            }
                            RectangleF rec = dm.listColumn[com.Items.IndexOf(ls) - columnStartIndex].getRectangleF((dm.commodity.IndexOf(com) + 1 - rowStartIndex) * staPara.rowHeight);
                            //清空
                            gra.FillRectangle(Brushes.Transparent, rec);

                            //判断矩形是否遮盖滚动条 或遮盖固定列 或者 行标题
                            if (checkRecisOut(rec))
                            {
                                //清空颜色
                                ls.ClearColor();
                            }
                            else
                            {
                                //重绘数据
                                drawSubItems(dm.listColumn[com.Items.IndexOf(ls)], com);
                            }
                            break;
                        }
                    }
                }
            }

            //绘制单个数据项
            private void drawSubItems(ListColumn col, Commodity com)
            {
                //列不可见则跳过
                if (col.Visible == false) return;

                //获取列序号
                int i = dm.listColumn.IndexOf(col);

                //获取绘制数据
                string str = com.Items[i].Value;

                //是否是小数
                bool isDecimal = false;
                if (str.IndexOf(".") != -1) isDecimal = true;

                //如果显示涨幅标志
                if (col.showCompareSymbol)
                {
                    switch (dm.getComapreResult(col, com, com.Items[dm.listColumn.IndexOf(col)].Value))
                    {
                        case compareResultType.Less:
                            {
                                str = "↓" + str;
                                break;
                            }
                        case compareResultType.More:
                            {
                                str = "↑" + str;
                                break;
                            }
                    }
                    str = str.Replace("-", "");

                    //判断如果只剩"-"
                    if (str == "") str = "-";
                }

                //如果是小数，则替换掉小数点，绘制为横线
                if (isDecimal) str = str.Replace(".", "");

                //填充背景色(如果当前列在排序，则绘制蓝色，否则绘制数据项中的背景色)
                if (dm.listColumn.IndexOf(col) == columnIndexSort && SortStatu != sortType.None)
                {
                    gra.FillRectangle(sysColor.sBlue, col.getRectangleF((dm.commodity.IndexOf(com) + 1 - rowStartIndex) * staPara.rowHeight));
                }
                else
                {
                    gra.FillRectangle(new SolidBrush(com.Items[i].BackColor), col.getRectangleF((dm.commodity.IndexOf(com) + 1 - rowStartIndex) * staPara.rowHeight));
                }

                //绘制行数据(判断数据对齐方式)
                //如果列没有比较方式，则绘制列定义的数据颜色
                if (col.CompareType == compareType.None)
                {
                    drawText(col.DataAlignStyle, str, col.dataFontColor, col.getRectangleF((dm.commodity.IndexOf(com) + 1 - rowStartIndex) * staPara.rowHeight), isDecimal);
                }
                else
                {
                    drawText(col.DataAlignStyle, str, com.Items[i].FontColor, col.getRectangleF((dm.commodity.IndexOf(com) + 1 - rowStartIndex) * staPara.rowHeight), isDecimal);
                }
            }

            //根据对齐方式绘制数据
            private void drawText(textAlignStyle alignStyle, string str, Color color, RectangleF rec, bool isDecimal)
            {
                //绘制字体格式
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.FormatFlags = StringFormatFlags.LineLimit;

                //绘制数据
                switch (alignStyle)
                {
                    case textAlignStyle.Right:
                        {
                            sf.Alignment = StringAlignment.Far;
                            break;
                        }
                    case textAlignStyle.Left:
                        {
                            sf.Alignment = StringAlignment.Near;
                            break;
                        }
                    case textAlignStyle.Center:
                        {
                            sf.Alignment = StringAlignment.Center;
                            break;
                        }
                }

                //如果是小数，则绘制横线
                if (isDecimal)
                {
                    gra.DrawString(str.Substring(0, str.Length - 2) + "__", staPara.Font, new SolidBrush(color), rec, sf);
                    string tempSpace = "";
                    for (int i = 0; i < str.Length - 2; i++)
                    {
                        tempSpace += " ";
                    }
                    gra.DrawString(tempSpace + str.Substring(str.Length - 2, 2), staPara.Font, new SolidBrush(color), rec, sf);
                }
                else
                {
                    gra.DrawString(str, staPara.Font, new SolidBrush(color), rec, sf);
                }

                //判断如果没有完全绘制数据，则绘制"··“用于提示
                if (staPara.getStringSizeF(str).Width > rec.Width)
                {
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Far;
                    gra.DrawString("...", new Font("宋体", 9), Brushes.White, rec, sf);
                }
            }

            //绘制滚动条
            private void drawScrollBar()
            {
                //按钮
                PointF[] LeftButton = new PointF[3];
                PointF[] RightButton = new PointF[3];
                PointF[] UpButton = new PointF[3];
                PointF[] DownButton = new PointF[3];
                LeftButton[0] = new PointF(ScrollWidth * 0.25f, pic.Height - ScrollWidth * 0.5f);
                LeftButton[1] = new PointF(ScrollWidth * 0.75f, pic.Height - ScrollWidth * 0.25f);
                LeftButton[2] = new PointF(ScrollWidth * 0.75f, pic.Height - ScrollWidth * 0.75f);
                RightButton[0] = new PointF(pic.Width - ScrollWidth * 0.25f, pic.Height - ScrollWidth * 0.5f);
                RightButton[1] = new PointF(pic.Width - ScrollWidth * 0.75f, pic.Height - ScrollWidth * 0.25f);
                RightButton[2] = new PointF(pic.Width - ScrollWidth * 0.75f, pic.Height - ScrollWidth * 0.75f);
                UpButton[0] = new PointF(pic.Width - ScrollWidth * 0.5f, ScrollWidth * 0.25f);
                UpButton[1] = new PointF(pic.Width - ScrollWidth * 0.25f, ScrollWidth * 0.75f);
                UpButton[2] = new PointF(pic.Width - ScrollWidth * 0.75f, ScrollWidth * 0.75f);
                DownButton[0] = new PointF(pic.Width - ScrollWidth * 0.5f, pic.Height - ScrollWidth * 1.25f);
                DownButton[1] = new PointF(pic.Width - ScrollWidth * 0.25f, pic.Height - ScrollWidth * 1.75f);
                DownButton[2] = new PointF(pic.Width - ScrollWidth * 0.75f, pic.Height - ScrollWidth * 1.75f);

                //水平滚动条
                //{
                //    //滚动条边框
                //    gra.FillRectangle(Brushes.Black, 0, pic.Height - ScrollWidth, pic.Width, ScrollWidth);
                //    gra.DrawRectangle(Pens.Red, 0, pic.Height - ScrollWidth, pic.Width, ScrollWidth);

                //    //按钮边界线
                //    gra.DrawLine(Pens.Red, new Point(ScrollWidth, pic.Height - ScrollWidth + 4), new Point(ScrollWidth, pic.Height - 4));
                //    gra.DrawLine(Pens.Red, new Point(pic.Width - ScrollWidth, pic.Height - ScrollWidth + 4), new Point(pic.Width - ScrollWidth, pic.Height - 4));

                //    //按钮
                //    gra.FillPolygon(Brushes.Gray, LeftButton);
                //    gra.FillPolygon(Brushes.Gray, RightButton);
                //}

                //垂直滚动条
                {
                    //滚动条边框
                    gra.FillRectangle(Brushes.Black, pic.Width - ScrollWidth, 0, ScrollWidth, pic.Height - ScrollWidth);
                    gra.DrawRectangle(Pens.Red, pic.Width - ScrollWidth, 0, ScrollWidth, pic.Height - ScrollWidth);

                    //按钮边界线
                    gra.DrawLine(Pens.Red, new Point(pic.Width - ScrollWidth + 4, ScrollWidth), new Point(pic.Width - 4, ScrollWidth));
                    gra.DrawLine(Pens.Red, new Point(pic.Width - ScrollWidth + 4, pic.Height - 2 * ScrollWidth), new Point(pic.Width - 4, pic.Height - 2 * ScrollWidth));

                    //按钮
                    gra.FillPolygon(Brushes.Gray, UpButton);
                    gra.FillPolygon(Brushes.Gray, DownButton);
                }

                //绘制滚动块
                {
                    //清空滚动条
                    gra.FillRectangle(Brushes.Black, ScrollWidth + 1, pic.Height - ScrollWidth * 0.75f, pic.Width - 2 * ScrollWidth - 1, ScrollWidth * 0.5f); //水平滚动条
                    gra.FillRectangle(Brushes.Black, pic.Width - ScrollWidth * 0.75f, ScrollWidth + 1, ScrollWidth * 0.5f, pic.Height - 3 * ScrollWidth - 1); //垂直滚动条

                    //水平滚动条
                    //gra.FillRectangle(Brushes.Gray, ScrollWidth + 1 + columnStartIndex * HScrollWidth, pic.Height - ScrollWidth * 0.75f, HScrollWidth, ScrollWidth * 0.5f);
                    //垂直滚动条
                    gra.FillRectangle(Brushes.Gray, pic.Width - ScrollWidth * 0.75f, ScrollWidth + 1 + rowStartIndex * VScrollUnitHeight, ScrollWidth * 0.5f, VScrollHeight);
                }
            }
            #endregion

            #region 计算方法
            /// <summary>
            /// 调整列宽(如果调整的列宽值小于最小列宽，将强制调制为最小列宽)
            /// </summary>
            /// <param name="ColumnIndex">列序号</param>
            /// <param name="ColumnWidth">要调整的列宽</param>
            private void setColumnWidth(int ColumnIndex, float ColumnWidth)
            {
                if (ColumnWidth < staPara.minColumnWidth)
                {
                    //如果小于最小列宽，则设置调整为最小列宽
                    ColumnWidth = staPara.minColumnWidth;
                }
                //设置当前列宽,得到当前列终点值（即下一列的起始值）
                float startPos = dm.listColumn[ColumnIndex].setWidth(dm.listColumn[ColumnIndex].StartPos, ColumnWidth);
                //循环调整列宽
                for (int i = ColumnIndex + 1; i < dm.listColumn.Count; i++)
                {
                    startPos = dm.listColumn[i].setWidth(startPos, dm.listColumn[i].Width);
                }
                //设置起始列
                setColumnStart(columnStartIndex + staPara.fixColumnIndex + 1);
            }

            /// <summary>
            /// 设置某一列为起始列
            /// </summary>
            /// <param name="ColumnIndex"></param>
            private void setColumnStart(int ColumnIndex)
            {
                //获取固定列的终点
                //Console.WriteLine("当前起始列序号{0}", ColumnIndex);
                float startPos = dm.listColumn[staPara.fixColumnIndex].EndPos;
                //设置当前列（不含）之前的所有列（除固定列）不可见
                for (int i = staPara.fixColumnIndex + 1; i < ColumnIndex; i++)
                {
                    dm.listColumn[i].Visible = false;
                    //Console.WriteLine("第{0}列不可见", i);

                }
                //设置当前列（含）之后的所有列可见，并重新设置起点
                for (int i = ColumnIndex; i < dm.listColumn.Count; i++)
                {
                    dm.listColumn[i].Visible = true;
                    startPos = dm.listColumn[i].setWidth(startPos, dm.listColumn[i].Width);
                }
            }

            /// <summary>
            /// 查找需要从更新队列中删除的SubItems对象
            /// </summary>
            /// <param name="sub"></param>
            /// <returns></returns>
            private bool findSubItems(ListSubItems sub)
            {
                if (sub.BackColor == Color.FromArgb(0, 0, 0)) return true;
                else return false;
            }

            /// <summary>
            /// 商品列表排序
            /// </summary>
            private void commodity_sort()
            {
                if (SortStatu != sortType.None)
                {
                    Commodity a;
                    Commodity b;
                    Commodity temp;

                    float ParseValue_a;
                    float ParseValue_b;
                    for (int i = 0; i < dm.commodity.Count - 1; i++)
                    {
                        for (int j = i; j < dm.commodity.Count; j++)
                        {
                            a = dm.commodity[i];
                            b = dm.commodity[j];

                            //判断是否存在"-"
                            if (a.Items[columnIndexSort].Value == "-" || b.Items[columnIndexSort].Value == "-")
                            {
                                if (a.Items[columnIndexSort].Value == "-" && b.Items[columnIndexSort].Value == "-")
                                {
                                    //如果都为"-"则不做变换
                                }
                                else
                                {
                                    //因为a在b前面，所以只有当a为"-"时交换，保证"-"永远在最后
                                    if (a.Items[columnIndexSort].Value == "-")
                                    {
                                        temp = dm.commodity[i];
                                        dm.commodity[i] = dm.commodity[j];
                                        dm.commodity[j] = temp;
                                    }
                                }
                            }
                            else
                            {
                                //如果为数字
                                if (float.TryParse(a.Items[columnIndexSort].Value, out ParseValue_a))
                                {
                                    float.TryParse(b.Items[columnIndexSort].Value, out ParseValue_b);

                                    //升序排列
                                    if (SortStatu == sortType.Asc)
                                    {
                                        if (ParseValue_a > ParseValue_b)
                                        {
                                            temp = dm.commodity[i];
                                            dm.commodity[i] = dm.commodity[j];
                                            dm.commodity[j] = temp;
                                        }
                                    }
                                    //降序排列
                                    else
                                    {
                                        if (ParseValue_a < ParseValue_b)
                                        {
                                            temp = dm.commodity[i];
                                            dm.commodity[i] = dm.commodity[j];
                                            dm.commodity[j] = temp;
                                        }

                                    }
                                }
                                //如果为汉字
                                else
                                {
                                    //升序排列
                                    if (SortStatu == sortType.Asc)
                                    {
                                        if (String.Compare(a.Items[columnIndexSort].Value, b.Items[columnIndexSort].Value) == 1)
                                        {
                                            temp = dm.commodity[i];
                                            dm.commodity[i] = dm.commodity[j];
                                            dm.commodity[j] = temp;
                                        }
                                    }
                                    //降序排列
                                    else
                                    {
                                        if (String.Compare(a.Items[columnIndexSort].Value, b.Items[columnIndexSort].Value) == -1)
                                        {
                                            temp = dm.commodity[i];
                                            dm.commodity[i] = dm.commodity[j];
                                            dm.commodity[j] = temp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Draw(drawType.redraw);
                }
            }

            /// <summary>
            /// 按列序号排序，并取消排序标志
            /// </summary>
            private void sortbyIndex()
            {
                dm.commodity.Sort(delegate (Commodity a, Commodity b)
                {
                    if (a.Index < b.Index) return -1;
                    else if (a.Index == b.Index) return 0;
                    else return 1;
                });
                SortStatu = sortType.None;
            }

            /// <summary>
            /// 计算滚动条宽度
            /// </summary>
            private void getScrollWidth()
            {
                //水平滚动条
                if (dm.listColumn.Count == 3)
                {
                    HScrollWidth = (pic.Width - 2 * ScrollWidth);
                }
                else
                {
                    HScrollWidth = (pic.Width - 2 * ScrollWidth) / (float)(dm.listColumn.Count - 3);
                }
                //垂直滚动条
                int commodityCount = dm.commodity.Count + 6 - ((int)(pic.Height / staPara.rowHeight) - 1);
                if (commodityCount < 0 || dm.commodity.Count < (int)(pic.Height / staPara.rowHeight) - 1)
                {
                    VScrollUnitHeight = (pic.Height - 3 * ScrollWidth);
                }
                else
                {
                    VScrollUnitHeight = (pic.Height - 3 * ScrollWidth) / (float)(commodityCount + 1);
                }
                //如果垂直单位数据高度过小，则固定为10
                if (VScrollUnitHeight < 10)
                {
                    VScrollHeight = 10;
                }
                else
                {
                    VScrollHeight = VScrollUnitHeight;
                }
            }

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
            /// 判断矩形是否超出边界
            /// </summary>
            /// <param name="rec"></param>
            private bool checkRecisOut(RectangleF rec)
            {
                if (rec.Y + rec.Height > pic.Height - ScrollWidth || rec.X + rec.Width > pic.Width - ScrollWidth || rec.Y < staPara.rowHeight || rec.X < dm.listColumn[staPara.fixColumnIndex].EndPos) return true;
                return false;
            }
            #endregion

            #region 更新线程
            private void thread_UpdateDataChangeBackColor()
            {
                Predicate<ListSubItems> pre = new Predicate<ListSubItems>(findSubItems);

                while (listChange.Count != 0)
                {
                    //判断线程是否存活,
                    if (pic.IsDisposed) break;

                    //执行绘画方法
                    Draw(drawType.updateData);

                    //--先删除的原因是因为不更新背景颜色的列需要该方法绘制，然后绘制完之后不能减低颜色（颜色已经为0，会出错）
                    //根据判断条件删除
                    listChange.RemoveAll(pre);
                    GC.Collect();

                    //减低颜色
                    foreach (ListSubItems l in listChange)
                    {
                        l.downColor();
                    }

                    Thread.Sleep(250);
                }
            }
            #endregion

            #region 接口API
            /// <summary>
            /// 更新行情列表数据
            /// </summary>
            /// <param name="columnName">列名称</param>
            /// <param name="commodityName">商品名称</param>
            /// <param name="value">更新值</param>
            /// <returns>返回是否更新成功</returns>
            public void UpdateData(string columnName, string commodityName, string value)
            {
                //如果更新数据为当前列则排序
                if (SortStatu != sortType.None && columnIndexSort == dm.getListColumnIndex(columnName))
                {
                    commodity_sort();
                }

                //判断是否已经存在于更新队列
                ListSubItems ls = dm.getCommodity(commodityName).Items[dm.getListColumnIndex(columnName)];
                if (listChange.IndexOf(ls) == -1)
                {
                    RectangleF rec = dm.listColumn[dm.getCommodity(commodityName).Items.IndexOf(ls)].getRectangleF((dm.getCommodityIndex(commodityName) + 1 - rowStartIndex) * staPara.rowHeight);

                    //判断矩形是否超界或者当前并不显示
                    if (checkRecisOut(rec) || isShowed == false)
                    {
                        //清空颜色
                        ls.ClearColor();
                    }
                    else
                    {
                        //添加到更新列表
                        listChange.Add(ls);
                        //启动更新线程
                        startUpdateThread();
                    }
                }
            }

            /// <summary>
            /// 显示行情列表
            /// </summary>
            public void Show()
            {
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
            #endregion

            #region 枚举类型
            //绘画类型
            private enum drawType
            {
                /// <summary>
                /// 画布初始化，全部重绘
                /// </summary>
                redraw,
                /// <summary>
                /// 绘画鼠标调整线
                /// </summary>
                drawColumnAdjustLine,
                /// <summary>
                /// 绘画行选择线
                /// </summary>
                drawRowSelectLine,
                /// <summary>
                /// 列宽调整重绘
                /// </summary>
                ColumnAdjust,
                /// <summary>
                /// 清空操作线图层
                /// </summary>
                ClearMouseGra,
                /// <summary>
                /// 更新数据
                /// </summary>
                updateData
            }

            //排序类型
            private enum sortType
            {
                /// <summary>
                /// 升序
                /// </summary>
                Asc,
                /// <summary>
                /// 降序
                /// </summary>
                Desc,
                /// <summary>
                /// 不排序
                /// </summary>
                None
            }

            //鼠标点击状态
            private enum MouseClickType
            {
                /// <summary>
                /// 没有点击
                /// </summary>
                None,
                /// <summary>
                /// 列线
                /// </summary>
                ColumnLine,
                /// <summary>
                /// 水平滚动条
                /// </summary>
                HScroll,
                /// <summary>
                /// 垂直滚动条
                /// </summary>
                VScroll,
                /// <summary>
                /// 滚动条左按钮
                /// </summary>
                LeftButton,
                /// <summary>
                /// 滚动条右按钮
                /// </summary>
                RightButton,
                /// <summary>
                /// 滚动条上按钮
                /// </summary>
                UpButton,
                /// <summary>
                /// 滚动条下按钮
                /// </summary>
                DownButton
            }
            #endregion
        }
    }
}
