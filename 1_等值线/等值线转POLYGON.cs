using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._1_等值线
{
    class 等值线转POLYGON
    {
        /********************************   等值线转换为多边形   ***********************************************/

        /// <summary>
        /// 等值线转为POLYGON
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string dzPoly(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");

            string a = StaticTools.tempFilePath("shp","等值线POLY");
            OSGeo.OGR.DataSource newDS = dr.CreateDataSource(a, null);
            OSGeo.OGR.Layer polyLayer = newDS.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            polyLayer.CreateField(fieldDf0, 1);//ID
            polyLayer.CreateField(fieldDf1, 1);//Value
            OSGeo.OGR.FeatureDefn featDF = new OSGeo.OGR.FeatureDefn("");
            Console.WriteLine("开始等值线转POLY！");
            OSGeo.OGR.DataSource cleanDS = dr.Open(filePath, 0);
            OSGeo.OGR.Layer cleanLayer = cleanDS.GetLayerByIndex(0);
            for (int i = 0; i < cleanLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature lineFeat = cleanLayer.GetFeature(i);
                OSGeo.OGR.Geometry lineGeom = lineFeat.GetGeometryRef();
                
                OSGeo.OGR.Feature polyFeat = new OSGeo.OGR.Feature(featDF);
                OSGeo.OGR.Geometry polyGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                int u = lineGeom.GetPointCount();
                for (int s = 0; s < u; s++)
                {
                    double x = lineGeom.GetX(s);
                    double y = lineGeom.GetY(s);
                    double z = lineGeom.GetZ(s);
                    subGeom.AddPoint(x, y, z);
                }
                polyGeom.AddGeometry(subGeom);
                polyFeat.SetGeometry(polyGeom);
                polyLayer.CreateFeature(polyFeat);
                lineGeom.Dispose();
                polyGeom.Dispose();
                subGeom.Dispose();
                lineFeat.Dispose();
                polyFeat.Dispose();
            }
            cleanLayer.Dispose();
            polyLayer.Dispose();
            cleanDS.Dispose();
            newDS.Dispose();
            Console.WriteLine("等值线转POLY完成！");
            return a;
        }
    }
}
