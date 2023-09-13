using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{

    /// <summary>
    /// a 2D point with coordination
    /// </summary>
    public class GeoPoint:Geometry
    {
        private double _X, _Y;

        #region Initialize
        public GeoPoint()
        {}


        /// <summary>
        /// create a GeoPoint with coordination
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public GeoPoint(double x, double y)
        {
            _X = x;
            _Y = y;
        }

        #endregion

        #region property
        /// <summary>
        /// Set or get X
        /// </summary>
        public double X
        {
            get { return _X; }
            set { _X = value; }
        }

        /// <summary>
        /// Set or get Y
        /// </summary>
        public double Y
        {
            get { return _Y; }
            set { _Y = value; }
        }
        #endregion

        #region methods

        /// <summary>
        /// make a copy of this GeoPoint
        /// </summary>
        /// <returns>GeoPoint</returns>
        public GeoPoint Clone()
        {
            GeoPoint sPoint = new GeoPoint(_X, _Y);
            return sPoint;
        }
        #endregion region


    }
}
