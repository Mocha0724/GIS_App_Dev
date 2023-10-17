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
    public static class MapDrawingTool
    {
        public static void DrawGeometry(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, Geometry geometry, Symbol symbol)
        {
            if (extent == null)
                return;
            if (geometry == null)
                return;
            if (symbol == null)
                return;
            if (geometry.GetType() == typeof(GeoPoint))
            {
                GeoPoint sPoint = (GeoPoint)geometry;
                DrawPoint(g, extent, mapScale, dpm, mpu, sPoint, symbol);
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
        public static void DrawPoint(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoint point, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleMarkerSymbol)
            {
                SimpleMarkerSymbol sSymbol = (SimpleMarkerSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawPointBySimpleMarker(g, extent, mapScale, dpm, mpu, point, sSymbol);
            }
        }

        public static void DrawLine(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoint point1,GeoPoint point2, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sSymbol = (SimpleLineSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawLineBySimpleLine(g, extent, mapScale, dpm, mpu, point1, point2, sSymbol);
            }
        }
        //绘制点集合（多点）
        public static void DrawPoints(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoints points, Symbol symbol)
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

        //绘制简单折线
        public static void DrawPolyline(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoints points, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sSymbol = (SimpleLineSymbol)symbol;
                if (sSymbol.Visible == true)
                {
                    DrawPolylineBySimpleLine(g, extent, mapScale, dpm, mpu, points, sSymbol);
                }
            }
        }

        //绘制简单多边形
        public static void DrawPolygon(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPoints points, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleFillSymbol)
            {
                SimpleFillSymbol sSymbol = (SimpleFillSymbol)symbol;
                if (sSymbol.Visible == true)
                {
                    DrawPolygonBySimpleFill(g, extent, mapScale, dpm, mpu, points, sSymbol);
                }
            }
        }

        //绘制复合折线
        public static void DrawMultiPolyline(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPolyline multiPolyline, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sSymbol = (SimpleLineSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawMultiPolylineBySimpleLine(g, extent, mapScale, dpm, mpu, multiPolyline, sSymbol);
            }
        }

        //绘制复合多边形
        public static void DrawMultiPolygon(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoPolygon multiPolygon, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleFillSymbol)
            {
                SimpleFillSymbol sSymbol = (SimpleFillSymbol)symbol;
                if (sSymbol.Visible == true)
                    DrawMultiPolygonBySimpleFill(g, extent, mapScale, dpm, mpu, multiPolygon, sSymbol);
            }
        }



        //绘制矩形
        public static void DrawRectangle(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, GeoRectangle rectangle, Symbol symbol)
        {
            if (symbol.SymbolType == SymbolTypeConstant.SimpleFillSymbol)
            {
                SimpleFillSymbol sSymbol = (SimpleFillSymbol)symbol;
                if (sSymbol.Visible == true)
                {
                    DrawRectangleBySimpleFill(g, extent, mapScale, dpm, mpu, rectangle, sSymbol);
                }
            }
        }



        #region private methods
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

        //采用简单线符号绘制简单折线
        private static void DrawPolylineBySimpleLine(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPoints points, SimpleLineSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            GraphicsPath sGraphicPath = new GraphicsPath();     //用于屏幕绘制
            Int32 sPointCount = points.Count;  //顶点数目
            PointF[] sScreenPoints = new PointF[sPointCount];
            for (Int32 j = 0; j <= sPointCount - 1; j++)
            {
                PointF sScreenPoint = new PointF();
                GeoPoint sCurPoint = points.GetItem(j);
                sScreenPoint.X = (float)((sCurPoint.X - sOffsetX) * mpu / mapScale * dpm);
                sScreenPoint.Y = (float)((sOffsetY - sCurPoint.Y) * mpu / mapScale * dpm);
                sScreenPoints[j] = sScreenPoint;
            }
            sGraphicPath.AddLines(sScreenPoints);
            //（2）绘制
            Pen sPen = new Pen(symbol.Color, (float)(symbol.Size / 1000 * dpm));
            sPen.DashStyle = (DashStyle)symbol.Style;
            g.DrawPath(sPen, sGraphicPath);
            sPen.Dispose();
        }

        //采用简单填充符号绘制简单多边形
        private static void DrawPolygonBySimpleFill(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPoints points, SimpleFillSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            GraphicsPath sGraphicPath = new GraphicsPath();     //用于屏幕绘制
            Int32 sPointCount = points.Count;  //顶点数目
            PointF[] sScreenPoints = new PointF[sPointCount];
            for (Int32 j = 0; j <= sPointCount - 1; j++)
            {
                PointF sScreenPoint = new PointF();
                GeoPoint sCurPoint = points.GetItem(j);
                sScreenPoint.X = (float)((sCurPoint.X - sOffsetX) * mpu / mapScale * dpm);
                sScreenPoint.Y = (float)((sOffsetY - sCurPoint.Y) * mpu / mapScale * dpm);
                sScreenPoints[j] = sScreenPoint;
            }
            sGraphicPath.AddPolygon(sScreenPoints);
            //（2）填充
            SolidBrush sBrush = new SolidBrush(symbol.Color);
            g.FillPath(sBrush, sGraphicPath);
            sBrush.Dispose();
            //（3）绘制边界
            if (symbol.Outline.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sOutline = symbol.Outline;
                if (sOutline.Visible == true)
                {
                    Pen sPen = new Pen(sOutline.Color, (float)(sOutline.Size / 1000 * dpm));
                    sPen.DashStyle = (DashStyle)sOutline.Style;
                    g.DrawPath(sPen, sGraphicPath);
                    sPen.Dispose();
                }
            }
        }

        //采用简单线符号绘制复合折线
        private static void DrawMultiPolylineBySimpleLine(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPolyline multiPolyline, SimpleLineSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            Int32 sPartCount = multiPolyline.Parts.Count;        //简单折线的数目
            GraphicsPath sGraphicPath = new GraphicsPath();     //定义复合多边形，用于屏幕绘制
            for (Int32 i = 0; i <= sPartCount - 1; i++)
            {
                Int32 sPointCount = multiPolyline.Parts.GetItem(i).Count;  //当前简单折线的顶点数目
                PointF[] sScreenPoints = new PointF[sPointCount];
                for (Int32 j = 0; j <= sPointCount - 1; j++)
                {
                    PointF sScreenPoint = new PointF();
                    GeoPoint sCurPoint = multiPolyline.Parts.GetItem(i).GetItem(j);
                    sScreenPoint.X = (float)((sCurPoint.X - sOffsetX) * mpu / mapScale * dpm);
                    sScreenPoint.Y = (float)((sOffsetY - sCurPoint.Y) * mpu / mapScale * dpm);
                    sScreenPoints[j] = sScreenPoint;
                }
                sGraphicPath.AddLines(sScreenPoints);
                sGraphicPath.StartFigure();
            }
            //（2）绘制
            Pen sPen = new Pen(symbol.Color, (float)(symbol.Size / 1000 * dpm));
            sPen.DashStyle = (DashStyle)symbol.Style;
            g.DrawPath(sPen, sGraphicPath);
            sPen.Dispose();
        }

        //采用简单填充符号绘制复合多边形
        private static void DrawMultiPolygonBySimpleFill(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoPolygon multiPolygon, SimpleFillSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标
            Int32 sPartCount = multiPolygon.Parts.Count;        //简单多边形的数目
            GraphicsPath sGraphicPath = new GraphicsPath();     //定义复合多边形，用于屏幕绘制
            for (Int32 i = 0; i <= sPartCount - 1; i++)
            {
                Int32 sPointCount = multiPolygon.Parts.GetItem(i).Count;  //当前简单多边形的顶点数目
                PointF[] sScreenPoints = new PointF[sPointCount];
                for (Int32 j = 0; j <= sPointCount - 1; j++)
                {
                    PointF sScreenPoint = new PointF();
                    GeoPoint sCurPoint = multiPolygon.Parts.GetItem(i).GetItem(j);
                    sScreenPoint.X = (float)((sCurPoint.X - sOffsetX) * mpu / mapScale * dpm);
                    sScreenPoint.Y = (float)((sOffsetY - sCurPoint.Y) * mpu / mapScale * dpm);
                    sScreenPoints[j] = sScreenPoint;
                }
                sGraphicPath.AddPolygon(sScreenPoints);
            }
            //（2）填充
            SolidBrush sBrush = new SolidBrush(symbol.Color);
            g.FillPath(sBrush, sGraphicPath);
            sBrush.Dispose();
            //（3）绘制边界
            if (symbol.Outline.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sOutline = symbol.Outline;
                if (sOutline.Visible == true)
                {
                    Pen sPen = new Pen(sOutline.Color, (float)(sOutline.Size / 1000 * dpm));
                    sPen.DashStyle = (DashStyle)sOutline.Style;
                    g.DrawPath(sPen, sGraphicPath);
                    sPen.Dispose();
                }
            }
        }

        //采用简单填充符号绘制矩形
        private static void DrawRectangleBySimpleFill(Graphics g, GeoRectangle extent, double mapScale, double dpm,
            double mpu, GeoRectangle rectangle, SimpleFillSymbol symbol)
        {
            double sOffsetX = extent.MinX, sOffsetY = extent.MaxY;  //获取投影坐标系相对屏幕坐标系的平移量
            //（1）转换为屏幕坐标并生成矩形
            Point sTopLeftPoint = new Point(), sBottomRightPoint = new Point();
            sTopLeftPoint.X = (Int32)((rectangle.MinX - sOffsetX) * mpu / mapScale * dpm);
            sTopLeftPoint.Y = (Int32)((sOffsetY - rectangle.MaxY) * mpu / mapScale * dpm);
            sBottomRightPoint.X = (Int32)((rectangle.MaxX - sOffsetX) * mpu / mapScale * dpm);
            sBottomRightPoint.Y = (Int32)((sOffsetY - rectangle.MinY) * mpu / mapScale * dpm);
            Int32 sWidth = sBottomRightPoint.X - sTopLeftPoint.X;
            Int32 sHeight = sBottomRightPoint.Y - sTopLeftPoint.Y;
            Rectangle sRect = new Rectangle(sTopLeftPoint.X, sTopLeftPoint.Y, sWidth, sHeight);
            //（2）填充
            if (symbol.Color != Color.Transparent)
            {
                SolidBrush sBrush = new SolidBrush(symbol.Color);
                g.FillRectangle(sBrush, sRect);
                sBrush.Dispose();
            }
            //（3）绘制边界
            if (symbol.Outline.SymbolType == SymbolTypeConstant.SimpleLineSymbol)
            {
                SimpleLineSymbol sOutline = symbol.Outline;
                if (sOutline.Visible == true)
                {
                    Pen sPen = new Pen(sOutline.Color, (float)(sOutline.Size / 1000 * dpm));
                    sPen.DashStyle = (DashStyle)sOutline.Style;
                    g.DrawRectangle(sPen, sRect);
                    sPen.Dispose();
                }
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

        #endregion
    }
}
