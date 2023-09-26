using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapObjects
{
    public class GeoPolygon:Geometry
    {
        public GeoPolygon()
        {
        }
        public GeoPolygon(GeoPoints[] points)
        {
            _parts = new GeoParts(points);
            UpdateExtent();
        }
    }
}
