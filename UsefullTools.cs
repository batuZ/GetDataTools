using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetDataTools
{
   static class UsefullTools
    {
        #region overpast
        /// <summary>
        /// 求多边形某坐标轴的最大值
        /// </summary>
        /// <param name="point"></param>
        /// <param name="XorYorZ"></param>
        /// <returns></returns>
        public static double maxPOINT(Point[] point, string XorYorZ)
        {
            if (XorYorZ == "X")
            {
                double temp = point[0].X;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].X > temp)
                    { temp = point[i].X; }
                }
                return temp;
            }
            else if (XorYorZ == "Y")
            {
                double temp = point[0].Y;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].Y > temp)
                    { temp = point[i].Y; }
                }
                return temp;
            }
            else if (XorYorZ == "Z")
            {
                double temp = point[0].Z;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].Z > temp)
                    { temp = point[i].Z; }
                }
                return temp;
            }
            else
                return -1;
        }

        /// <summary>
        /// 求多边形某坐标轴的最小值
        /// </summary>
        /// <param name="point"></param>
        /// <param name="XorYorZ"></param>
        /// <returns></returns>
        public static double minPOINT(Point[] point, string XorYorZ)
        {
            if (XorYorZ == "X")
            {
                double temp = point[0].X;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].X < temp)
                    { temp = point[i].X; }
                }
                return temp;
            }
            else if (XorYorZ == "Y")
            {
                double temp = point[0].Y;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].Y < temp)
                    { temp = point[i].Y; }
                }
                return temp;
            }
            else if (XorYorZ == "Z")
            {
                double temp = point[0].Z;
                for (int i = 0; i < point.Length; i++)
                {
                    if (point[i].Z < temp)
                    { temp = point[i].Z; }
                }
                return temp;
            }
            else
                return -1;
        }

        /// <summary>
        /// 求多边形某坐标轴的平均值
        /// </summary>
        /// <param name="point"></param>
        /// <param name="XorYorZ"></param>
        /// <returns></returns>
        public static double auePOINT(Point[] point, string XorYorZ)
        {
            if (XorYorZ == "X")
            {
                int count = point.Length;
                double temp = 0;
                for (int i = 0; i < point.Length; i++)
                {
                    temp += point[i].X;
                }
                return temp / count;
            }
            else if (XorYorZ == "Y")
            {
                int count = point.Length;
                double temp = 0;
                for (int i = 0; i < point.Length; i++)
                {
                    temp += point[i].Y;
                }
                return temp / count;
            }
            else if (XorYorZ == "Z")
            {
                int count = point.Length;
                double temp = 0;
                for (int i = 0; i < point.Length; i++)
                {
                    temp += point[i].Z;
                }
                return temp / count;
            }
            else
                return -99999999999999;
        }

        /// <summary>
        /// 求一个多边形的外接矩形面积
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static double outRecArea(Point[] polygon)
        {
            double[] X = new double[polygon.Length];
            double[] Y = new double[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                X[i] = polygon[i].X;
                Y[i] = polygon[i].Y;
            }
            return (Math.Abs(myMax(X) -myMin(X))) + Math.Abs((myMax(Y) - myMin(Y)));
        }

        /// <summary>
        /// 求最大值
        /// </summary>
        /// <param name="suzu"></param>
        /// <returns>最大值</returns>
        public static double myMax(double[] suzu)
        {
            double temp = suzu[0];
            for (int i = 0; i < suzu.Length; i++)
            {
                if (suzu[i] > temp) { temp = suzu[i]; }
            }
            return temp;
        }

        /// <summary>
        /// 求最小值
        /// </summary>
        /// <param name="suzu"></param>
        /// <returns>最小值</returns>
        public static double myMin(double[] suzu)
        {
            double temp = suzu[0];
            for (int i = 0; i < suzu.Length; i++)
                temp = suzu[i] < temp ? suzu[i] : temp;
            return temp;
        }

        /// <summary>
        /// 求平均数
        /// </summary>
        /// <param name="suzu"></param>
        /// <returns>平均数</returns>
        public static double myAue(double[] suzu)
        {
            double sum = 0;
            int count = 0;
            for (; count < suzu.Length; count++)
                sum += suzu[count];
            return sum / count;
        }
        /// <summary>
        /// 求一个数组的标准差
        /// </summary>
        /// <param name="suzu"></param>
        /// <returns></returns>
        public static double myBiaoZhunC(double[] suzu)
        {
            double pingjunzhi = myAue(suzu);
            double temp=0;
            for (int i = 0; i < suzu.Length; i++)
            {
                temp += (suzu[i] - pingjunzhi) * (suzu[i] - pingjunzhi);
            }
            return Math.Sqrt(temp / suzu.Length);
        }

        #endregion


        /*******************************  删除shp文件组 *******************************************************/
        /// <summary>
        /// 删除shp文件组
        /// </summary>
        /// <param name="filePath"></param>
        public static void deleteFiles(string filePath)
        { 
            string files = filePath.Substring(0,filePath.LastIndexOf("."));
            System.IO.File.Delete(files + ".shp");
            System.IO.File.Delete(files + ".dbf");
            System.IO.File.Delete(files + ".shx");
            System.IO.File.Delete(files + ".qix");
            System.IO.File.Delete(files + ".prj");
            System.IO.File.Delete(files + ".img");
            System.IO.File.Delete(files + ".rrd");
            System.IO.File.Delete(files + ".cpg");
            
        }
        /*************************************************************************************************/



        /***************************读入SHP文件***************************/
        /// <summary>
        /// shpPolygon To List<Point[]>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<Point[]> shpPolygonToPointGL(string filePath)
        {
            List<Point[]> allFeature = new List<Point[]>();
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.Open(filePath, 0);
            OSGeo.OGR.Layer fileLayer = ds.GetLayerByIndex(0);

            int FeatCount = fileLayer.GetFeatureCount(0);
            for (int u = 0; u < FeatCount; u++)
            {
                OSGeo.OGR.Feature fileFeature = fileLayer.GetFeature(u);
                OSGeo.OGR.Geometry fileGeom = fileFeature.GetGeometryRef();
                OSGeo.OGR.Geometry subGeom = fileGeom.GetGeometryRef(0);
                int k = subGeom.GetPointCount();
                Point[] aFeat = new Point[k];
                for (int i = 0; i < k; i++)
                {
                    aFeat[i].X = subGeom.GetX(i);
                    aFeat[i].Y = subGeom.GetY(i);
                    aFeat[i].Z = subGeom.GetZ(i);
                }
                allFeature.Add(aFeat);
            }
            return allFeature;
        }



        /// <summary>
        /// shpPoint To Point[]
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Point[] shpPointtoPointG(string filePath)
        {
           var temp = new List<Point>();
           OSGeo.OGR.Ogr.RegisterAll();
           OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
           OSGeo.OGR.DataSource ds = dr.Open(filePath, 0);
           OSGeo.OGR.Layer fileLayer = ds.GetLayerByIndex(0);
           int FeatCount = fileLayer.GetFeatureCount(0);
           for (int i = 0; i < FeatCount; i++)
           {
               OSGeo.OGR.Feature fileFeature = fileLayer.GetFeature(i);
               OSGeo.OGR.Geometry fileGeom = fileFeature.GetGeometryRef();
               var newPoint = new Point();
               newPoint.X = fileGeom.GetX(0);
               newPoint.Y = fileGeom.GetY(0);
               newPoint.Z = fileGeom.GetZ(0);
               temp.Add(newPoint);
           }
           return temp.ToArray();
        }





        /************************* 输出SHP文件  *********************************/
        /// <summary>
        /// List<Point[]> To shpPolygon
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="allFeat"></param>
        public static void pointGLToShpPolygong(string savePath, List<Point[]> allFeat)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            for (int u = 0; u < allFeat.Count; u++)
            {
                Point[] a = allFeat[u];
                OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
                OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
                OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                for (int i = 0; i < a.Length; i++)
                {
                    subGeom.AddPoint(a[i].X, a[i].Y, a[i].Z);
                }
                fileGeom.AddGeometry(subGeom);
                fileFeat.SetGeometry(fileGeom);
                fileLayer.CreateFeature(fileFeat);
            }
            fileLayer.Dispose();
            ds.Dispose();
        }
        /// <summary>
        /// List<Point[]> To shpLine
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="allFeat"></param>
        public static void pointGtoshpLine(string savePath, List<Point[]> allFeat)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbLineString, null);
            for (int u = 0; u < allFeat.Count; u++)
            {
                Point[] a = allFeat[u];
                OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
                OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
                OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
                for (int i = 0; i < a.Length; i++)
                {
                   fileGeom.AddPoint(a[i].X, a[i].Y, a[i].Z);
                }
                fileFeat.SetGeometry(fileGeom);
                fileLayer.CreateFeature(fileFeat);
            }
            fileLayer.Dispose();
            ds.Dispose();
        }

        /// <summary>
        /// Point[] to shpPoint
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="allFeat"></param>
        public static void pointGToShpPoint(string savePath, Point[] allFeat)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPoint, null);
            for (int i = 0; i < allFeat.Length; i++)
            {
                OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
                OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
                OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPoint);
                fileGeom.AddPoint(allFeat[i].X, allFeat[i].Y, allFeat[i].Z);
                fileFeat.SetGeometry(fileGeom);
                fileLayer.CreateFeature(fileFeat);
            }
            fileLayer.Dispose();
            ds.Dispose();
        }

        /// <summary>
        /// List<LineD> to shpLine
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="aLine"></param>
        public static void LineDtoshpLine(string savePath,List< LineD> allLine)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbLineString, null);
            int a = allLine.Count;
            for (int i = 0; i < a; i++)
            {
                LineD aLine = allLine[i];
                OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
                OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
                OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
                fileGeom.AddPoint(aLine.Start.X, aLine.Start.Y, aLine.Start.Z);
                fileGeom.AddPoint(aLine.End.X, aLine.End.Y, aLine.End.Z);
                fileFeat.SetGeometry(fileGeom);
                fileLayer.CreateFeature(fileFeat);
            }
            fileLayer.Dispose();
            ds.Dispose();
        }


        /// <summary>
        /// LineD to shpLine
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="aLine"></param>
        public static void LineDtoshpLine(string savePath, LineD aLine)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbLineString, null);
            OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
            OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
            OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
            fileGeom.AddPoint(aLine.Start.X, aLine.Start.Y, aLine.Start.Z);
            fileGeom.AddPoint(aLine.End.X, aLine.End.Y, aLine.End.Z);
            fileFeat.SetGeometry(fileGeom);
            fileLayer.CreateFeature(fileFeat);
            fileLayer.Dispose();
            ds.Dispose();
        }
        /// <summary>
        /// Point[] to shp poly
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="allFeat"></param>
        public static void TEMPPointGtoshpPoly(string savePath, Point[] allFeat)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(savePath, null);
            OSGeo.OGR.Layer fileLayer = ds.CreateLayer("", null, OSGeo.OGR.wkbGeometryType.wkbPolygon, null);
            OSGeo.OGR.FeatureDefn FeatDf = new OSGeo.OGR.FeatureDefn("");
            OSGeo.OGR.Feature fileFeat = new OSGeo.OGR.Feature(FeatDf);
            OSGeo.OGR.Geometry fileGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
            OSGeo.OGR.Geometry subGeom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
            for (int i = 0; i < allFeat.Length; i++)
            {
                subGeom.AddPoint(allFeat[i].X, allFeat[i].Y, allFeat[i].Z);
            }
            fileGeom.AddGeometry(subGeom);
            fileFeat.SetGeometry(fileGeom);
            fileLayer.CreateFeature(fileFeat);
            fileLayer.Dispose();
            ds.Dispose();
        }

        /*************************************************************************************************/


        
        /*************************Point[] to X[] Y[]*********************************/
        public static double[] pointGroupToArrayGroup(Point[] PointGroup, string XorYorZ)
        {
            int groupCount = PointGroup.Length;
            double[] singleG = new double[groupCount];
            if (XorYorZ == "X")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].X;
                }
            }
            else if (XorYorZ == "Y")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].Y;
                }
            }
            else if (XorYorZ == "Z")
            {
                for (int i = 0; i < groupCount; i++)
                {
                    singleG[i] = PointGroup[i].Z;
                }
            }
            else
                return null;

            return singleG;
        }

        /*************************************************************************************************/


        /**************************************求交***************************************************/
        /// <summary>
        /// 两直线求交,返回交点 线公式 AX+BY+C1=0;
        /// </summary>
        /// <param name="lineA_z"></param>
        /// <param name="lineB"></param>
        /// <returns></returns>
        public static Point jiaoD(LineD lineA_z, LineD lineB)
        {
            //定义交点
            Point res = new Point();
            double X1 = 0, X2 = 0, X3 = 0, X4 = 0, Y1 = 0, Y2 = 0, Y3 = 0, Y4 = 0;
            if (Math.Abs(lineA_z.Start.X - lineA_z.End.X) > 0.01)
            {
                //lineA的两点坐标
                X1 = lineA_z.Start.X;
                Y1 = lineA_z.Start.Y;
                X2 = lineA_z.End.X;
                Y2 = lineA_z.End.Y;
                //lineB的两点坐标
                X3 = lineB.Start.X;
                Y3 = lineB.Start.Y;
                X4 = lineB.End.X;
                Y4 = lineB.End.Y;
            }
            else if (Math.Abs(lineB.Start.X - lineB.End.X) > 0.01)
            {
                //lineB的两点坐标
                X1 = lineB.Start.X;
                Y1 = lineB.Start.Y;
                X2 = lineB.End.X;
                Y2 = lineB.End.Y;
                //lineA的两点坐标
                X3 = lineA_z.Start.X;
                Y3 = lineA_z.Start.Y;
                X4 = lineA_z.End.X;
                Y4 = lineA_z.End.Y;
            }
            else
            {
               
               // MessageBox.Show("求交过程中被输入两条平行线,无法获得交点");
            }
            //lineA套入直线公式
            double A1 = Y2 - Y1;
            double B1 = X1 - X2;
            double C1 = X2 * Y1 - X1 * Y2;
            //lineB套入直线公式
            double A2 = Y4 - Y3;
            double B2 = X3 - X4;
            double C2 = X4 * Y3 - X3 * Y4;
            //A1*X+B1*Y+C1=0;
            //A2*X+B2*Y+C2=0;

            //解出交点

            // 1 提出lineA中的未知数Y
            //      Y*B1 = -A1*X - C1;
            //      Y = -X*A1/B1 - C1/B1;
            // 2 把Y 套入 lineB的公式中
            //      A2*X+B2*(-A1/B1 * X - C1/B1)+C2=0;
            // 3 提出lineB的公式中的未知数X
            //      A2*X + B2*(-X*A1/B1 - C1/B1) + C2=0;
            //      double g =B2*(A1/B1);
            //      double h =B2*(C1/B1);
            //      A2*X + (-B2*(A1/B1) * X - B2*(C1/B1)) + C2=0;
            //      A2*X + (-g * X - h) + C2=0;
            //      A2*X - g * X =h-C2;
            //      (A2-g)*X=h-C2;
            //      X=(h-C2)/(A2-g);
            res.X = (B2 * (C1 / B1) - C2) / (A2 - B2 * (A1 / B1));
            //4 把X套入步骤 1的公式
            res.Y = -res.X * A1 / B1 - C1 / B1;
            res.Z = lineA_z.Start.Z;
            return res;
        }
        /*************************************************************************************************/



        /****************************************  拟合函数  ************************************************/

        #region 拟合函数
        ///用最小二乘法拟合二元多次曲线
        ///</summary>
        ///<param name="arrX">已知点的x坐标集合</param>
        ///<param name="arrY">已知点的y坐标集合</param>
        ///<param name="length">已知点的个数</param>
        ///<param name="dimension">方程的最高次数</param>
        public static double[] MultiLine(double[] arrX, double[] arrY, int length, int dimension)//二元多次线性方程拟合曲线
        {
            int n = dimension + 1;                  //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1];      //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++)
            {
                int j;
                for (j = 0; j < n; j++)
                {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);
        }
        private static double SumArr(double[] arr, int n, int length) //求数组的元素的n次方的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }
        private static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length)
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;

        }
        private static double[] ComputGauss(double[,] Guass, int n)
        {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];
            for (i = 0; i < n; i++) x[i] = 0.0;//初始化

            for (j = 0; j < n; j++)
            {
                max = 0;
                k = j;
                for (i = j; i < n; i++)
                {
                    if (Math.Abs(Guass[i, j]) > max)
                    {
                        max = Guass[i, j];
                        k = i;
                    }
                }


                if (k != j)
                {
                    for (m = j; m < n + 1; m++)
                    {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;
                    }
                }
                if (0 == max)
                {
                    // "此线性方程为奇异线性方程" 
                    return x;
                }

                for (i = j + 1; i < n; i++)
                {
                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++)
                    {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);
                    }
                }

            }//结束for (j=0;j<n;j++)

            for (i = n - 1; i >= 0; i--)
            {
                s = 0;
                for (j = i + 1; j < n; j++)
                {
                    s = s + Guass[i, j] * x[j];
                }
                x[i] = (Guass[i, n] - s) / Guass[i, i];
            }
            return x;
        }
        //返回值是函数的系数
        //例如：y=a0+a1*x 返回值则为a0 a1
        //例如：y=a0+a1*x+a2*x*x 返回值则为a0 a1 a2
        #endregion

        /*************************************************************************************************/


        /************************************   生成等值线  **********************************************/

        public static string dzxPath(string filePath)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.GDAL.Gdal.AllRegister();

            //无效值
            double noDataValue;

            //0不使用无效值,1使用无效值
            int hasDataValue;

            //读入数据源
            OSGeo.GDAL.Dataset inData = OSGeo.GDAL.Gdal.Open(filePath, OSGeo.GDAL.Access.GA_ReadOnly);

            //分析数据源
            inData.GetRasterBand(1).GetNoDataValue(out noDataValue, out hasDataValue);

            double min, max, mean, std;
            inData.GetRasterBand(1).GetStatistics(0, 1, out min, out max, out mean, out std);

            int jianG = 2;

            int count = Convert.ToInt32((max - min) / jianG + 0.5);

            double[] shu = new double[count];

            for (int i = 0; i < count; i++)
            {
                shu[i] = min + jianG * i;
            }

            //创建空的SHP
            string dzSavePath = filePath.Substring(0, filePath.LastIndexOf(".")) + "_dzx.shp";
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            if (System.IO.File.Exists(dzSavePath))
                System.IO.File.Delete(dzSavePath);
            OSGeo.OGR.DataSource ds = dr.CreateDataSource(dzSavePath, null);
            OSGeo.OGR.Layer dzxLayer = ds.CreateLayer("等值线", null, OSGeo.OGR.wkbGeometryType.wkbMultiLineString, null);
            OSGeo.OGR.FieldDefn fieldDf0 = new OSGeo.OGR.FieldDefn("LID", OSGeo.OGR.FieldType.OFTInteger);
            OSGeo.OGR.FieldDefn fieldDf1 = new OSGeo.OGR.FieldDefn("EVE", OSGeo.OGR.FieldType.OFTReal);
            dzxLayer.CreateField(fieldDf0, 1);//ID
            dzxLayer.CreateField(fieldDf1, 1);//Value

            //Band(1), 间隔, 起始高度, 分段数量, 分段值数组, 是否有无效值, 无效值, 预置图层. ID字段, 高度值字段, null , null
            OSGeo.GDAL.Gdal.ContourGenerate(inData.GetRasterBand(1), jianG, min, count, shu, hasDataValue, noDataValue, dzxLayer, 0, 1, null, null);

            dzxLayer.Dispose();
            ds.Dispose();

            return dzSavePath;
        }

        /*************************************************************************************************/
    }
}
