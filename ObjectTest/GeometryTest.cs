namespace ObjectTest
{
    [TestClass]
    public class GeometryTest
    {
        [TestMethod]
        public void GeoPoint_Test()
        {
            MapObjects.GeoPoint geoPoint = new MapObjects.GeoPoint(1.0, 1.0);
            MapObjects.GeoPoint geoPoint_c = geoPoint.Clone();
            Assert.AreEqual(geoPoint.X, geoPoint_c.X);
            geoPoint.X = 2.0;
            Assert.AreEqual(geoPoint, geoPoint_c);
        }

        public void Geometry_Test()
        {
            MapObjects.GeoPoint p1 = new MapObjects.GeoPoint(1.0, 2.0);
            MapObjects.GeoPoint p2 = p1.Clone();
            p2.X = 2.0;
            MapObjects.GeoPoint p3 = p1.Clone();
            p3.X = 3.0; p3.Y = 3.0;

            MapObjects.GeoPoints ps1 = new MapObjects.GeoPoints();
            ps1.Add(p1);ps1.Add(p2);ps1.Add(p3);

            MapObjects.GeoPoints ps2 = ps1.Clone();
            for(int i = 0; i< ps2.Count; i++)
            {
                MapObjects.GeoPoint p = ps2.GetItem(i);
                p.X += 5;
                p.Y += 5;
            }

            List<MapObjects.GeoPoints> test_part = new List<MapObjects.GeoPoints>();
            test_part.Add(ps1);test_part.Add(ps2);

            MapObjects.GeoPolygon test_polygon = new MapObjects.GeoPolygon(test_part);

        }

    }
}