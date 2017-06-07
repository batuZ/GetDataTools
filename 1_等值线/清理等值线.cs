using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._1_等值线
{
    class 清理等值线
    {
        /**************************************   清理等值线   *****************************************/
        /// <summary>
        /// 通过线长,值 清理等值线
        /// </summary>
        /// <param name="filePath"></param>
        public static string cleanDS(string dzx)
        {
            Console.WriteLine("开始清理等值线！");
            double aue, bzc; _标准差(dzx, out aue, out bzc);
            double minLength = 50;
            double maxLength = 2600;
            double minValue = aue - bzc * 2;
            double maxValue = aue + bzc * 2;
            //open dzx
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
            OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);
            //new a shp
            string cleanline = StaticTools.tempFilePath("shp", "清理后的等值线");
            OSGeo.OGR.DataSource newdzxDS = dr.CreateDataSource(cleanline, null);
            OSGeo.OGR.Layer newdzxLayer = newdzxDS.CreateLayer(dzxLayer.GetName(), dzxLayer.GetSpatialRef(), dzxLayer.GetGeomType(), null);

            for (int i = 0; i < dzxLayer.GetFeatureCount(0); i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();

                double FeatLength = fileGeom.Length();
                bool s1 = FeatLength > minLength && FeatLength < maxLength;

                double featValue = fileFeat.GetFieldAsDouble("EVE");
                bool s2 = featValue > minValue && featValue < maxValue;

                bool isR = fileGeom.IsRing();
                if (s1 && s2 && isR)
                {
                    newdzxLayer.CreateFeature(fileFeat);
                }
                fileFeat.Dispose();
            }
            newdzxDS.Dispose();
            dzxDS.Dispose();
            Console.WriteLine("清理等值线完成！");
            return cleanline;
        }

        private static void _标准差(string dzx, out double aue, out double bzc)
        {

            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource dzxDS = dr.Open(dzx, 0);
            OSGeo.OGR.Layer dzxLayer = dzxDS.GetLayerByIndex(0);

            //获取Featuer数
            int featCount = dzxLayer.GetFeatureCount(0);

            // 1 拿到每个Featuer的Value
            double[] values = new double[featCount];
            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature fileFeat = dzxLayer.GetFeature(i);
                OSGeo.OGR.Geometry fileGeom = fileFeat.GetGeometryRef();
                values[i] = fileFeat.GetFieldAsDouble("EVE");
                fileGeom.Dispose();
                fileFeat.Dispose();
            }
            dzxDS.Dispose();
            // 2 求Values的平均值
            aue = values.Average();

            // 3 求values与平均值差的平方和
            double pingFangHe = 0;
            for (int i = 0; i < featCount; i++)
            {
                pingFangHe += (values[i] - aue) * (values[i] - aue);
            }
            // 4 每个值与平均值的差相加,除Featuer数.再开方,得到标准差
            bzc = Math.Sqrt(pingFangHe / featCount);
        }
    }
}
