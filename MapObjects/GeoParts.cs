using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoParts
    {
        #region field
        private List<GeoMultiPoint> _Parts;
        private double _MinX = double.MaxValue;
        private double _MaxX = double.MinValue;
        private double _MinY = double.MaxValue;
        private double _MaxY = double.MinValue;
        #endregion

        #region initial
        public GeoParts()
        {
            _Parts = new List<GeoMultiPoint>();
        }

        public GeoParts(GeoMultiPoint[] parts)
        {
            _Parts = new List<GeoMultiPoint>();
            _Parts.AddRange(parts);
        }
        #endregion

        #region property

        public Int32 Count
        {
            get { return _Parts.Count; }
        }
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


        #endregion

        #region Methods

        /// <summary>
        /// Get item with index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GeoMultiPoint GetItem(Int32 index)
        {
            return _Parts[index];
        }

        /// <summary>
        /// set item with index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="part"></param>
        public void SetItem(Int32 index, GeoMultiPoint part)
        {
            _Parts[index] = part;
            CalExtent();
        }

        /// <summary>
        /// add item to the end
        /// </summary>
        /// <param name="part"></param>
        public void Add(GeoMultiPoint part)
        {
            _Parts.Add(part);
            CalExtent();
        }

        /// <summary>
        /// add a list of item to the end
        /// </summary>
        /// <param name="parts"></param>
        public void AddRange(GeoMultiPoint[] parts)
        {
            _Parts.AddRange(parts);
            CalExtent();
        }

        public void UpdateExtent()
        {
            CalExtent();
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public GeoParts Clone()
        {
            GeoParts sParts = new GeoParts();
            Int32 sPartCount = _Parts.Count;
            for (Int32 i = 0; i <= sPartCount - 1; i++)
            {
                GeoMultiPoint sPart = _Parts[i].Clone();
                sParts.Add(sPart);
            }
            return sParts;
        }
        #endregion

        #region private methods
        private void CalExtent()
        {
            double sMinX = double.MaxValue;
            double sMaxX = double.MinValue;
            double sMinY = double.MaxValue;
            double sMaxY = double.MinValue;
            for (Int32 i=0; i<_Parts.Count;i++)
            {
                GeoMultiPoint part = _Parts[i];
                part.UpdateExtent();
                if (_MaxX < part.MaxX)
                {
                    sMaxX = part.MaxX;
                }
                if (_MaxY < part.MaxY)
                {
                    sMaxY = part.MaxY;
                }
                if (_MinX >part.MinX)
                {
                    sMinX = part.MinX;
                }
                if (_MinY > part.MinY)
                {
                    sMinY = part.MinY;
                }
            }
            _MinX = sMinX;
            _MaxX = sMaxX;
            _MinY = sMinY;
            _MaxY = sMaxY;
        }

        #endregion 

    }
}
