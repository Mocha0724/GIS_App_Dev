using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoPoints
    {
        #region feild
        private List<GeoPoint> _Points;  //List of GeoPoints
        private double _MinX = double.MaxValue;
        private double _MaxX = double.MinValue;
        private double _MinY = double.MaxValue;
        private double _MaxY = double.MinValue;
        #endregion

        #region initialize
        public GeoPoints()
        {
            _Points = new List<GeoPoint>();
        }

        public GeoPoints(GeoPoint[] points)
        {
            _Points = new List<GeoPoint>();
            _Points.AddRange(points);
        }
        #endregion

        #region property
        public Int32 Count
        {
            get { return _Points.Count; }
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

        #region methods 

        /// <summary>
        /// Get an item with index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GeoPoint GetItem(Int32 index)
        {
            return _Points[index];
        }

        /// <summary>
        /// Add a point into the collection
        /// </summary>
        /// <param name="newPoint"></param>
        public void Add(GeoPoint newPoint)
        {
            _Points.Add(newPoint);
            CalExtent();
        }

        public void AddRange(GeoPoint[] newPoints)
        {
            _Points.AddRange(newPoints);
            CalExtent();
        }
        public void InsertRange(Int32 index, GeoPoint[] newPoints)
        {
            _Points.InsertRange(index, newPoints);
            CalExtent();
        }

        public void Insert(Int32 index, GeoPoint newPoint)
        {
            _Points.Insert(index, newPoint);
            CalExtent();
        }

        public void RemoveAt(Int32 index)
        {
            _Points.RemoveAt(index);
            CalExtent();
        }

        /// <summary>
        /// Copy all the items to a new array
        /// </summary>
        /// <returns></returns>
        public GeoPoint[] ToArray()
        {
            return _Points.ToArray();
        }

        /// <summary>
        /// Clear all the items
        /// </summary>
        public void Clear()
        {
            _Points.Clear();
            CalExtent();
        }

        /// <summary>
        /// Get Rectangle
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// 更新范围
        /// </summary>
        public void UpdateExtent()
        {
            CalExtent();
        }

        /// <summary>
        /// make a new GeoPoints
        /// </summary>
        /// <returns></returns>
        public GeoPoints Clone()
        {
            GeoPoints sPoints = new GeoPoints();
            Int32 sPointCount = _Points.Count;
            for (Int32 i = 0; i <= sPointCount - 1; i++)
            {
                GeoPoint sPoint = new GeoPoint(_Points[i].X, _Points[i].Y);
                sPoints.Add(sPoint);
            }
            sPoints._MinX = _MinX;
            sPoints._MaxX = _MaxX;
            sPoints._MinY = _MinY;
            sPoints._MaxY = _MaxY;
            return sPoints;
        }

        #endregion

        #region private methods
        private void CalExtent()
        {
            double sMinX = double.MaxValue;
            double sMaxX = double.MinValue;
            double sMinY = double.MaxValue;
            double sMaxY = double.MinValue;
            Int32 sPointCount = _Points.Count;
            for (Int32 i = 0; i < sPointCount; i++)
            {
                if (_Points[i].X < sMinX)
                    sMinX = _Points[i].X;
                if (_Points[i].X > sMaxX)
                    sMaxX = _Points[i].X;
                if (_Points[i].Y < sMinY)
                    sMinY = _Points[i].Y;
                if (_Points[i].Y > sMaxY)
                    sMaxY = _Points[i].Y;
            }
            _MinX = sMinX;
            _MaxX = sMaxX;
            _MinY = sMinY;
            _MaxY = sMaxY;
        }
        #endregion

    }
}
