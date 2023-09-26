using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoMultiPoint:Geometry
    {
        public GeoMultiPoint()
        {
        }
        public GeoMultiPoint(GeoPoints[] points)
        {
            _parts = new GeoParts(points);
            UpdateExtent();
        }
    }
}
