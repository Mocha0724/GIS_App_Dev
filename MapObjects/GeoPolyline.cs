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
        #region initialize

        public GeoPolyline()
        {
        }

        public GeoPolyline(GeoPoints[] points)
        {
            _parts = new GeoParts(points);
            UpdateExtent();
        }
        #endregion


    }
}
