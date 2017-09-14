using System;
using System.Collections.Generic;
using System.Drawing;

namespace GDIChart
{
    partial class StockChart
    {
        public class AreaClass
        {
            /// <summary>
            /// 构造函数(所有参数均默认为false)
            /// </summary>
            /// <param name="areaName">区域名称</param>
            /// <param name="inMiddle">是否在中间显示起始值</param>
            /// <param name="tagMaxMin">是否标记最大值和最小值</param>
            /// <param name="HeightPercent">高度百分比</param>
            public AreaClass(string areaName, bool inMiddle, bool tagMaxMin, float HeightPercent)
            {
                this._areaName = areaName;
                this.inMiddle = inMiddle;
                this.tagMaxMin = tagMaxMin;
                this._heightPercent = HeightPercent;
            }

            #region 成员变量
            private string _areaName;
            /// <summary>
            /// 区域名称
            /// </summary>
            public string areaName
            {
                get { return _areaName; }
            }

            private float _heightPercent;
            /// <summary>
            /// 高度百分比
            /// </summary>
            public float heightPercent
            {
                get { return _heightPercent; }
            }

            /// <summary>
            /// 起始线是否在中间
            /// </summary>
            public bool inMiddle = false;

            /// <summary>
            /// 是否标记最高值和最低值
            /// </summary>
            public bool tagMaxMin = false;

            /// <summary>
            /// 线信息列表
            /// </summary>
            public List<LineInfoClass> listLineInfo = new List<LineInfoClass>();

            /// <summary>
            /// 区域当前商品的最小值（只在绘图时ChartClass对象做临时记录，用于显示商品数值更新时判断是否超出范围需要重绘，类内部无用）
            /// </summary>
            public float minValue;

            /// <summary>
            /// 区域当前商品的最大值（只在绘图时ChartClass对象做临时记录，用于显示商品数值更新时判断是否超出范围需要重绘，类内部无用）
            /// </summary>
            public float maxValue;
            #endregion

            #region 公共接口API
            /// <summary>
            /// 设置高度百分比
            /// </summary>
            /// <param name="heightPercent">高度百分比</param>
            public void setHeightPercent(float heightPercent)
            {
                this._heightPercent = heightPercent;
            }

            /// <summary>
            /// 添加线信息（指定显示名称）
            /// </summary>
            /// <param name="lineName">线名称（唯一标识）</param>
            /// <param name="lineType">线类型</param>
            /// <param name="showName">显示名称</param>
            /// <param name="groupName">分组名称</param>
            /// <param name="lineColor">线颜色（如果为Null，则按颜色列表顺序显示）</param>
            public void addLineInfo(string lineName, LineType lineType, string showName, string groupName, Color? lineColor = null)
            {
                listLineInfo.Add(new LineInfoClass(lineName, lineType, showName, groupName, lineColor));
            }
            #endregion
        }
    }
}
