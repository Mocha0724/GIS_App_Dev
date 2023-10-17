using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace MapObjects
{
    public class Layer
    {
        private Type _ShapeType = typeof(MapObjects.Geometry);   //要素集合类型
        private string _Name = "Untitled";  //图层名称
        private bool _Visible = true;   //是否可见
        private bool _Selectable = true;    //是否可选择
        private string _Description = "";   //描述
        private bool _IsDirty = false;  //是否被修改过
        private Fields _AttributeFields = new Fields();    //字段集合
        private Features _Features = new Features();    //要素集合
        private Features _SelectedFeatures = new Features();    //选择要素集合
        private GeoRectangle _Extent = new GeoRectangle(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);  //图层范围
        private Renderer _Renderer;   //图层渲染对象
        //private LabelRenderer _LabelRenderer; //注记渲染对象
        public bool LabelVisible = false;

        private bool IsDataBase = false;
        #region 属性

        /// <summary>
        /// 获取图层的要素几何类型
        /// </summary>
        public Type ShapeType
        {
            get { return _ShapeType; }
        }

        /// <summary>
        /// 获取或设置图层名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// 指示图层是否可见
        /// </summary>
        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        /// <summary>
        /// 指示图层是否可以进行选择操作
        /// </summary>
        public bool Selectable
        {
            get { return _Selectable; }
            set { _Selectable = value; }
        }

        /// <summary>
        /// 获取或设置描述
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        /// <summary>
        /// 指示图层是否被修改过
        /// </summary>
        public bool IsDirty
        {
            get { return _IsDirty; }
            set { _IsDirty = value; }
        }

        /// <summary>
        /// 获取图层范围
        /// </summary>
        public GeoRectangle Extent
        {
            get { return _Extent; }
        }

        /// <summary>
        /// 获取或设置要素集合
        /// </summary>
        public Features Features
        {
            get { return _Features; }
            set
            {
                _Features = value;
                CalExtent();
            }
        }

        /// <summary>
        /// 获取或设置选择要素集合
        /// </summary>
        public Features SelectedFeatures
        {
            get { return _SelectedFeatures; }
            set { _SelectedFeatures = value; }
        }

        /// <summary>
        /// 获取属性字段集合
        /// </summary>
        public Fields AttributeFields
        {
            get { return _AttributeFields; }
        }

        /// <summary>
        /// 获取或设置图层渲染
        /// </summary>
        public Renderer Renderer
        {
            get { return _Renderer; }
            set { _Renderer = value; }
        }

        /// <summary>
        /// 获取或设置图层注记渲染
        /// </summary>
        /*public moLabelRenderer LabelRenderer
        {
            get { return _LabelRenderer; }
            set { _LabelRenderer = value; }
        }*/

        #endregion

        #region 构造函数
        public Layer()
        {
            Initialize();
        }

        public Layer(string name, Type shapeType)
        {
            _Name = name;
            _ShapeType = shapeType;
            Initialize();
        }
        #endregion


        #region public methods

        public void UpdateExtent()
        {
            CalExtent();
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            _SelectedFeatures.Clear();
        }

        public void DrawFeatures(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu)
        {
            //为所有要素配置符号
            //SetFeatureSymbols();
            //判断是否位于绘制范围内，如是，则绘制
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                Feature sFeature = _Features.GetItem(i);
                if (IsFeatureInExtent(sFeature, extent) == true)
                {
                    Geometry sGeometry = _Features.GetItem(i).Geometry;
                    Symbol sSymbol = _Features.GetItem(i).Symbol;
                    MapDrawingTool.DrawGeometry(g, extent, mapScale, dpm, mpu, sGeometry, sSymbol);
                }
            }
        }

        //绘制指定范围内的所有选择要素
        public void DrawSelectedFeatures(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, Symbol symbol)
        {
            //判断是否位于绘制范围内，如是，则绘制
            Int32 sFeatureCount = _SelectedFeatures.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                Feature sFeature = _SelectedFeatures.GetItem(i);
                if (IsFeatureInExtent(sFeature, extent) == true)
                {
                    Geometry sGeometry = _SelectedFeatures.GetItem(i).Geometry;
                    MapDrawingTool.DrawGeometry(g, extent, mapScale, dpm, mpu, sGeometry, symbol);
                }
            }
        }

        /// <summary>
        /// 根据矩形盒执行搜索
        /// </summary>
        /// <param name="selectingBox"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public Features SearchByBox(GeoRectangle selectingBox, double tolerance)
        {
            //说明：出于简化，仅考虑一种选择模式
            Features sSelection = null;
            if (selectingBox.Width == 0 && selectingBox.Height == 0)
            {
                //按点选
                GeoPoint sSelectingPoint = new GeoPoint(selectingBox.MinX, selectingBox.MinY);
                sSelection = SearchFeaturesByPoint(sSelectingPoint, tolerance);
            }
            else
            {
                //按框选
                sSelection = SearchFeaturesByBox(selectingBox);
            }
            return sSelection;
        }

        //根据指定方法执行选择（如新建、求并、求交、求差）
        public void ExecuteSelect(Features features, Int32 selectMethod)
        {
            //说明：此处仅新建集合
            if (selectMethod == 0)
            {
                _SelectedFeatures.Clear();
                Int32 sFeatureCount = features.Count;
                for (Int32 i = 0; i <= sFeatureCount - 1; i++)
                {
                    _SelectedFeatures.Add(features.GetItem(i));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region private methods
        //绘制所有要素



        private void Initialize()
        {
            //加入_AttributesFields对象的事件
            _AttributeFields.FieldAppended += _AttributesFields_FieldAppended;
            _AttributeFields.FieldRemoved += _AttributesFields_FieldRemoved;
            //初始化图层渲染
            InitializeRenderer();
        }

        private void InitializeRenderer()
        {
            SimpleRenderer sRenderer = new SimpleRenderer();
            if (_ShapeType == typeof(GeoPoint))
            {
                sRenderer.Symbol = new SimpleMarkerSymbol();
                _Renderer = sRenderer;
            }
            else if (_ShapeType == typeof(GeoPolyline))
            {
                sRenderer.Symbol = new SimpleLineSymbol();
                _Renderer = sRenderer;
            }
            else
            {
                sRenderer.Symbol = new SimpleFillSymbol();
                _Renderer = sRenderer;
            }
        }


        //有字段被加入
        private void _AttributesFields_FieldAppended(object sender, Field fieldAppended)
        {
            //给所有要素增加一个属性值
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                Feature sFeature = _Features.GetItem(i);
                if (fieldAppended.ValueType == typeof(Int16))
                {
                    Int16 sValue = 0;
                    sFeature.Attributes.Append(sValue);
                }
                else if (fieldAppended.ValueType == typeof(Int32))
                {
                    Int32 sValue = 0;
                    sFeature.Attributes.Append(sValue);
                }
                else if (fieldAppended.ValueType == typeof(Int64))
                {
                    Int64 sValue = 0;
                    sFeature.Attributes.Append(sValue);
                }
                else if (fieldAppended.ValueType == typeof(float))
                {
                    float sValue = 0;
                    sFeature.Attributes.Append(sValue);
                }
                else if (fieldAppended.ValueType == typeof(double))
                {
                    double sValue = 0;
                    sFeature.Attributes.Append(sValue);
                }
                else if (fieldAppended.ValueType == typeof(string))
                {
                    string sValue = string.Empty;
                    sFeature.Attributes.Append(sValue);
                }
                else
                {
                    throw new Exception("Invalid field value type!");
                }
            }
        }

        //有字段被删除
        private void _AttributesFields_FieldRemoved(object sender, int fieldIndex, Field fieldRemoved)
        {
            //删除所有要素对应字段的属性值
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                Feature sFeature = _Features.GetItem(i);
                sFeature.Attributes.RemoveAt(fieldIndex);
            }
        }

        //计算图层范围
        private void CalExtent()
        {
            double sMinX = Double.MaxValue;
            double sMaxX = Double.MinValue;
            double sMinY = Double.MaxValue;
            double sMaxY = Double.MinValue;
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                GeoRectangle sExtent = _Features.GetItem(i).GetEnvelope();
                if (sExtent.MinX < sMinX)
                    sMinX = sExtent.MinX;
                if (sExtent.MaxX > sMaxX)
                    sMaxX = sExtent.MaxX;
                if (sExtent.MinY < sMinY)
                    sMinY = sExtent.MinY;
                if (sExtent.MaxY > sMaxY)
                    sMaxY = sExtent.MaxY;
            }
            _Extent = new GeoRectangle(sMinX, sMaxX, sMinY, sMaxY);
        }

        private bool IsFeatureInExtent(Feature feature, GeoRectangle extent)
        {
            GeoRectangle sRect = feature.GetEnvelope();
            if (sRect.MaxX < extent.MinX || sRect.MinX > extent.MaxX)
            { return false; }
            else if (sRect.MaxY < extent.MinY || sRect.MinY > extent.MaxY)
            { return false; }
            else
            { return true; }
        }


        //根据点搜索要素
        private Features SearchFeaturesByPoint(GeoPoint point, double tolerance)
        {
            Features sSelectedFeatures = new Features();
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                if (_ShapeType == typeof(GeoPoint))
                {
                    GeoPoint sPoint = (GeoPoint)_Features.GetItem(i).Geometry;
                    if (MapTool.IsPointOnPoint(point, sPoint, tolerance) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
                else if (_ShapeType == typeof(GeoPolyline))
                {
                    GeoPolyline sMultiPolyline = (GeoPolyline)_Features.GetItem(i).Geometry;
                    if (MapTool.IsPointOnMultiPolyline(point, sMultiPolyline, tolerance) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
                else if (_ShapeType == typeof(GeoPolygon))
                {
                    GeoPolygon sMultiPolygon = (GeoPolygon)_Features.GetItem(i).Geometry;
                    if (MapTool.IsPointWithinMultiPolygon(point, sMultiPolygon) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
            }
            return sSelectedFeatures;
        }

        //根据矩形盒搜索要素
        private Features SearchFeaturesByBox(GeoRectangle selectingBox)
        {
            Features sSelectedFeatures = new Features();
            Int32 sFeatureCount = _Features.Count;
            for (Int32 i = 0; i <= sFeatureCount - 1; i++)
            {
                if (_ShapeType == typeof(GeoPoint))
                {
                    GeoPoint sPoint = (GeoPoint)_Features.GetItem(i).Geometry;
                    if (MapTool.IsPointWithinBox(sPoint, selectingBox) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
                else if (_ShapeType == typeof(GeoPolyline))
                {
                    GeoPolyline sMultiPolyline = (GeoPolyline)_Features.GetItem(i).Geometry;
                    if (MapTool.IsMultiPolylinePartiallyWithinBox(sMultiPolyline, selectingBox) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
                else if (_ShapeType == typeof(GeoPolygon))
                {
                    GeoPolygon sMultiPolygon = (GeoPolygon)_Features.GetItem(i).Geometry;
                    if (MapTool.IsMultiPolygonPartiallyWithinBox(sMultiPolygon, selectingBox) == true)
                    {
                        sSelectedFeatures.Add(_Features.GetItem(i));
                    }
                }
            }
            return sSelectedFeatures;
        }

        #endregion



    }
}
