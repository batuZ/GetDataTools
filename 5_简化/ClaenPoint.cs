using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetDataTools;
using System.IO;
using System.Windows.Forms;
using OSGeo.OGR;

namespace GetDataTools._5_简化
{
    class CleanPoint
    {

        /*************************************  点角式  *****************************************/
        /// <summary>
        /// 清理等后生成新的文件
        /// 
        /// </summary>
        public static void claenPoint(string filePath, string savefile, double jiaodu, int cishu)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            //进来的Layer
            OSGeo.OGR.DataSource oriDs = dr.Open(filePath, 0);
            OSGeo.OGR.Layer oriLayer = oriDs.GetLayerByIndex(0);
            //出去的Layer
            if (System.IO.File.Exists(savefile))
            {
                savefile.deleteFiles();
            }
            OSGeo.OGR.DataSource targDs = dr.CreateDataSource(savefile, null);
            OSGeo.OGR.Layer targLayer = targDs.CreateLayer("targ", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);

            long featCount = oriLayer.GetFeatureCount(0);
            for (int i = 0; i < featCount; i++)
            {
                ///进来的Featuer
                OSGeo.OGR.Feature oriFeat = oriLayer.GetFeature(i);

                ///把一个Featuer转为点数组
                OSGeo.OGR.Geometry oriGeom = oriFeat.GetGeometryRef();
                OSGeo.OGR.Geometry subGeom = oriGeom.GetGeometryRef(0);
                int pointCount = subGeom.GetPointCount();
                Point[] aFeat = new Point[pointCount];
                for (int c = 0; c < pointCount; c++)
                {
                    aFeat[c].X = subGeom.GetX(c);
                    aFeat[c].Y = subGeom.GetY(c);
                    aFeat[c].Z = subGeom.GetZ(c);
                }

                ///调选点方法,得到一个新的Featuer
                Geometry newGeom = null;
                if (aFeat.Length > cishu * 3)
                {
                    newGeom = JID(aFeat, jiaodu, cishu);
                }
                else
                {
                    oriFeat.Dispose();
                    continue;
                }
                if (newGeom != null)
                {
                    Feature temp = new Feature(new FeatureDefn(""));
                    temp.SetGeometry(newGeom);
                    targLayer.CreateFeature(temp);
                    temp.Dispose();
                }
                oriFeat.Dispose();
                Console.WriteLine("已完成{0}/{1}", i, featCount);
            }
            oriDs.Dispose();
            targDs.Dispose();
        }

        /// <summary>
        /// 清理原文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void claenPoint(string filePath, double jiaodu, int cishu)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource oriDs = dr.Open(filePath, 1);
            OSGeo.OGR.Layer oriLayer = oriDs.GetLayerByIndex(0);

            var featCount = oriLayer.GetFeatureCount(0);

