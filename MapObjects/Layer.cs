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

        internal void DrawFeatures(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu)
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
        internal void DrawSelectedFeatures(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu, Symbol symbol)
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

        #endregion



    }
}
