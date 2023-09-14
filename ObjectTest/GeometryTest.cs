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

    }
}