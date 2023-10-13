using System;
using System.Collections.Generic;
using System.Text;


namespace MyMapObjects
{

    public enum SymbolTypeConstant
    {
        SimpleMarkerSymbol = 0,
        SimpleLineSymbol = 1,
        SimpleFillSymbol = 2
    }

    /// <summary>
    /// 简单点符号形状常数
    /// </summary>
    public enum SimpleMarkerSymbolStyleConstant
    {
        Circle = 0,
        SolidCircle = 1,
        Triangle = 2,
        SolidTriangle = 3,
        Square = 4,
        SolidSquare = 5,
        CircleDot = 6,
        CircleCircle = 7
    }

    public enum SimpleLineSymbolStyleConstant
    {
        Solid = 0,
        Dash = 1,
        Dot = 2,
        DashDot = 3,
        DashDotDot = 4
    }


    /// <summary>
    /// 图层渲染类型常数
    /// </summary>
    public enum RendererTypeConstant
    {
        Simple = 0,
        UniqueValue = 1,
        ClassBreaks = 2
    }

    /// <summary>
    /// 文本符号布局常数
    /// </summary>
    public enum TextSymbolAlignmentConstant
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        CenterLeft = 3,
        CenterCenter = 4,
        CenterRight = 5,
        BottomLeft = 6,
        BottomCenter = 7,
        BottomRight = 8
    }

    /// <summary>
    /// 投影类型常数
    /// </summary>
    public enum ProjectionTypeConstant
    {
        None = 0,
        Mercator = 1,
        UTM = 2,
        Gauss_Kruger = 3,
        Lambert_Conformal_Conic_2SP = 4,
        Albers_Equal_Area = 5
    }

    /// <summary>
    /// 线性单位常数
    /// </summary>
    public enum LinearUnitConstant
    {
        Millimeter = 0,
        Centimeter = 0,
        Decimeter = 2,
        Meter = 3,
        Kilometers = 4
    }
}