using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoPolyline:Geometry
    {
        private GeoParts _Curves;
        double _MinX = double.MaxValue, _MaxX = double.MinValue;
        double _MinY = double.MaxValue, _MaxY = double.MinValue;

        #region initialize
        public GeoPolyline()
        {
            _Curves = new GeoParts();
        }

        public GeoPolyline(GeoMultiPoint[] points)
        {
            _Curves = new GeoParts(points);
            CalExtent();
        }
        #endregion

        #region property
        public GeoParts Parts
        {
            get { return _Curves; }
            set { _Curves = value; }
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

        public void UpdateExtent()
        {
            CalExtent();
        }

        public GeoPolyline Clone()
        {
            GeoPolyline sMultiPolyline = new GeoPolyline();
            sMultiPolyline._Curves = _Curves.Clone();
            sMultiPolyline.UpdateExtent();
            return sMultiPolyline;
        }
        #endregion


        #region private methods
        private void CalExtent()
        {
            _Curves.UpdateExtent();
            _MinX = _Curves.MinX;
            _MaxX = _Curves.MaxX;
            _MinY = _Curves.MinY;
            _MaxY = _Curves.MinX;
        }
        #endregion

    }
}
