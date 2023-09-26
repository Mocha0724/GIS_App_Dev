using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public abstract class Geometry
    {
        protected GeoParts _parts;
        protected double _MinX = double.MaxValue, _MaxX = double.MinValue;
        protected double _MinY = double.MaxValue, _MaxY = double.MinValue;

        #region initialize
        public Geometry()
        {
            _parts = new GeoParts();
        }

        public Geometry(GeoPoints[] points)
        {
            _parts = new GeoParts(points);
            CalExtent();
        }
        #endregion

        #region property
        public GeoParts Parts
        {
            get { return _parts; }
            set { _parts = value; }
        }

        /// <summary>
        /// 获取最小X坐标
        /// </summary>
        public double MinX
        {
            get { return _MinX; }
        }

        /// <summary>
        /// 获取最大X坐标
        /// </summary>
        public double MaxX
        {
            get { return _MaxX; }
        }

        /// <summary>
        /// 获取最小Y坐标
        /// </summary>
        public double MinY
        {
            get { return _MinY; }
        }

        /// <summary>
        /// 获取最大Y坐标
        /// </summary>
        public double MaxY
        {
            get { return _MaxY; }
        }

        #endregion

        #region methods
        public GeoRectangle GetEnvelope()
        {
            GeoRectangle sRectangle = new GeoRectangle(_MinX, _MaxX, _MinY, _MaxY);
            return sRectangle;
        }


        public GeoPolyline Clone()
        {
            GeoPolyline sMultiPolyline = new GeoPolyline();
            sMultiPolyline._parts = _parts.Clone();
            sMultiPolyline.UpdateExtent();
            return sMultiPolyline;
        }
        #endregion

        #region private methods
        protected void UpdateExtent()
        {
            _parts.UpdateExtent();
            _MinX = _parts.MinX;
            _MaxX = _parts.MaxX;
            _MinY = _parts.MinY;
            _MaxY = _parts.MinX;
        }
        #endregion

    }
}
