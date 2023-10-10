using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        
        //绘制所有要素
        internal void DrawFeatures(Graphics g, GeoRectangle extent, double mapScale, double dpm, double mpu)
        {

        }

        #endregion

        #region private methods
        private void Initialize()
        {
            //加入_AttributesFields对象的事件
            //_AttributeFields.FieldAppended += _AttributesFields_FieldAppended;
            //_AttributeFields.FieldRemoved += _AttributesFields_FieldRemoved;
            //初始化图层渲染
            //InitializeRenderer();
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
        #endregion



    }
}
