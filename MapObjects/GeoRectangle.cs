using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoRectangle:Shape
    {
        #region field

        private double _MinX, _MaxX, _MinY, _MaxY;
        #endregion

        #region initialize
        public GeoRectangle(double minX, double maxX, double minY, double maxY)
        {
            _MinX = minX;
            _MaxX = maxX;
            _MinY = minY;
            _MaxY = maxY;
        }
        #endregion

        #region feilds

        /// <summary>
        /// 获取最小X坐标
        /// </summary>
        public double MinX
        {
            get { return _MinX; }
        }

        public double MaxX
        {
            get { return _MaxX; }
        }

        public double MinY
        {
            get { return _MinY; }
        }

        public double MaxY
        {
            get { return _MaxY; }
        }

        public double Width
        {
            get { return _MaxX - _MinX; }
        }

        public double Height
        {
            get { return _MaxY - _MinY; }
        }

        /// <summary>
        /// 指示是否为空矩形
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (_MaxX <= _MinX)
                    return true;
                else if (_MaxY <= _MinY)
                    return true;
                else
                    return false;
            }
        }
        #endregion
    }
}
