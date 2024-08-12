using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools._5_简化
{
    class _点距法提断点
    {
        /******************************************   点距法提断点  ***********************************************************/
        /// <summary>
        /// 把原始轮廓的拐点提出来，生成shp文件
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public static void _提取断点到文件(string inFile, string outFile)
        {
            Ogr.RegisterAll();
            Driver shpdr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            DataSource shpDs = shpdr.Open(inFile, 0);
            if (File.Exists(outFile))
            { File.Delete(outFile); }
            DataSource newDs = shpdr.CreateDataSource(outFile, null);
            Layer shpLayer = shpDs.GetLayerByIndex(0);
            Layer newLayer = newDs.CreateLayer("", null, wkbGeometryType.wkbPolygon, null);

            long featCount = shpLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                Feature inFeat = shpLayer.GetFeature(i);
                Geometry geom = inFeat.GetGeometryRef();
                Geometry subGeom = geom.GetGeometryRef(0);
                List<int> ids = _提取断点(inFeat, 2);

                Feature newFeat = new Feature(new FeatureDefn(""));
                Geometry newGeom = new Geometry(wkbGeometryType.wkbPolygon);
                Geometry newSubGeom = new Geometry(wkbGeometryType.wkbLinearRing);
                for (int j = 0; j < ids.Count; j++)
                {
                    newSubGeom.AddPoint(subGeom.GetX(ids[j]), subGeom.GetY(ids[j]), subGeom.GetZ(ids[j]));
                }
                newGeom.AddGeometry(newSubGeom);
                newFeat.SetGeometry(newGeom);
                newLayer.CreateFeature(newFeat);
                subGeom.Dispose();
                geom.Dispose();
                inFeat.Dispose();
                newSubGeom.Dispose();
                newGeom.Dispose();
                newFeat.Dispose();
            }
            shpDs.Dispose();
            newDs.Dispose();
        }

        /// <summary>
        /// 提取一个FEATURE的拐点
        /// </summary>
        /// <param name="inFeat"></param>
        /// <param name="userSet"></param>
        /// <returns></returns>
        public static List<int> _提取断点(Feature inFeat, double userSet)
        {
            Ogr.RegisterAll();
            Geometry geom = inFeat.GetGeometryRef();
            Geometry subGeom = geom.GetGeometryRef(0);
            List<int> ids = new List<int>();
            //拿到图形中最远的两个点
            int max1 = _离点最远点ID(subGeom, 0);
            int max2 = _离点最远点ID(subGeom, max1);
            //加到表中，排续后准备开始循环加入
            ids.Add(max1);
            ids.Add(max2);
            ids.Sort();
            ///重要的s
            ///1、当作判断是否有点加入的变量，如果当前表中的成员数量多于上一次的数量，说明有新成员加入，需要继续循环
            ///2、当作当前循环的次数上限
            int s = 0;
            while (ids.Count > s)
            {
                s = ids.Count;
                for (int i = 0; i < s; i++)
                {
                    int ok;
                    //如果i是最后一个点的索引，则start为当前，end为第一个点
                    if (i == (s - 1))
                    {
                        if ((ok = _离线最远点ID(ids[i], ids[0], subGeom, userSet)) != -1)
                        {
                            ids.Add(ok);
                        }
                    }
                    else
                    {
                        if ((ok = _离线最远点ID(ids[i], ids[i + 1], subGeom, userSet)) != -1)
                        {
                            ids.Add(ok);
                        }
                    }
                }
                ids.Sort();//排续
            }
            Console.WriteLine("前前FEAT提取到{0}个点", ids.Count);
            return ids;
        }
        /// <summary>
        /// 返回一个ID ，代表离输入的线最远的那个点，
        /// userSet为一个限制值，如果全部距离都小于它则返回-1
        /// </summary>
        /// <param name="starID"></param>
        /// <param name="endID"></param>
        /// <param name="subGeom"></param>
        /// <param name="userSet"></param>
        /// <returns></returns>
        private static int _离线最远点ID(int starID, int endID, Geometry subGeom, double userSet)
        {
            double X1 = subGeom.GetX(starID);
            double Y1 = subGeom.GetY(starID);
            double X2 = subGeom.GetX(endID);
            double Y2 = subGeom.GetY(endID);

            double maxJL = -1;
            int maxID = -1, i = starID;
            while (i != endID)
            {
                double X = subGeom.GetX(i);
                double Y = subGeom.GetY(i);
                double thisJL = _点到直线的距离(X1, Y1, X2, Y2, X, Y);
                if (thisJL > maxJL)
                {
                    maxID = i;
                    maxJL = thisJL;
                }
                i++;
                if (i == (subGeom.GetPointCount() - 1))
                {
                    i = 0;
                }
            }
            return maxJL >= userSet ? maxID : -1;
        }
        private static int _离点最远点ID(Geometry subGeom, int inID)
        {
            double maxJL = 0;
            int maxID = -1;
            for (int i = 0; i < subGeom.GetPointCount(); i++)
            {
                double x = subGeom.GetX(inID);
                double y = subGeom.GetY(inID);
                double tx = subGeom.GetX(i);
                double ty = subGeom.GetY(i);
                double juli = Math.Sqrt((x - tx) * (x - tx) + (y - ty) * (y - ty));
                if (juli > maxJL)
                {
                    maxJL = juli;
                    maxID = i;
                }
            }
            return maxID;
        }
        private static double _点到直线的距离(double X1, double Y1, double X2, double Y2, double X, double Y)
        {
            double A = Y2 - Y1;
            double B = X1 - X2;
            double C = X2 * Y1 - X1 * Y2;
            return Math.Abs(A * X + B * Y + C) / Math.Sqrt(A * A + B * B);
        }


    }
}
