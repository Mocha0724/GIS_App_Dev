using MyMapObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    internal static class MapDrawingTool
    {
        internal static void DrawGeometry(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, Geometry geometry, moSymbol symbol)
        {
            if (extent == null)
                return;
            if (geometry == null)
                return;
            if (symbol == null)
                return;
            if (geometry.GetType() == typeof(GeoMultiPoint))
            {
                GeoMultiPoint sPoint = (GeoMultiPoint)geometry;
                DrawMultiPoint(g, extent, mapScale, dpm, mpu, sPoint, symbol);
            }
            else if (geometry.GetType() == typeof(GeoPolyline))
            {
                GeoPolyline sMultiPolyline = (GeoPolyline)geometry;
                DrawMultiPolyline(g, extent, mapScale, dpm, mpu, sMultiPolyline, symbol);
            }
            else if (geometry.GetType() == typeof(GeoPolygon))
            {
                GeoPolygon sMultiPolygon = (GeoPolygon)geometry;
                DrawMultiPolygon(g, extent, mapScale, dpm, mpu, sMultiPolygon, symbol);
            }
        }

        //绘制点
        internal static void DrawPoint(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoint point, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleMarkerSymbol)
            {
                SimpleMarkerSymbol sSymbol = (SimpleMarkerSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawPointBySimpleMarker(g, extent, mapScale, dpm, mpu, point, sSymbol);
            }
        }

        internal static void DrawLine(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoint point1,GeoPoint point2, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sSymbol = (SimpleLineSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawLineBySimpleLine(g, extent, mapScale, dpm, mpu, point1, point2, sSymbol);
            }
        }
        //绘制点集合（多点）
        internal static void DrawPoints(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoints points, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleMarkerSymbol)
            {
                SimpleMarkerSymbol sSymbol = (SimpleMarkerSymbol)symbol;
                if (sSymbol.Visible == true)
                {
                    Int32 sPointCount = points.Count;
                    for (Int32 i = 0; i <= sPointCount - 1; i++)
                    {
                        GeoPoint sPoint = points.GetItem(i);
                        DrawPointBySimpleMarker(g, extent, mapScale, dpm, mpu, sPoint, sSymbol);
                    }
                }
            }
        }







        //采用简单点符号绘制点
        private static void DrawPointBySimpleMarker(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPoint point, SimpleMarkerSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            PointF sScreenPoint = new PointF();
            sScreenPoint.X = (float)((point.X - sOffsetX) * mpu / mapScale * dpm);
            sScreenPoint.Y = (float)((sOffsetY - point.Y) * mpu / mapScale * dpm);
            //（2）计算符号大小
            float sSize = (float)(symbol.Size / 1000 * dpm);     //符号大小，像素
            if (sSize < 1)
                sSize = 1;
            //（3）定义绘制区域并绘制
            Rectangle sDrawingArea = new Rectangle((Int32)(sScreenPoint.X - sSize / 2), (Int32)(sScreenPoint.Y - sSize / 2), (Int32)sSize, (Int32)sSize);
            DrawSimpleMarker(g, sDrawingArea, dpm, symbol);
        }

        //采用简单线符号绘制线段
        private static void DrawLineBySimpleLine(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPoint point1, GeoPoint point2, SimpleLineSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            PointF sScreenPoint1 = new PointF();
            PointF sScreenPoint2 = new PointF();
            sScreenPoint1.X = (float)((point1.X - sOffsetX) * mpu / mapScale * dpm);
            sScreenPoint1.Y = (float)((sOffsetY - point1.Y) * mpu / mapScale * dpm);
            sScreenPoint2.X = (float)((point2.X - sOffsetX) * mpu / mapScale * dpm);
            sScreenPoint2.Y = (float)((sOffsetY - point2.Y) * mpu / mapScale * dpm);
            //（2）绘制
            Pen sPen = new Pen(symbol.Color, (float)(symbol.Size / 1000 * dpm));
            sPen.DashStyle = (DashStyle)symbol.Style;
            g.DrawLine(sPen, sScreenPoint1, sScreenPoint2);
            sPen.Dispose();
        }

        //绘制简单点符号
        private static void DrawSimpleMarker(Graphics g, Rectangle drawingArea, double dpm, SimpleMarkerSymbol symbol)
        {
            if (symbol.Style == SimpleMarkerSymbolStyleConstant.Circle)
            {
                Pen sPen = new Pen(symbol.Color);
                g.DrawEllipse(sPen, drawingArea);
                sPen.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.SolidCircle)
            {
                SolidBrush sBrush = new SolidBrush(symbol.Color);
                g.FillEllipse(sBrush, drawingArea);
                sBrush.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.Triangle)
            {
                Pen sPen = new Pen(symbol.Color);
                DrawTriangle(drawingArea, g, sPen);
                sPen.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.SolidTriangle)
            {
                SolidBrush sBrush = new SolidBrush(symbol.Color);
                FillTriangle(drawingArea, g, sBrush);
                sBrush.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.Square)
            {
                Pen sPen = new Pen(symbol.Color);
                g.DrawRectangle(sPen, drawingArea);
                sPen.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.SolidSquare)
            {
                SolidBrush sBrush = new SolidBrush(symbol.Color);
                g.FillRectangle(sBrush, drawingArea);
                sBrush.Dispose();

            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.CircleDot)
            {
                Pen sPen = new Pen(symbol.Color);
                SolidBrush sBrush = new SolidBrush(symbol.Color);
                g.DrawEllipse(sPen, drawingArea);
                g.FillEllipse(sBrush, drawingArea.X + drawingArea.Width / 2, drawingArea.Y + drawingArea.Height / 2, 1, 1);
                sPen.Dispose();
            }
            else if (symbol.Style == SimpleMarkerSymbolStyleConstant.CircleCircle)
            {
                Pen sPen = new Pen(symbol.Color);
                g.DrawEllipse(sPen, drawingArea);
                g.DrawEllipse(sPen, GetHalfRectangle(drawingArea));
                sPen.Dispose();
            }
        }
        private static Rectangle GetHalfRectangle(Rectangle rectangle)
        {
            Point center = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            Point LeftTop = new Point(center.X - rectangle.Width / 4, center.Y - rectangle.Height / 4);
            Size size = new Size(rectangle.Width / 2, rectangle.Height / 2);
            Rectangle sRectangle = new Rectangle(LeftTop, size);
            return sRectangle;
        }
        private static void DrawTriangle(Rectangle rectangle, Graphics g, Pen pen)
        {
            Point p1 = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y);
            Point p2 = new Point(rectangle.X, rectangle.Y + rectangle.Height);
            Point p3 = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            Point[] sPointArray = new Point[3];
            sPointArray[0] = p1;
            sPointArray[1] = p2;
            sPointArray[2] = p3;
            g.DrawPolygon(pen, sPointArray);
        }
        private static void FillTriangle(Rectangle rectangle, Graphics g, SolidBrush brush)
        {
            Point p1 = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y);
            Point p2 = new Point(rectangle.X, rectangle.Y + rectangle.Height);
            Point p3 = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            Point[] sPointArray = new Point[3];
            sPointArray[0] = p1;
            sPointArray[1] = p2;
            sPointArray[2] = p3;
            g.FillPolygon(brush, sPointArray);
        }
    }
}
