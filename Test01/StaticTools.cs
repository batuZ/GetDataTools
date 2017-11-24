using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test01
{
    static class StaticTools
    {
        public static void deleteFeatUpdate(this OSGeo.OGR.DataSource myDS)
        {
            string a = "REPACK " + myDS.GetLayerByIndex(0).GetName();
            myDS.ExecuteSQL(a, null, "");
            myDS.Dispose();
        }
        public static bool IsDispose(this OSGeo.GDAL.Dataset myDS)
        {
            try
            {
                int e = myDS.RasterCount;
                return false;
            }
            catch { return true; }

        }
        public static string tempFilePath(string format, string whatAmI = "")
        {
            if (!Directory.Exists(@"D:\TEMPFORGETDATATOOLS\"))
            {
                Directory.CreateDirectory(@"D:\TEMPFORGETDATATOOLS\");
            }
            int i = 0;
            while (true)
            {
                string filePath = @"D:\TEMPFORGETDATATOOLS\Temp"
                    + format.ToUpper()
                    + "File"
                    + i.ToString()
                    + "_"
                    + whatAmI
                    + "."
                    + format;
                if (File.Exists(filePath))
                { i++; }
                else
                { return filePath; }
            }
        }
        /// <summary>
        /// 判断两个Featuer是否重复，ori 当前Feat，next 目标Feat
        /// </summary>
        /// <param name="ori"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static bool isSame(OSGeo.OGR.Feature ori, OSGeo.OGR.Feature next, double fanWei = 0.1)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Geometry oriGeom = ori.GetGeometryRef();
            OSGeo.OGR.Envelope oriEnve = new OSGeo.OGR.Envelope();
            oriGeom.GetEnvelope(oriEnve);
            OSGeo.OGR.Geometry nextGeom = next.GetGeometryRef();
            OSGeo.OGR.Envelope nextEnve = new OSGeo.OGR.Envelope();
            nextGeom.GetEnvelope(nextEnve);
            double oriArea = oriGeom.GetArea();
            double nextArea = nextGeom.GetArea();
            bool res =
                Math.Abs(oriEnve.MaxX - nextEnve.MaxX) < fanWei && //外接矩形差
               Math.Abs(oriEnve.MaxY - nextEnve.MaxY) < fanWei &&
               Math.Abs(oriEnve.MinX - nextEnve.MinX) < fanWei &&
               Math.Abs(oriEnve.MinY - nextEnve.MinY) < fanWei;
            //面积？    && Math.Abs(oriArea - nextArea) < 0.1;
            oriGeom.Dispose();
            oriEnve.Dispose();
            nextGeom.Dispose();
            nextEnve.Dispose();
            return res;
        }
        /// <summary>
        /// 标准差
        /// </summary>
        /// <param name="dzxLayer"></param>
        /// <param name="aue"></param>
        /// <param name="bzc"></param>
        public static void getBZC(OSGeo.OGR.Layer dzxLayer, out double aue, out double bzc)
        {
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
