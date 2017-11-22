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
    }
}
