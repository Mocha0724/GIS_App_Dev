using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapObjects;
using MyMapObjects;


namespace GIS_APP_DEV
{
    public partial class MapControl : UserControl
    {
        #region 字段

        //（1）设计时属性变量
        private Color _SelectionColor = Color.Cyan; //绘制选择图形的颜色
        private Color _FlashColor = Color.Green;    //绘制闪烁图形的颜色
        //（2）运行时属性变量
        private Layers _Layers = new Layers();  //图层集合
        private ProjectionCS _ProjectionCS;
        //（3）模块级变量
        private MapDrawingReference mMapDrawingReference; //地图绘制参考对象
        private Bitmap mBufferMap1 = new Bitmap(10, 10);    //缓冲位图，仅含所有要素（含注记）
        private Bitmap mBufferMap2 = new Bitmap(10, 10);    //缓冲位图，含所有要素（含注记）及跟踪图形
        private Bitmap mBufferMap3 = new Bitmap(10, 10);    //在屏幕上移动BufferMap2时，先将2绘制到3上，再将3绘制到屏幕上，以避免清除屏幕带来的闪烁
        private SimpleMarkerSymbol mSelectedPointSymbol;  //选择点符号
        private SimpleLineSymbol mSelectedLineSymbol;     //选择线符号
        private SimpleFillSymbol mSelectedFillSymbol;     //选择面符号
        private ShapeFlashControler mFlashControl = new ShapeFlashControler();  //闪烁控制器
        private SimpleMarkerSymbol mFlashPointSymbol;     //闪烁点符号
        private SimpleLineSymbol mFlashLineSymbol;        //闪烁线符号
        private SimpleFillSymbol mFlashFillSymbol;        //闪烁面符号

        #endregion
        #region Initialize
        public MapControl()
        {
            InitializeComponent();
        }
        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置绘制选择要素的颜色
        /// </summary>
        [Browsable(true), Description("获取或设置绘制选择要素的颜色")]
        public Color SelectionColor
        {
            get { return _SelectionColor; }
            set { _SelectionColor = value; }
        }

        /// <summary>
        /// 获取或设置闪烁图形的颜色
        /// </summary>
        [Browsable(true), Description("获取或设置闪烁图形的颜色")]
        public Color FlashColor
        {
            get { return _FlashColor; }
            set { _FlashColor = value; }
        }

        /// <summary>
        /// 获取或设置投影坐标系统
        /// </summary>
        [Browsable(false)]
        public ProjectionCS ProjectionCS
        {
            get { return _ProjectionCS; }
            set
            {
                _ProjectionCS = value;
                mMapDrawingReference.mpu = _ProjectionCS.ToMeters(1);
            }
        }

        /// <summary>
        /// 获取图层集合
        /// </summary>
        [Browsable(false)]
        public Layers Layers
        {
            get { return _Layers; }
        }

        /// <summary>
        /// 获取地图比例尺倒数
        /// </summary>
        public double MapScale
        {
            get { return mMapDrawingReference.MapScale; }
        }

        /// <summary>
        /// 获取地图控件左上点的X坐标
        /// </summary>
        public double MapOffsetX
        {
            get { return mMapDrawingReference.OffsetX; }
        }

