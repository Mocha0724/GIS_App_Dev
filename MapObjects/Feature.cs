using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MapObjects
{
    public class Feature
    {
        #region 字段

        private Geometry _Geometry;   //几何图形
        private Attributes _Attributes;   //属性集合
        private bool _Selected;     //是否被选中
        private Type _ShapeType;
        private Symbol _Symbol;

        #endregion
        #region 构造函数

        public Feature( Geometry geometry, Attributes attributes)
        {

            _Geometry = geometry;
            _Attributes = attributes;
            _Selected = false;
            _ShapeType = geometry.GetType();
        }

        #endregion
        #region 属性


        public Geometry Geometry
        {
            get { return _Geometry; }
            set { _Geometry = value; }
        }

        public Attributes Attributes
        {
            get { return _Attributes; }
            set { _Attributes = value; }
        }

        public Type ShapeType
        {
            get { return _ShapeType; }
        }

        
        public Symbol Symbol
        {
            get { return _Symbol; }
            set { _Symbol = value; }
        }

        public bool Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }
        #endregion
        #region 方法

        /// <summary>
        /// 获取要素的外包矩形
        /// </summary>
        /// <returns></returns>
        public GeoRectangle GetEnvelope()
        {
            GeoRectangle sRect = null;
            sRect = _Geometry.GetEnvelope();

            return sRect;
        }

        public Feature Clone()
        {
            //GeometryTypeConstant sShapeType = _ShapeType;
            Geometry sGeometry = _Geometry;
            Attributes sAttributes = _Attributes.Clone();
            Feature sFeature = new Feature(sGeometry, sAttributes);
            return sFeature;
        }

        #endregion
        #region 私有函数
        #endregion
    }
}
