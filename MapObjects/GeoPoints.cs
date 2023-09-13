﻿using System;
using System.Collections.Generic;
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
        public GeoPoint GetItem(Int32 id)
        {
            return _Points[id];
        }

        public GeoPoint AddPoint(Int32 )



        #endregion

    }
}