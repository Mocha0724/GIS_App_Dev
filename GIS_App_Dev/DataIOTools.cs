using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using MapObjects;

namespace GIS_APP_DEV
{
    public static class DataIOTools
    {
        /*
        public static Layer ReadShp(string filepath)
        {
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8","YES");
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            OSGeo.GDAL.Gdal.AllRegister();
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.OGR.Driver pDriver = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile");
            OSGeo.OGR.DataSource pDataSource = pDriver.Open(filepath, 1);
            OSGeo.OGR.Layer pLayer = pDataSource.GetLayerByName(System.IO.Path.GetFileNameWithoutExtension(filepath));
            
        }
        */
    }
}