            for (int i = 0; i < featCount; i++)
            {
                OSGeo.OGR.Feature oriFeat = oriLayer.GetFeature(i);
                OSGeo.OGR.Geometry oriGeom = oriFeat.GetGeometryRef();
                OSGeo.OGR.Geometry subGeom = oriGeom.GetGeometryRef(0);

                int pointCount = subGeom.GetPointCount();

                Point[] aFeat = new Point[pointCount];

                for (int c = 0; c < pointCount; c++)
                {
                    aFeat[c].X = subGeom.GetX(c);
                    aFeat[c].Y = subGeom.GetY(c);
                    aFeat[c].Z = subGeom.GetZ(c);
                }

                OSGeo.OGR.Geometry newGeom = null;
                if (aFeat.Length > cishu * 3)
                {
                    newGeom = JID(aFeat, jiaodu, cishu);
                }
                else
                {
                    oriFeat.Dispose();
                    continue;
                }
                if (newGeom != null)
                {
                    oriFeat.SetGeometry(newGeom);
                    oriLayer.SetFeature(oriFeat);
                }
                Console.WriteLine("cleanPoint已完成{0}/{1}", i, featCount);
                oriFeat.Dispose();
            }
            oriDs.Dispose();
        }

        /// <summary>
        /// 三点夹角的判定条件,输出为满足条件的成员的ID所组成的ID数组
        /// </summary>
        /// <param name="aFeat"></param>
        /// <returns></returns>
        private static OSGeo.OGR.Geometry JID(Point[] aFeat, double userSet, int seleTime)
        {
            List<Point[]> pjGroupL = new List<Point[]>();
            List<Point[]> zjGroupL = new List<Point[]>();

            List<Point> pjGroup = new List<Point>();
            List<Point> zjGroup = new List<Point>();

            for (int i = 0; i < aFeat.Length; i++)
            {
                int frontId, thisId, backId;
                bool[] yon = new bool[seleTime];
                for (int t = 1; t <= seleTime; t++)
                {
                    frontId = i < t ? aFeat.Length - 1 + i - t : i - t;

                    thisId = i;

                    backId = i > aFeat.Length - 1 - t ? i - (aFeat.Length - 1) + t : backId = i + t;

                    double jiaodu = cosCalculator(aFeat[frontId], aFeat[thisId], aFeat[backId]);

                    yon[t - 1] = jiaodu > userSet;
                }

                if (yon.Contains(true))
                {
                    pjGroup.Add(aFeat[i]);
                }
                else
                {
                    zjGroup.Add(aFeat[i]);
                }
            }

            OSGeo.OGR.Geometry outGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
            OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);

            for (int g = 0; g < zjGroup.Count(); g++)
            {
                Point a = zjGroup[g];
                subGeom.AddPoint(a.X, a.Y, a.Z);
            }
            if (subGeom.GetPointCount() < 4)
            {
                return null;
            }
            subGeom.CloseRings();
            outGeom.AddGeometry(subGeom);
            return outGeom;
        }
        private static double cosCalculator(Point p1, Point p, Point p2)   /// 求夹角
        {
            double fenzi = (p1.X - p.X) * (p2.X - p.X) + (p1.Y - p.Y) * (p2.Y - p.Y);
            double fenmu = Math.Sqrt((p1.X - p.X) * (p1.X - p.X) + (p1.Y - p.Y) * (p1.Y - p.Y)) * Math.Sqrt((p2.X - p.X) * (p2.X - p.X) + (p2.Y - p.Y) * (p2.Y - p.Y));
            double cosValue = fenzi / fenmu;
            double acosV = Math.Acos(cosValue) * 180 / Math.PI;
            return acosV;
        }

        /*************************************  清理点簇  （未完成）*****************************************/


        public static void getCleanLins(string inFile, string outFile)
        {
            Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = Ogr.GetDriverByName("ESRI shapefile");
            DataSource inds = dr.Open(inFile, 0);
            Layer inLayer = inds.GetLayerByIndex(0);

            if (File.Exists(outFile))
                File.Delete(outFile);
            DataSource outds = dr.CreateDataSource(outFile, null);
            Layer outLayer = outds.CreateLayer("", null, wkbGeometryType.wkbPolygon, null);

            long featCount = inLayer.GetFeatureCount(0);

            for (int i = 0; i < featCount; i++)
            {
                Feature inFeat = inLayer.GetFeature(i);
                Geometry inGeom = inFeat.GetGeometryRef();
                Geometry inSubG = inGeom.GetGeometryRef(0);

                Feature outFeat = new Feature(new FeatureDefn(""));
                Geometry outGeom = new Geometry(wkbGeometryType.wkbPolygon);

                outGeom.AddGeometry(getCleanGeom(inSubG));
                outFeat.SetGeometry(outGeom);
                outLayer.CreateFeature(outFeat);
                Console.WriteLine("{0}/{1}", i, featCount);
            }
            inds.Dispose();
            outds.Dispose();
        }

        private static Geometry getCleanGeom(Geometry s)
        {
            //记录当前点的上一次状态，True 为小于fazhi ,false为大于fazhi
            bool after距离 = true;
            //记录当前点的上一次倾角
            double afterK = 0;
            double afterJ = Math.Atan(afterK) * 180 / Math.PI;
            //最小距离，小于此值的点将被加入点簇进行优化----------------------------------------------------------------------------------------
            double fazhi = 2;
            //点数量
            int pointCount = s.GetPointCount();
            //Feat的全部小距离点组
            List<int[]> pointGroupG = new List<int[]>();
            // List<List<int>> pointGroupG = new List<List<int>>();
            //一个小距区点组
            List<int> pointGroup = new List<int>();
            //筛选结果
            List<int> saveP = new List<int>();

            for (int i = 0; i < pointCount - 1; i++)
            {
                double x = s.GetX(i);
                double y = s.GetY(i);
                double xx = s.GetX(i + 1);
                double yy = s.GetY(i + 1);

                //当前点和下一个点形成的线段的斜率
                double k = (y - yy) / (x - xx);
                double j = Math.Atan(k) * 180 / Math.PI;
                //当前点到下一个点的距离
                double thisJL = Math.Sqrt(((x - xx) * (x - xx) + (y - yy) * (y - yy)));

                if (thisJL < fazhi)//当前点与下个点的距离小于阈值时，把点加到b组中，状态改为临近点
                {
                    pointGroup.Add(i);
                    after距离 = true;
                }
                else if (after距离)//距离大于阈值，状态还是临近点时
                {
                    pointGroup.Add(i);//先把点加进b组
                    if (pointGroup.Count > 3)//且当前b组中已有多于三个点
                    {
                        pointGroupG.Add(pointGroup.ToArray());//把b组添加到a组中
                    }
                    else//且当前b组少于或等于三个点
                    {
                        saveP.AddRange(pointGroup);//把当前b组加入结果集中
                    }
                    pointGroup.Clear();//清空b组
                    after距离 = false;//状态改为非临近点
                }
                else if (Math.Abs(afterJ - j) > 5)//如果距离大于阈值，且状态非临近，---------------------------------------------------------倾角差大于3度时
                {
                    saveP.Add(i);//把当前点加入结果集
                }
                afterJ = j;//把当前倾角记录到前一倾角
            }
            //此时a组中有若干b
            //把a和原Geometry发送给ids方法，提取a中满足条件的点，并将其加入结果集

            saveP.AddRange(ids(pointGroupG, s));
            //结果集排序
            saveP.Sort();

            //用saveP里的点重建 geomety 
            Geometry newGeom = new Geometry(wkbGeometryType.wkbLinearRing);
            for (int i = 0; i < saveP.Count; i++)
            {
                newGeom.AddPoint_2D(s.GetX(saveP[i]), s.GetY(saveP[i]));
            }
            newGeom.AddPoint_2D(s.GetX(saveP[0]), s.GetY(saveP[0]));
            s.Dispose();
            return newGeom;
        }
        private static List<int> ids(List<int[]> pointGroupG, Geometry aFeat)
        {// 拆箱
            List<int> saveP = new List<int>();
            for (int i = 0; i < pointGroupG.Count; i++)
            {
                saveP.AddRange(_提取断点(aFeat, pointGroupG[i]));
            }
            return saveP;
        }
        private static List<int> _提取断点(Geometry subGeom, int[] v)
        {
            Ogr.RegisterAll();

            List<int> ids = new List<int>();

            //拿到图形中最远的两个点
            //加到表中，排续后准备开始循环加入
            ids.Add(v[0]);
            ids.Add(v[v.Length - 1]);

            //重要的s
            //1、当作判断是否有点加入的变量，如果当前表中的成员数量多于上一次的数量，说明有新成员加入，需要继续循环
            //2、当作当前循环的次数上限
            int s = 0;

            //刚刚被初始化的点集中只有起点和终点
            //每次循环都会加入更多的大于userSet的点，使点集成员增加
            //s会记录前一次点集成员数量
            //当前循环会判断当前点集成员数量是否有增加，只要有新增，就开始循环
            while (ids.Count > s)
            {
                //记录本次循环以前点集成员数
                s = ids.Count;
                ///除最后一个点，使用每一个成员作为起点，下一点为终点的线段，
                ///找到距离最远且大于userSet的点ID，
                ///将其添加到点集中，使点集成员发生变化
                for (int i = 0; i < s - 1; i++)
                {
                    int ok = _离线最远点ID(ids[i], ids[i + 1], subGeom);
                    if (ok != -1)
                    {
                        ids.Add(ok);
                    }
                }
                ids.Sort();//排续
            }
            return ids;
        }
        private static int _离线最远点ID(int starID, int endID, Geometry subGeom)
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
            }
            return maxJL >= 0.2 ? maxID : -1;//------------------------------------------------------------------------>设置点距式中的最小距离阈值
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