        /// <summary>
        /// 获取地图控件左上点的Y坐标
        /// </summary>
        public double MapOffsetY
        {
            get { return mMapDrawingReference.OffsetY; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取地图窗口对应的地图范围（地图坐标）
        /// </summary>
        /// <returns></returns>
        public GeoRectangle GetExtent()
        {
            //定义变量
            double sMinX = double.MaxValue, sMaxX = double.MinValue;
            double sMinY = double.MaxValue, sMaxY = double.MinValue;
            GeoRectangle sExtent;
            //如果工作区为空，则返回空矩形
            Rectangle sClientRect = this.ClientRectangle;
            if (sClientRect.IsEmpty == true)
            {
                sExtent = new GeoRectangle(sMinX, sMaxX, sMinY, sMaxY);
                return sExtent;
            }
            //定义工作区左上点和右下点的屏幕坐标
            GeoPoint sTopLeftScreenPoint = new GeoPoint(0, 0);
            GeoPoint sBottomRightScreenPoint = new GeoPoint(sClientRect.Width, sClientRect.Height);
            //获取工作区左上点和右下点的地图坐标
            GeoPoint sTopLeftMapPoint = mMapDrawingReference.ToMapPoint(sTopLeftScreenPoint.X, sTopLeftScreenPoint.Y);
            GeoPoint sBottomRightMapPoint = mMapDrawingReference.ToMapPoint(sBottomRightScreenPoint.X, sBottomRightScreenPoint.Y);
            //定义范围矩形
            sMinX = sTopLeftMapPoint.X;
            sMaxX = sBottomRightMapPoint.X;
            sMinY = sBottomRightMapPoint.Y;
            sMaxY = sTopLeftMapPoint.Y;
            sExtent = new GeoRectangle(sMinX, sMaxX, sMinY, sMaxY);
            return sExtent;
        }

        /// <summary>
        /// 获取地图范围（地图坐标）
        /// </summary>
        /// <returns></returns>
        public GeoRectangle GetFullExtent()
        {
            //（1）新建一个空矩形
            double sMinX = double.MaxValue, sMaxX = double.MinValue;
            double sMinY = double.MaxValue, sMaxY = double.MinValue;
            GeoRectangle sFullExtent;
            //（2）如果图层数量为0，则返回空矩形
            Int32 sLayerCount = _Layers.Count;
            if (sLayerCount == 0)
            {
                sFullExtent = new GeoRectangle(sMinX, sMaxX, sMinY, sMaxY);
                return sFullExtent;
            }
            //（3）计算范围矩形
            for (Int32 i = 0; i <= sLayerCount - 1; i++)
            {
                Layer sLayer = _Layers.GetItem(i);
                GeoRectangle sExtent = sLayer.Extent;
                if (sExtent.IsEmpty == false)
                {
                    if (sExtent.MinX < sMinX)
                        sMinX = sExtent.MinX;
                    if (sExtent.MaxX > sMaxX)
                        sMaxX = sExtent.MaxX;
                    if (sExtent.MinY < sMinY)
                        sMinY = sExtent.MinY;
                    if (sExtent.MaxY > sMaxY)
                        sMaxY = sExtent.MaxY;
                }
            }
            sFullExtent = new GeoRectangle(sMinX, sMaxX, sMinY, sMaxY);
            return sFullExtent;
        }
        /// <summary>
        /// 在窗口内显示地图全部范围
        /// </summary>
        public void FullExtent()
        {
            GeoRectangle sFullExtent = GetFullExtent();
            if (sFullExtent.IsEmpty == false)
            {
                Rectangle sClientRect = this.ClientRectangle;
                mMapDrawingReference.ZoomExtentToWindow(sFullExtent, sClientRect.Width, sClientRect.Height);
                this.UseWaitCursor = true;
                DrawBufferMap1();
                DrawBufferMap2();
                this.UseWaitCursor = false;
                Refresh();
                //触发事件
                if (MapScaleChanged != null)
                    MapScaleChanged(this);
            }
        }


        /// <summary>
        /// 设置地图范围
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="mapScale"></param>
        public void SetView(double offsetX, double offsetY, double mapScale)
        {
            mMapDrawingReference.SetView(offsetX, offsetY, mapScale);
        }

        /// <summary>
        /// 以指定中心和指定系数进行缩放
        /// </summary>
        /// <param name="center"></param>
        /// <param name="ratio"></param>
        public void ZoomByCenter(GeoPoint center, double ratio)
        {
            mMapDrawingReference.ZoomByCenter(center, ratio);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
            //触发事件
            if (MapScaleChanged != null)
                MapScaleChanged(this);
        }


        /// <summary>
        /// 在地图窗口内显示指定范围
        /// </summary>
        /// <param name="extent"></param>
        public void ZoomToExtent(GeoRectangle extent)
        {
            double sWindowWidth = this.ClientRectangle.Width;
            double sWindowHeight = this.ClientRectangle.Height;
            mMapDrawingReference.ZoomExtentToWindow(extent, sWindowWidth, sWindowHeight);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
            //触发事件
            if (MapScaleChanged != null)
                MapScaleChanged(this);
        }

        /// <summary>
        /// 将地图平移指定量
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void PanDelta(double deltaX, double deltaY)
        {
            mMapDrawingReference.PanDelta(deltaX, deltaY);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 将屏幕坐标转换为地图坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public GeoPoint ToMapPoint(double x, double y)
        {
            GeoPoint sPoint = mMapDrawingReference.ToMapPoint(x, y);
            return sPoint;
        }

        /// <summary>
        /// 将地图坐标转换为屏幕坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public GeoPoint FromMapPoint(double x, double y)
        {
            GeoPoint sPoint = mMapDrawingReference.FromMapPoint(x, y);
            return sPoint;
        }

        /// <summary>
        /// 将屏幕距离转换为地图距离
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public double ToMapDistance(double dis)
        {
            double sDis = mMapDrawingReference.ToMapDistance(dis);
            return sDis;
        }

        /// <summary>
        /// 将地图距离转换为屏幕距离
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public double FromMapDistance(double dis)
        {
            double sDis = mMapDrawingReference.FromMapDistance(dis);
            return sDis;
        }

        /// <summary>
        /// 重新绘制地图
        /// </summary>
        public void RedrawMap()
        {
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 重新绘制跟踪图形
        /// </summary>
        public void RedrawTrackingShapes()
        {
            this.UseWaitCursor = true;
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 将地图图像移动到指定位置（屏幕坐标）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void PanMapImageTo(float x, float y)
        {
            Graphics g = Graphics.FromImage(mBufferMap3);
            g.Clear(Color.White);
            g.DrawImage(mBufferMap2, x, y);
            g = Graphics.FromHwnd(this.Handle);
            g.DrawImage(mBufferMap3, 0, 0);
            g.Dispose();
        }

        /// <summary>
        /// 对指定的图形数组，以指定次数和时间间隔进行闪烁显示
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="times"></param>
        /// <param name="interval"></param>
        public void FlashShapes(Shape[] shapes, Int32 times, Int32 interval)
        {
            mFlashControl.StartFlash(shapes, times, interval);
        }

        /// <summary>
        /// 获取一个绘图工具
        /// </summary>
        /// <returns></returns>
        public UserDrawingTool GetDrawingTool()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            UserDrawingTool sDrawingTool = CreateDrawingTool(g);
            return sDrawingTool;
        }


        /// <summary>
        /// 根据指定选择盒与选择方法执行选择
        /// </summary>
        /// <param name="selectingBox"></param>
        /// <param name="tolerance"></param>
        /// <param name="selectMethod"></param>
        public void SelectByBox(GeoRectangle selectingBox, double tolerance, Int32 selectMethod)
        {
            Int32 sLayerCount = _Layers.Count;
            for (Int32 i = 0; i <= sLayerCount - 1; i++)
            {
                Layer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible == true && sLayer.Selectable == true)
                {
                    Features sFeatures = sLayer.SearchByBox(selectingBox, tolerance);
                    sLayer.ExecuteSelect(sFeatures, 0);
                }
                else
                {
                    sLayer.SelectedFeatures.Clear();
                }
            }
        }


        #region 事件

        public delegate void MapScaleChangedHandle(object sender);
        /// <summary>
        /// 地图比例尺发生了变化
        /// </summary>
        [Browsable(true), Description("地图比例尺发生了变化")]
        public event MapScaleChangedHandle MapScaleChanged;

        public delegate void AfterTrackingLayerDrawHandle(object sender, UserDrawingTool drawingTool);
        /// <summary>
        /// 地图跟踪图形已经绘制完毕
        /// </summary>
        [Browsable(true), Description("地图跟踪图形已经绘制完毕")]
        public event AfterTrackingLayerDrawHandle AfterTrackingLayerDraw;

        #endregion

        #region 母版和对象的事件处理



        //需要绘制闪烁图形
        private void MFlashControl_NeedDrawFlashShapes(object sender, Shape[] shapes)
        {
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            GeoRectangle sExtent = GetExtent();
            Graphics g = Graphics.FromImage(mBufferMap2);
            Int32 sShapeCount = shapes.Length;
            for (Int32 i = 0; i <= sShapeCount - 1; i++)
            {
                if (shapes[i].GetType() == typeof(GeoPoint))
                {
                    GeoPoint sPoint = (GeoPoint)shapes[i];
                    MapDrawingTool.DrawPoint(g, sExtent, sMapScale, dpm, mpu, sPoint, mFlashPointSymbol);
                }
                /*else if (shapes[i].GetType() == typeof(moPoints))
                {
                    moPoints sPoints = (moPoints)shapes[i];
                    MapDrawingTool.DrawPoints(g, sExtent, sMapScale, dpm, mpu, sPoints, mFlashPointSymbol);
                }*/
                else if (shapes[i].GetType() == typeof(GeoRectangle))
                {
                    GeoRectangle sRect = (GeoRectangle)shapes[i];
                    MapDrawingTool.DrawRectangle(g, sExtent, sMapScale, dpm, mpu, sRect, mFlashFillSymbol);
                }
                else if (shapes[i].GetType() == typeof(GeoPolyline))
                {
                    GeoPolyline sMultiPolyline = (GeoPolyline)shapes[i];
                    MapDrawingTool.DrawMultiPolyline(g, sExtent, sMapScale, dpm, mpu, sMultiPolyline, mFlashLineSymbol);
                }
                else if (shapes[i].GetType() == typeof(GeoPolygon))
                {
                    GeoPolygon sMultiPolygon = (GeoPolygon)shapes[i];
                    MapDrawingTool.DrawMultiPolygon(g, sExtent, sMapScale, dpm, mpu, sMultiPolygon, mFlashFillSymbol);
                }
            }
            g.Dispose();
            Refresh();
        }

        //需要清除闪烁图形
        private void MFlashControl_NeedClearFlashShapes(object sender)
        {
            RedrawTrackingShapes();
        }

        #endregion


        #region private
        //创建一个默认的投影坐标系统
        private void CreateDefaultProjectionCS()
        {
            string sProjCSName = "Beijing54 Lambert Conformal Conic 2SP";
            string sGeoCSName = "Beijing 1954";
            string sDatumName = "Beijing 1954";
            string sSpheroidName = "Krassowsky_1940";
            double sSemiMajor = 6378245;
            double sInverseFlattening = 298.3;
            double sOriginLatitude = 0;
            double sCentralMeridian = 105;
            double sFalseEasting = 0;
            double sFalseNorthing = 0;
            double sScaleFactor = 1;
            double sStandardParallelOne = 30;
            double sStandardParallelTwo = 62;
            LinearUnitConstant sLinearUnit = LinearUnitConstant.Meter;
            ProjectionTypeConstant sProjType = ProjectionTypeConstant.Lambert_Conformal_Conic_2SP;
            _ProjectionCS = new ProjectionCS(sProjCSName, sGeoCSName, sDatumName, sSpheroidName, sSemiMajor,
                sInverseFlattening, sProjType, sOriginLatitude, sCentralMeridian, sFalseEasting,
                sFalseNorthing, sScaleFactor, sStandardParallelOne, sStandardParallelTwo, sLinearUnit);
        }

        //创建地图-屏幕坐标转换对象
        private void CreateMapDrawingReference()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            double dpm = g.DpiX / 0.0254;
            g.Dispose();
            double mpu = _ProjectionCS.ToMeters(1);
            mMapDrawingReference = new MapDrawingReference(0, 0, 1000000, dpm, mpu);
        }




        //调整缓冲位图大小
        private void ResizeBufferMap()
        {
            Rectangle sClientRectangle = this.ClientRectangle;
            if (sClientRectangle.Width > 0 && sClientRectangle.Height > 0)
            {
                if (sClientRectangle.Width != mBufferMap1.Width || sClientRectangle.Height != mBufferMap1.Height)
                {
                    mBufferMap1 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                    mBufferMap2 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                    mBufferMap3 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                }
            }
        }

        //初始化符号
        private void InitializeSymbols()
        {
            //选择点符号
            mSelectedPointSymbol = new SimpleMarkerSymbol();
            mSelectedPointSymbol.Color = _SelectionColor;
            mSelectedPointSymbol.Size = 3;
            //选择线符号
            mSelectedLineSymbol = new SimpleLineSymbol();
            mSelectedLineSymbol.Color = _SelectionColor;
            mSelectedLineSymbol.Size = 3000 / mMapDrawingReference.dpm;
            //选择面符号
            mSelectedFillSymbol = new SimpleFillSymbol();
            mSelectedFillSymbol.Color = Color.Transparent;
            mSelectedFillSymbol.Outline.Color = _SelectionColor;
            mSelectedFillSymbol.Outline.Size = 3000 / mMapDrawingReference.dpm;
            //闪烁点符号
            mFlashPointSymbol = new SimpleMarkerSymbol();
            mFlashPointSymbol.Color = _FlashColor;
            mFlashPointSymbol.Style = SimpleMarkerSymbolStyleConstant.SolidCircle;
            mFlashPointSymbol.Size = 3;
            //闪烁线符号
            mFlashLineSymbol = new SimpleLineSymbol();
            mFlashLineSymbol.Color = _FlashColor;
            mFlashLineSymbol.Size = 1.5;
            //闪烁面符号
            mFlashFillSymbol = new SimpleFillSymbol();
            mFlashFillSymbol.Color = _FlashColor;
            mFlashFillSymbol.Outline.Color = Color.Black;
            mFlashFillSymbol.Outline.Size = 0.35;
        }



        //绘制缓冲位图1
        private void DrawBufferMap1()
        {
            //（1）获取地图窗口的范围
            GeoRectangle sExtent = GetExtent();
            if (sExtent.IsEmpty == true)
                return;
            //（2）绘制所有图层的要素，采用倒序
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            Graphics g = Graphics.FromImage(mBufferMap1);
            g.Clear(Color.White);
            Int32 sLayerCount = _Layers.Count;
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                Layer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible == true)
                {
                    sLayer.DrawFeatures(g, sExtent, sMapScale, dpm, mpu);
                }
            }
            //（3）绘制所有图层的注记，依然倒序
            /*
            List<RectangleF> sPlacedLabelExtents = new List<RectangleF>();
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                Layer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible == true)
                {
                    sLayer.DrawLabels(g, sExtent, sMapScale, dpm, mpu, sPlacedLabelExtents);
                }
            }*/
            g.Dispose();
        }

        //绘制缓冲位图2
        private void DrawBufferMap2()
        {
            //（1）获取地图窗口的范围
            GeoRectangle sExtent = GetExtent();
            if (sExtent.IsEmpty == true)
                return;
            //（2）绘制缓冲位图1
            Graphics g = Graphics.FromImage(mBufferMap2);
            g.Clear(Color.White);
            Rectangle sRect = new Rectangle(0, 0, mBufferMap1.Width, mBufferMap1.Height);
            g.DrawImage(mBufferMap1, sRect, sRect, GraphicsUnit.Pixel);
            //（3）绘制所有图层的选择要素，采用倒序
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            Int32 sLayerCount = _Layers.Count;
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                Layer sLayer = _Layers.GetItem(i);
                if (sLayer.ShapeType == typeof(GeoPoint))
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedPointSymbol);
                }
                else if (sLayer.ShapeType == typeof(GeoPolyline))
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedLineSymbol);
                }
                else if (sLayer.ShapeType == typeof(GeoPolygon))
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedFillSymbol);
                }
            }
            //（4）触发事件，以便用户程序继续绘图
            if (AfterTrackingLayerDraw != null)
            {
                //新建绘图工具
                UserDrawingTool sDrawingTool = CreateDrawingTool(g);
                AfterTrackingLayerDraw(this, sDrawingTool);
            }
            g.Dispose();
        }

        //生成一个用户绘图工具
        private UserDrawingTool CreateDrawingTool(Graphics graphics)
        {
            GeoRectangle sExtent = GetExtent();
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            UserDrawingTool sDrawingTool = new UserDrawingTool(graphics, sExtent, sMapScale, dpm, mpu);
            return sDrawingTool;
        }
        #endregion

        private void MapControl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(mBufferMap2, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        private void MapControl_Resize(object sender, EventArgs e)
        {
            ResizeBufferMap();
        }
        #endregion
    }
}
